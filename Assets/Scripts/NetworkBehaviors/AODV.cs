﻿//------------------------------------------------------------
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

public class AODV : MonoBehaviour, INetworkBehavior
{
    LoadOptionsGUI simValues;
    AODVGUI AODV_GUI;
    List<GameObject> neighbors;
    Hashtable currentRREQ;
    Hashtable routes;
    float active_route_timer;
    int nodeSeqNum;
    int broadcastID;

    //--------------------------------------Unity Functions---------------------------------------
    // Use this for initialization
    void Start()
    {
        simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
        AODV_GUI = GameObject.Find("Spawner").GetComponent<AODVGUI>();
        active_route_timer = 3.0f;  // used to delete route information;
        neighbors = new List<GameObject>();
        nodeSeqNum = 0;
        broadcastID = 0;
        currentRREQ = new Hashtable();
        routes = new Hashtable();

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
            RouteEntry r = (RouteEntry)revPath.Value;
            if (r.expirationTime < Time.time)
                routes.Remove(r.destination);
        }
    }
    void OnMouseDown()
    {
        AODV_GUI.source = gameObject;
        AODV_GUI.sourceStr = gameObject.name;
    }

    //--------------------------------------Custom Functions------------------------------------------
    public void addNeighbor(GameObject node)
    {
        if (!neighbors.Contains(node))
        {
            neighbors.Add(node);
        }
    }
    //public function to be called by nodeController if we need to remove a connection
    public void removeNeighbor(GameObject nodeID)
    {
        if (neighbors.Contains(nodeID))
        {
            neighbors.Remove(nodeID);
        }
    }

    public void recRREQ(RREQpacket dataIn){
        StartCoroutine(delayRecRREQ(dataIn));
    }

    IEnumerator delayRecRREQ(RREQpacket dataIn)
    {
        float distance = Vector3.Distance(gameObject.transform.position, dataIn.intermediate.transform.position);
        distance = distance / 2000;
        yield return new WaitForSeconds(distance);
        bool destFound = false;
        RouteEntry route;
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
                route = (RouteEntry)routes[dataIn.destination];
                //check to see if our route is stale...if so, don't use it.
                if (dataIn.dest_sequence_num <= route.dest_sequence_num)
                {
                    destFound = true;
                }
            }

            if (destFound)
            {
                if(simValues.foundTime==0)
                   simValues.foundTime=Time.time;
                //update routes table
                RevPath revEntry = new RevPath();
                revEntry.destination = dataIn.destination;
                revEntry.intermediate = dataIn.intermediate;
                revEntry.source = dataIn.source;
                revEntry.broadcast_id = dataIn.broadcast_id;
                revEntry.expTimer = Time.time + active_route_timer;
                revEntry.source_sequence_num = dataIn.source_seq;
                currentRREQ.Add(rreqStr, revEntry);

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
                reply.path =gameObject.name;
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
                currentRREQ.Add(rreqStr, revEntry);
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
                RouteEntry routeToSource;
                if (routes.Contains(dataIn.source))
                {
                   routeToSource = (RouteEntry)routes[dataIn.source];
                }
                else
                {
                    routeToSource = new RouteEntry();
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
   // public IEnumerator sendRREP(RREPpacket rrepPacket)
    {
        bool fwdRREP = false;
       // countdown--;
        string id = rrepPacket.source.name + "-" + rrepPacket.broadcast_id.ToString();
        if (currentRREQ.Contains(id))
        {

     //       RevPath revpath = new RevPath();
          
            RevPath  revpath = (RevPath)currentRREQ[id];
            GameObject returnP = revpath.intermediate;

            if (!revpath.replied)
                fwdRREP = true;
            if (revpath.replied)
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
                    simValues.foundTime = Time.time;
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
        StartCoroutine(delayRecRREP(dataIn));
    }

    //update route table with rrepPacket info
    void updateRouteFromRREP(RREPpacket rrepPacketIn)
    {
            if (routes.Contains(rrepPacketIn.destination))
            {
                RouteEntry path = (RouteEntry)routes[rrepPacketIn.destination];
                path.expirationTime = Time.time + active_route_timer;
            }

            else
            {
                if (!routes.Contains(rrepPacketIn.destination))
                {
                    RouteEntry path = new RouteEntry();
                    path.destination = rrepPacketIn.destination;
                    path.dest_sequence_num = rrepPacketIn.dest_sequence_num;
                    path.expirationTime = Time.time + active_route_timer;
                    path.numberHops = rrepPacketIn.hop_count;
                    path.nextHop = rrepPacketIn.intermediate;
                    routes.Add(rrepPacketIn.destination, path);
                }
            }
    }

    void updateRouteFromRREQ(RREQpacket rreqPacketIn)
    {
        if (routes.Contains(rreqPacketIn.destination))
        {
            RouteEntry path = (RouteEntry)routes[rreqPacketIn.destination];
            path.expirationTime = Time.time + active_route_timer;
        }
                    else
            {
                if (!routes.Contains(rreqPacketIn.destination))
                {
                    RouteEntry source = new RouteEntry();
                    source.dest_sequence_num = rreqPacketIn.source_seq;
                    source.destination = rreqPacketIn.source;
                    source.expirationTime = Time.time + active_route_timer;
                    source.nextHop = rreqPacketIn.intermediate;
                    source.numberHops = rreqPacketIn.hop_count;

                    //check again incase of race conditions 
                   if (!routes.Contains(rreqPacketIn.destination)){
                        routes.Remove(source.destination);
                        routes.Add(source.destination, source);
                        }
                    else updateRouteFromRREQ(rreqPacketIn);

                }
            }
    }

    public void sendMessage(MSGPacket packet)
    {
        StartCoroutine(delaySendMessage(packet));
    }



    IEnumerator delaySendMessage(MSGPacket packet)
    {
        bool haveRoute = false;
        if(routes.Contains(packet.destination)){
   //         print(gameObject.name + " has route");
            haveRoute = true;
            }
        else
        {
     //       print(gameObject.name + " doesn't have route");
            for (int i = 0; i <= packet.retries; packet.retries--)
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
                print("Error: TimeOut");
            }
        }
        if (haveRoute)
        {
            packet.sender = gameObject;
            RouteEntry route = (RouteEntry)routes[packet.destination];
            GameObject nextHop = (GameObject)route.nextHop;
    //        print(gameObject.name + " FWD MSG to: " + nextHop.name);
            nextHop.GetComponent<AODV>().recMessage(packet);
        }

    }

    public void recMessage(MSGPacket packet)
    {
           StartCoroutine(delayRecMessage(packet));
    }

    IEnumerator delayRecMessage(MSGPacket packet)
    {
        float distance = Vector3.Distance(gameObject.transform.position, packet.sender.transform.position);
        distance = distance / 2000;
        yield return new WaitForSeconds(distance);
     //   print(gameObject.name + " rev'd MSG");

        gameObject.renderer.material.color = Color.white;
        if (gameObject == packet.destination)
        {
            gameObject.renderer.material.color = Color.green;
            float tTime  = Time.time - packet.startTime;
            print(packet.message + "Time to Send: " + tTime );
        }
        else
        {
             sendMessage(packet);
        }
    }

    public void initMessage(GameObject destination)
    {
        MSGPacket packetToSend = new MSGPacket();
        packetToSend.destination = destination;
        packetToSend.message = " I am a test message";
        packetToSend.retries = (int) simValues.numNodes / 2;
        packetToSend.source = gameObject;
        packetToSend.startTime = Time.time;
        print(gameObject.name + " Initiating MSG to " + destination.name);

        sendMessage(packetToSend);
    }


    IEnumerator delayRecRREP(RREPpacket rrepPacketIn)
    {
        bool done = false;

        rrepPacketIn.path += " -> " + gameObject.name;

        //update routes
        RouteEntry entry = new RouteEntry();
        entry.destination = rrepPacketIn.destination;
        entry.dest_sequence_num = rrepPacketIn.dest_sequence_num;
        entry.expirationTime = Time.time + active_route_timer;
       // print("GO = " + gameObject.name + "Int = " + rrepPacketIn.intermediate);
        entry.nextHop = rrepPacketIn.intermediate;
        entry.numberHops = rrepPacketIn.hop_count;

        if (!routes.Contains(rrepPacketIn.destination))
        {
            routes.Add(rrepPacketIn.destination, entry);
        }
        else
        {
            RouteEntry temp = (RouteEntry)routes[rrepPacketIn.destination];
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

        float distance = Vector3.Distance(gameObject.transform.position, rrepPacketIn.intermediate.transform.position);
          distance = distance/2000;
        yield return new WaitForSeconds(distance);

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
            if (simValues.endTime == 0)
            {
                simValues.endTime = Time.time;
                float totalTime = simValues.endTime - simValues.startTime;
                RouteEntry routetoDest = (RouteEntry)routes[rrepPacketIn.destination];
                AODV_GUI.timeToFind = totalTime;
                AODV_GUI.numHops = routetoDest.numberHops;
               AODV_GUI.nextHop = routetoDest.nextHop.name;
               foreach (DictionaryEntry r in routes)
               {
                   RouteEntry rt = (RouteEntry)r.Value;
               }
            }
        }
        else
        {
            sendRREP(rrepPacketIn);
        }
    }

    public void discoverPath(GameObject node)
    {
        RREQpacket dataOut = new RREQpacket();
        dataOut.source = gameObject;
        dataOut.intermediate = gameObject;
        dataOut.destination = node;
        dataOut.broadcast_id = ++broadcastID;
        if (routes.Contains(node))
        {
            dataOut.dest_sequence_num = ((RouteEntry)routes[node]).dest_sequence_num;
        }
        else
        {
            dataOut.dest_sequence_num = 0;
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
        currentRREQ.Add(dataOut.source + "-" + dataOut.broadcast_id, revEntry);

        recRREQ(dataOut);
    }
 
    void printRoutes()
    {
        print("printing routes for " + gameObject.name + "-----------------");
        foreach (DictionaryEntry routePair in routes)
        {
            RouteEntry r = (RouteEntry)routePair.Value;
        }
    }
}

//---------------------------Structure used--------------------------------

public struct RouteEntry
{
    public GameObject destination;
    public GameObject nextHop;
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

public struct MSGPacket
{
    public GameObject source;
    public GameObject sender;
    public GameObject destination;
    public float startTime;
    public string message;
    public int retries;
}
;
