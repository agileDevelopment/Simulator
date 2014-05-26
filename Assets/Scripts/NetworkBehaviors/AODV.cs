//------------------------------------------------------------
//  Title: AODV
//  Date: 5-16-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: AODVGUI, NodeController
//  Description: Implements C.E. Perkins AODV dynamic network protocol.
//  Local connectivity management is handled by the sphere colider and .addNeighbor().  This is in place
//  of the Local connective management as outlined by Perkins.  
//
//  Implements: INetworkBehavior
//
//--------------------------------------------------------------


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AODV : Network
{
    Hashtable currentRREQ;
    float active_route_timer;
    int nodeSeqNum;
    int broadcastID;

    //--------------------------------------Unity Functions---------------------------------------
    // Use this for initialization
    void Start()
    {

        setValues(); // initialize parent class since its not added to the spawner...
        active_route_timer = 3.0f;  // used to delete route information;
        nodeSeqNum = 0;
        broadcastID = 0;
        currentRREQ = new Hashtable();
  
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        //clear requests if they have expired.
        Hashtable temp = (Hashtable)currentRREQ.Clone();
        foreach (DictionaryEntry revPath in temp)
        {
            RevPath r = (RevPath)revPath.Value;
            if (r.expTimer < Time.time)
               currentRREQ.Remove(r.source.name + "-" + r.broadcast_id);
        }

        //clear routes if they have expired.
        Hashtable temp2 = (Hashtable)routes.Clone();
        foreach (DictionaryEntry revPath in temp2)
        {
            AODVRouteEntry r = (AODVRouteEntry)revPath.Value;
            if (r.expirationTime < Time.time)
                routes.Remove(r.destination);
        }
    }
    void OnMouseDown()
    {
        netValues.source = gameObject;
        netValues.sourceStr = gameObject.name;
    }

    //--------------------------------------Custom Functions------------------------------------------


    IEnumerator delayRecRREQ(RREQpacket packet)
    {
        float distance = Vector3.Distance(gameObject.transform.position, packet.intermediate.transform.position);
        distance = distance / 2000;
        yield return new WaitForSeconds(distance);
        performRecRREQ(packet);
    }


    public void recRREQ(RREQpacket dataIn){
        if (netValues.useLatency)
            StartCoroutine(delayRecRREQ(dataIn));
        else
            performRecRREQ(dataIn);
    }

    void performRecRREQ(RREQpacket dataIn)
    {
        lock (nodeLock)
        {
            bool destFound = false;
            AODVRouteEntry route;
            string rreqStr = dataIn.source.name + "-" + dataIn.broadcast_id.ToString();
            //check to see if we already have a RREQ on record with same source and broadcast_id
            if (!currentRREQ.Contains(rreqStr))
            {
                // check to see if we are the destination... (shouldn't be here)
                if (gameObject == dataIn.destination)
                {
                    destFound = true;
                }
                //check to see if our neighbor is the destination
                else if (neighbors.Contains(dataIn.destination))
                {
                    destFound = true;

                    RREPpacket update = new RREPpacket();
                    update.broadcast_id = dataIn.broadcast_id;
                    update.dest_sequence_num = dataIn.dest_sequence_num;
                    update.destination = dataIn.destination;
                    update.hop_count = 1;
                    update.intermediate = dataIn.destination;
                    update.source = dataIn.source;
                    updateRouteFromRREP(update);

                }
                //if we have a route to the destination in our routing table
                else if (routes.Contains(dataIn.destination))
                {
                    route = (AODVRouteEntry)routes[dataIn.destination];
                    //check to see if our route is stale...if so, don't use it.
                    if (dataIn.dest_sequence_num <= route.dest_sequence_num)
                    {
                        destFound = true;
                    }
                }

                if (destFound)
                {
                    if (netValues.foundTime == 0)
                        netValues.foundTime = Time.time;
                    //update routes table
                    RevPath revEntry = new RevPath();
                    revEntry.destination = dataIn.destination;
                    revEntry.intermediate = dataIn.intermediate;
                    revEntry.source = dataIn.source;
                    revEntry.broadcast_id = dataIn.broadcast_id;
                    revEntry.expTimer = Time.time + active_route_timer;
                    revEntry.source_sequence_num = dataIn.source_seq;
                    lock (nodeLock)
                    {
                        currentRREQ.Add(rreqStr, revEntry);
                    }
                    dataIn.hop_count++;
                    dataIn.TTL--;

                    //craft RREP packet to return
                    RREPpacket reply = new RREPpacket();
                    reply.broadcast_id = dataIn.broadcast_id;
                    reply.dest_sequence_num = dataIn.destination.GetComponent<AODV>().nodeSeqNum;
                    reply.source = dataIn.source;
                    reply.destination = dataIn.destination;
                    reply.hop_count = 1;
                    reply.lifetime = 10;
                    reply.path = gameObject.name;
                    sendRREP(reply);
                }

                //not in our routes list and not a neighbor,  forward the request on, and add to our list of current requests
                else
                {

                    RevPath revEntry = new RevPath();
                    revEntry.destination = dataIn.destination;
                    revEntry.intermediate = dataIn.intermediate;
                    revEntry.source = dataIn.source;
                    revEntry.broadcast_id = dataIn.broadcast_id;
                    revEntry.expTimer = Time.time + active_route_timer;
                    revEntry.source_sequence_num = dataIn.source_seq;
                    revEntry.replied = false;
                    lock (nodeLock)
                    {
                        currentRREQ.Add(rreqStr, revEntry);
                    }
                    dataIn.hop_count++;
                    dataIn.TTL--;

                    //add route to source to our table
                    updateRouteFromRREQ(dataIn);
                    if (dataIn.TTL >= 0)
                        StartCoroutine("sendRREQ", dataIn);
                    //    sendRREQ(dataIn);
                }

                if (currentRREQ.Contains(rreqStr))
                {
                    AODVRouteEntry routeToSource;
                    if (routes.Contains(dataIn.source))
                    {
                        routeToSource = (AODVRouteEntry)routes[dataIn.source];
                    }
                    else
                    {
                        routeToSource = new AODVRouteEntry();
                        routeToSource.numberHops = 10000000;
                    }
                    if (dataIn.hop_count < routeToSource.numberHops)
                    {
                        routeToSource.dest_sequence_num = dataIn.source_seq;
                        routeToSource.destination = dataIn.source;
                        routeToSource.expirationTime = Time.time + active_route_timer;
                        routeToSource.nextHop = dataIn.intermediate;
                        routeToSource.numberHops = dataIn.hop_count;
                    }
                }
            }
        }
    }
    public void sendRREQ(RREQpacket dataOut)
    {
        string cameFrom = dataOut.intermediate.name;
        List<GameObject> temp = new List<GameObject>(neighbors);

        foreach (GameObject node in temp)
        {
            if (cameFrom != node.name && node.name != dataOut.destination.name)
            {
                dataOut.intermediate = gameObject;
                node.GetComponent<AODV>().recRREQ(dataOut);
               // node.GetComponent<AODV>().StartCoroutine("recRREQ", dataOut);

            }
        }
    }

    public void sendRREP(RREPpacket rrepPacket)
    {
        bool fwdRREP = false;
        string id = rrepPacket.source.name + "-" + rrepPacket.broadcast_id.ToString();
        if (currentRREQ.Contains(id))
        {
         
            RevPath  revpath = (RevPath)currentRREQ[id];
            GameObject returnP = revpath.intermediate;

            if (!revpath.replied)
            {
                fwdRREP = true;
            }
            else
            {
                if (rrepPacket.dest_sequence_num > revpath.dest_sequence_num)
                    fwdRREP = true;
                if (rrepPacket.hop_count < revpath.hop_count)
                    fwdRREP = true;
            }


            if (fwdRREP)
            {

                revpath.replied = true;
                revpath.dest_sequence_num = rrepPacket.dest_sequence_num;
                revpath.hop_count = rrepPacket.hop_count;

                if (gameObject == rrepPacket.source)
                {
                    netValues.foundTime = Time.time;
                }
                rrepPacket.hop_count++;
                updateRouteFromRREP(rrepPacket);
                rrepPacket.intermediate = gameObject;
                if(gameObject.renderer.material.color == Color.blue)
                    gameObject.renderer.material.color = Color.red;
                if(gameObject!= rrepPacket.source)
                    returnP.GetComponent<AODV>().recRREP(rrepPacket);
            }
        }
    }
    public void recRREP(RREPpacket dataIn)
    {
        if (netValues.useLatency)
            StartCoroutine(delayRecRREP(dataIn));
        else
            performRecRREP(dataIn);
    }

    IEnumerator delayRecRREP(RREPpacket packet)
    {

        float distance = Vector3.Distance(gameObject.transform.position, packet.intermediate.transform.position);
        distance = distance / 2000;
        yield return new WaitForSeconds(distance);
        performRecRREP(packet);
    }

    


    //update route table with rrepPacket info
    void updateRouteFromRREP(RREPpacket rrepPacketIn)
    {
            if (routes.Contains(rrepPacketIn.destination))
            {
                AODVRouteEntry path = (AODVRouteEntry)routes[rrepPacketIn.destination];
                path.expirationTime = Time.time + active_route_timer;
            }

            else
            {
                if (!routes.Contains(rrepPacketIn.destination))
                {
                    AODVRouteEntry path = new AODVRouteEntry();
                    path.destination = rrepPacketIn.destination;
                    path.dest_sequence_num = rrepPacketIn.dest_sequence_num;
                    path.expirationTime = Time.time + active_route_timer;
                    path.numberHops = rrepPacketIn.hop_count;
                    path.nextHop = rrepPacketIn.intermediate;
                    lock (nodeLock)
                    {
                        routes.Add(rrepPacketIn.destination, path);
                    }
                }
            }
    }

    void updateRouteFromRREQ(RREQpacket rreqPacketIn)
    {
        lock (nodeLock)
        {
            if (routes.Contains(rreqPacketIn.destination))
            {
                AODVRouteEntry path = (AODVRouteEntry)routes[rreqPacketIn.destination];
                path.expirationTime = Time.time + active_route_timer;
                updateRouteFromRREQ(rreqPacketIn);
            }
            else
            {
                if (!routes.Contains(rreqPacketIn.destination))
                {
                    AODVRouteEntry source = new AODVRouteEntry();
                    source.dest_sequence_num = rreqPacketIn.source_seq;
                    source.destination = rreqPacketIn.source;
                    source.expirationTime = Time.time + active_route_timer;
                    source.nextHop = rreqPacketIn.intermediate;
                    source.numberHops = rreqPacketIn.hop_count;

                        routes.Remove(source.destination);
                        routes.Add(source.destination, source);
 

                }
            }
        }
    }



    public override void sendMessage(MSGPacket packet)
    {
        StartCoroutine(delaySendMessage(packet));
    }

    IEnumerator delaySendMessage(MSGPacket packet)
    {
        bool haveRoute = false;
        lock (nodeLock)
        {
            if (routes.Contains(packet.destination))
            {
                //         print(gameObject.name + " has route");
                haveRoute = true;
            }
            else
            {
                //       print(gameObject.name + " doesn't have route");
                for (int i = 0; i <= packet.retries; i++)
                {
                    //         print(gameObject.name + " waiting for route");
                    discoverPath(packet.destination);
                    yield return new WaitForSeconds(.1f);
                    if (routes.Contains(packet.destination))
                    {
                        haveRoute = true;
                        //             print(gameObject.name + " has route");
                        break;
                    }

                }
                if (!haveRoute)
                {
                    print("Error: Retry TimeOut");
                }
            }
            if (haveRoute)
            {
                packet.sender = gameObject;
                AODVRouteEntry route = (AODVRouteEntry)routes[packet.destination];
                GameObject nextHop = (GameObject)route.nextHop;
                if (nextHop != null)

                    if (packet.TTL > 0)
                    {
                        packet.TTL--;
                        nextHop.GetComponent<AODV>().recMessage(packet);
                    }
                    else
                    {
                        print("Error: TTL TimeOut");
                    }
            }
        }

    }

    protected override void performRecMessage(MSGPacket packet)
    {
        gameObject.renderer.material.color = Color.white;
        if (gameObject == packet.destination)
        {
            gameObject.renderer.material.color = Color.green;
      //      float tTime  = Time.time - packet.startTime;
      //      print(packet.message + "Time to Send: " + tTime );
        }
        else
        {
             sendMessage(packet);
        }
    }



    void performRecRREP(RREPpacket rrepPacketIn)
    {
        bool done = false;

        rrepPacketIn.path += " -> " + gameObject.name;

        //update routes
        AODVRouteEntry entry = new AODVRouteEntry();
        entry.destination = rrepPacketIn.destination;
        entry.dest_sequence_num = rrepPacketIn.dest_sequence_num;
        entry.expirationTime = Time.time + active_route_timer;
       // print("GO = " + gameObject.name + "Int = " + rrepPacketIn.intermediate);
        entry.nextHop = rrepPacketIn.intermediate;
        entry.numberHops = rrepPacketIn.hop_count;
        lock (nodeLock)
        {
            if (!routes.Contains(rrepPacketIn.destination))
            {
                routes.Add(rrepPacketIn.destination, entry);
            }

            else
            {
                AODVRouteEntry temp = (AODVRouteEntry)routes[rrepPacketIn.destination];
                if (temp.dest_sequence_num < entry.dest_sequence_num)
                {
                    temp.nextHop = entry.nextHop;
                    temp.numberHops = entry.numberHops;
                    temp.dest_sequence_num = entry.dest_sequence_num;
                }
                if ((temp.dest_sequence_num == entry.dest_sequence_num) && (temp.numberHops > entry.numberHops))
                {
                    temp.nextHop = entry.nextHop;
                    temp.numberHops = entry.numberHops;
                    temp.dest_sequence_num = entry.dest_sequence_num;
                }

                routes.Remove(rrepPacketIn.destination);
                routes.Add(rrepPacketIn.destination, entry);

            }
        }
        if (gameObject == rrepPacketIn.source)
        {
            done = true;
        }

        string id = rrepPacketIn.source.name + "-" + rrepPacketIn.broadcast_id.ToString();
        if (currentRREQ.Contains(id))
        {
            RevPath revpath = new RevPath();
            revpath = (RevPath)currentRREQ[id];
            if (!revpath.replied)
               revpath.replied = true;
            
        }
             
        if (done)
        {
            if (netValues.endTime == 0)
            {
                netValues.endTime = Time.time;
                float totalTime = netValues.endTime - netValues.startTime;
                AODVRouteEntry routetoDest = (AODVRouteEntry)routes[rrepPacketIn.destination];
                netValues.timeToFind = totalTime;
                netValues.numHops = routetoDest.numberHops;
               netValues.nextHop = routetoDest.nextHop.name;
            }
        }
        else
        {
            sendRREP(rrepPacketIn);
        }
    }

    public override void discoverPath(GameObject node)
    {
        RREQpacket dataOut = new RREQpacket();
        dataOut.source = gameObject;
        dataOut.intermediate = gameObject;
        dataOut.destination = node;
        dataOut.broadcast_id = ++broadcastID;
        lock (nodeLock)
        {
            if (routes.Contains(node))
            {
                dataOut.dest_sequence_num = ((AODVRouteEntry)routes[node]).dest_sequence_num;
            }
            else
            {
                dataOut.dest_sequence_num = 0;
            }
        }
        dataOut.hop_count = 0;
        dataOut.source_seq = nodeSeqNum;
        dataOut.TTL = (int)simValues.numNodes / 2;

        RevPath revEntry = new RevPath();
        revEntry.destination = dataOut.destination;
        revEntry.intermediate = dataOut.intermediate;
        revEntry.source = dataOut.source;
        revEntry.broadcast_id = dataOut.broadcast_id;
        revEntry.expTimer = Time.time + active_route_timer;
        revEntry.source_sequence_num = dataOut.source_seq;
        revEntry.replied = false;
        lock (nodeLock)
        {
            currentRREQ.Add(dataOut.source + "-" + dataOut.broadcast_id, revEntry);
        }
        recRREQ(dataOut);
    }
}

//---------------------------Structure used--------------------------------

public class AODVRouteEntry: RouteEntry
{
    public int numberHops;
    public int dest_sequence_num;
    public int activeNeighbors;
    public float expirationTime;
}


public struct RREQpacket
{
    public GameObject source;
    public GameObject intermediate;
    public int source_seq;
    public int broadcast_id;
    public GameObject destination;
    public int dest_sequence_num;
    public int hop_count;
    public int TTL;
}


public struct RREPpacket
{
    public string path;
    public int broadcast_id;
    public GameObject source;
    public GameObject destination;
    public GameObject intermediate;
    public int dest_sequence_num;
    public int hop_count;
    public int lifetime;
}



public struct RevPath
{
    public int hop_count;
    public int dest_sequence_num;
    public bool replied;
    public GameObject source;
    public GameObject destination;
    public GameObject intermediate;
    public int broadcast_id;
    public float expTimer;
    public int source_sequence_num;
}
;
