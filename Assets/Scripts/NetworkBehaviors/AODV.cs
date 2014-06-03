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
    public Dictionary<string, RevPath> currentRREQ;
    protected float active_route_timer;
    protected int nodeSeqNum;
    public int broadcastID;
    public int delayFactor;
    public int messageQueue=0;
    GUIText mCountText;

    

    //--------------------------------------Unity Functions---------------------------------------
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        netValues = simValues.networkGUI;
        delayFactor = 4000;
        active_route_timer = 5.0f;  // used to delete route information;
        nodeSeqNum = 0;
        broadcastID = 0;
        currentRREQ = new Dictionary<string, RevPath>();

   
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        messageQueue = currentRREQ.Count;
        if (gameObject.GetComponent<NodeController>().selected)
        {
            netValues.myUIElements["messageQueue"] = "# of mess:" + messageQueue.ToString();
        }
        netValues.myUIElements["Tot Messages"] = "Tot# Mess: " + netValues.messageCounter.ToString();

    }
    protected override void LateUpdate()
    {        //clear requests if they have expired.
        Dictionary<string, RevPath> temp = new Dictionary<string, RevPath>(currentRREQ);
        foreach (RevPath r in temp.Values)
        {
            if (r.expTimer < Time.time)
            {
                if (currentRREQ.Count == 1)
                    currentRREQ.Clear();
                else
                    currentRREQ.Remove(r.source.name + "-" + r.broadcast_id);      
            }

           
        }

        //clear routes if they have expired.
        Dictionary<GameObject, RouteEntry> temp2 = new Dictionary<GameObject, RouteEntry>(routes);
        foreach (AODVRouteEntry entry in temp2.Values)
        {
            AODVRouteEntry r = entry;

            if (r.expirationTime < Time.time)
                routes.Remove(r.destination);
        }

    }


    //--------------------------------------Custom Functions------------------------------------------

    IEnumerator delayRecRREQ(RREQpacket packet)
    {
        float distance = Vector3.Distance(gameObject.transform.position, packet.intermediate.transform.position);
        distance = distance / delayFactor;
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
            if (!currentRREQ.ContainsKey(rreqStr))
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
                else if (routes.ContainsKey(dataIn.destination))
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

                if (currentRREQ.ContainsKey(rreqStr))
                {
                    AODVRouteEntry routeToSource;
                    if (routes.ContainsKey(dataIn.source))
                    {
                        routeToSource = (AODVRouteEntry)routes[dataIn.source];
                    }
                    else
                    {
                        routeToSource = new AODVRouteEntry();
                        routeToSource.hop_count = 10000000;
                    }
                    if (dataIn.hop_count < routeToSource.hop_count)
                    {
                        routeToSource.dest_sequence_num = dataIn.source_seq;
                        routeToSource.destination = dataIn.source;
                        routeToSource.expirationTime = Time.time + active_route_timer;
                        routeToSource.nextHop = dataIn.intermediate;
                        routeToSource.hop_count = dataIn.hop_count;
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
                netValues.messageCounter++;
                node.GetComponent<AODV>().recRREQ(dataOut);
               // node.GetComponent<AODV>().StartCoroutine("recRREQ", dataOut);

            }
        }
    }

    public void sendRREP(RREPpacket rrepPacket)
    {
        bool fwdRREP = false;
        string id = rrepPacket.source.name + "-" + rrepPacket.broadcast_id.ToString();
        if (currentRREQ.ContainsKey(id))
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
                if (gameObject != rrepPacket.source)
                {
                    netValues.messageCounter++;
                    returnP.GetComponent<AODV>().recRREP(rrepPacket);
                }
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
        distance = distance / delayFactor;
        yield return new WaitForSeconds(distance);
        performRecRREP(packet);
    }

    


    //update route table with rrepPacket info
    void updateRouteFromRREP(RREPpacket rrepPacketIn)
    {
        AODVRouteEntry path = new AODVRouteEntry();
        path.destination = rrepPacketIn.destination;
        path.dest_sequence_num = rrepPacketIn.dest_sequence_num;
        path.expirationTime = Time.time + active_route_timer;
        path.hop_count = rrepPacketIn.hop_count;
        path.nextHop = rrepPacketIn.intermediate;

        updateRoutes(path);
    
    }

    void updateRouteFromRREQ(RREQpacket rreqPacketIn)
    {
        AODVRouteEntry source = new AODVRouteEntry();
        source.dest_sequence_num = rreqPacketIn.source_seq;
        source.destination = rreqPacketIn.source;
        source.expirationTime = Time.time + active_route_timer;
        source.nextHop = rreqPacketIn.intermediate;
        source.hop_count = rreqPacketIn.hop_count;

        updateRoutes(source);
   
    }

    protected void updateRoutes(AODVRouteEntry route)
    {
        lock (nodeLock)
        {
            if (routes.ContainsKey(route.destination))
            {
                AODVRouteEntry temp = (AODVRouteEntry)routes[route.destination];
                if (route.dest_sequence_num > temp.dest_sequence_num)
                {
                    AODVRouteEntry path = (AODVRouteEntry)routes[route.destination];
                    path.expirationTime = Time.time + active_route_timer;
                }
            }
            else
            {
                    routes.Remove(route.destination);
                    routes.Add(route.destination, route);
                }
            }
        
    }

    IEnumerator delaySendMessage(MSGPacket packet)
    {
        bool haveRoute = false;
        lock (nodeLock)
        {
            if (routes.ContainsKey(packet.destination))
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
                    if (routes.ContainsKey(packet.destination))
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
                        netValues.messageCounter++;
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
        bool fwd = true;
        if (gameObject == packet.destination)
        {
            if (packet.messageType == "mess")
            {
                //do something
            }

            else if (packet.messageType == "cmd")
            {
                if (packet.message == "ping")
                {
                    initMessage(packet.source, "cmd", "ack");
                }
                if (packet.message == "ack")
                {

                }
            }

        }
        else if (fwd)
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
        entry.hop_count = rrepPacketIn.hop_count;
        lock (nodeLock)
        {
            if (!routes.ContainsKey(rrepPacketIn.destination))
            {
                routes.Add(rrepPacketIn.destination, entry);
            }

            else
            {
                AODVRouteEntry temp = (AODVRouteEntry)routes[rrepPacketIn.destination];
                if (temp.dest_sequence_num < entry.dest_sequence_num)
                {
                    temp.nextHop = entry.nextHop;
                    temp.hop_count = entry.hop_count;
                    temp.dest_sequence_num = entry.dest_sequence_num;
                }
                if ((temp.dest_sequence_num == entry.dest_sequence_num) && (temp.hop_count > entry.hop_count))
                {
                    temp.nextHop = entry.nextHop;
                    temp.hop_count = entry.hop_count;
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
        if (currentRREQ.ContainsKey(id))
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
                netValues.numHops = routetoDest.hop_count;
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
            if (routes.ContainsKey(node))
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
        dataOut.TTL = (int)simValues.numNodes;

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
    public int hop_count;
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
