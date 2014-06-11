//-----------------Header-------------------------
//  Title: ACOVB.cs
//  Date: 5-30-2014
//  Version: 3.4
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: AODV, ACOVBGUI

//  Description: Implements the ACO Virtual Backbone Algorithm.
//
//  Extends AODV (which Implements INetworkBehavior)
//
//--------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACOVB : AODV
{
    #region Fields
    //Unity Objects
    GameObject superVisor;
    public GameObject myCDSconnection;

    //Custom Classes
    new ACOVBGUI netValues;
    public CDS myCurrentCDS;
    public CDS tempCDS;

    //System Generics
    public Dictionary<GameObject, ACORouteEntry> antRoutes;
    Dictionary<string, CDSAnt> antUpdates;
    List<GameObject> cdsConnections;

    //strings and primatives
    public float myPhereLevel;
    float startTime = 0;
    public float lastUpdateTime = 0f;
    public int connectedNodes = 0;
    public int counter=0;
    public bool memberOfCDS = false;
    public CDSAnt bestAnt;
    bool updateCDS = true;

    #endregion 

    //-------------------------Unity Functions--------------------------------------
    #region Unity Functions
    // Use this for initialization
    protected override void Start()
    {

        base.Start();
        netValues = (ACOVBGUI)simValues.networkGUI;
        superVisor = netValues.supervisor;
        myCDSconnection = null ;
        myCurrentCDS = null;
        tempCDS = null;
        antRoutes = new Dictionary<GameObject, ACORouteEntry>();
        antUpdates = new Dictionary<string, CDSAnt>();
        cdsConnections = new List<GameObject>();
        myPhereLevel = 1f;
        startTime = Time.time;
        bestAnt = null;
        
    }
  	

	// Update is called once per frame
    protected override void Update()
    {
        if (netValues.supervisor == gameObject && netValues.enableCDS )
        {
            if (Time.time > startTime + 2.9f && updateCDS)
            {
                if (tempCDS != null) {
                myCurrentCDS = new CDS(tempCDS);
                netValues.currentCDS = myCurrentCDS;
                }

                if (myCurrentCDS != null)
                {
                    print(myCurrentCDS.getInCDS().Count);
                    updateCDS = false;

                    foreach (GameObject neighbor in neighbors)
                    {
                        if (myCurrentCDS.getInCDS().Contains(neighbor))
                        {
                            neighbor.GetComponent<ACOVB>().sendBackAnt(bestAnt);
                        }
                    }
                }

            }  
         //   netValues.currentCDS = myCurrentCDS;
            if (Time.time > startTime + 3f)
            {
                startTime = Time.time;
                updateCDS = true;
                initBroadcast("cmd", "genCDS");

            }
        }
        base.Update();
        counter++;

        //if the current node is selected, show current pheremone levels
        if (netValues.source == gameObject)
        {
            netValues.myUIElements["pLevel"] = "Phereomon: " + myPhereLevel.ToString();
        }
	}


    //LateUpdate is called once per frame after Update
    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (counter % 5 == 0)
        {
            pollNeighbors();
        }
        if (memberOfCDS)
        {
            gameObject.renderer.material.color = Color.black;
        }
        else
        {
            gameObject.renderer.material.color = Color.grey;
        }
        if(netValues.enableTest)
          if (counter % ((gameObject.GetComponent<NodeController>().idNum+1)*2)==0){
               gameObject.GetComponent<ACOVB>().initBroadcast("cmd", "bCast recv");
           }
        if(counter==simValues.numNodes*2)
            counter = 0;
    }
    #endregion
    //---------------------------Utility Functions----------------------------------
    #region Utility



    #endregion 

        //--------------------------Message Handlers--------------------------------------
    #region Message Handling

    //------------------------------------------------------------
    //  Function: performRecMessage()
    //  Date: 6-7-2014
    //  Version: 3.2
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //  
    //  Class Dependicies: MSGPacket
    //
    //  Description:  Decides what to do with a message packet when recieved.  Decrements
    //  the TTL counter.  Only acts on the packet if it hasn't seen it before.
    //
    //--------------------------------------------------------------
    protected override void performRecMessage(MSGPacket packet)
    {
        //if I haven't seen this packet before or if I'm the final destination.  The second check is
        //required due to the handling of broadcast traffic. (It will process it as a broadcast first, then
        // as a direct message to itself.
        if (!messages.ContainsKey(packet.id) || gameObject == packet.destination)
        {
            packet.TTL--;

            if(!messages.ContainsKey(packet.id))
              messages.Add(packet.id, packet);
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
                    else if (packet.message == "ack")
                    {

                    }
                    else if (packet.message == "genCDS")
                    {
                        generateAntCDS();
                    }
                    else if (packet.message == "bCast recv")
                    {
                        //update our logdata so we can track it.
                        netValues.updateLogData((Time.time - packet.startTime), (packet.MaxTTL - packet.TTL));
                    }
                    else if (packet.message == "updateCDS_remove")
                    {
                        if (myCurrentCDS != null)
                        {
                          if(myCurrentCDS.getInCDS().Contains(packet.source))
                               myCurrentCDS.moveToEdgeCDS(packet.source);
                        }
                    }
                    else if (packet.message == "cdsAddRequest")
                    {
                        if (myCurrentCDS != null)
                        {
                            if (!myCurrentCDS.getInCDS().Contains(packet.source))
                            {
                                myCurrentCDS.moveToInCDS(gameObject);
                                memberOfCDS = true;
                                initBroadcast("cmd", "updateCDS_add");
                            }
                        }
                    }
                    else if (packet.message == "updateCDS_add")
                    {
                        if (myCurrentCDS != null)
                        {
                            if (myCurrentCDS.getEdgeCDS().Contains(packet.source))
                                myCurrentCDS.moveToInCDS(packet.source);
                        }
                    }
                }
                else  // we aren't the target destination, forward it on....
                {
                    sendMessage(packet);
                }
            }
        }
    }
    //------------------------------------------------------------
    //  Function: performBroadcast()
    //  Date: 6-7-2014
    //  Version: 3.2
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //  
    //  Class Dependicies: MSGPacket, AODV
    //  
    //  Description:  Overrides handling of performBroadcasts due to added check for CDS.
    //  If there is a CDS available it will send it out via its CDS node, otherwise it will
    //  process it in the same manner as the AODV broadcast.
    //
    //--------------------------------------------------------------

    //we are overriding AODV performBroadcast method...
    protected override void performBroadcast(MSGPacket packet)
    {
        //have I seen this broadcast?
        if (!messages.ContainsKey(packet.id))
        {
            netValues.broadcastCounter++;
            packet.TTL--;

            //is a CDS currently Active?
            if (netValues.enableCDS)
            {
                messages.Add(packet.id, packet);
                //Am I am member of the current CDS?
                if (memberOfCDS || gameObject==superVisor)
                {   //Send it on to all of my neighbors       
                    foreach (GameObject neighbor in neighbors)
                    {
                        packet.sender = gameObject;
                        neighbor.GetComponent<ACOVB>().sendBroadcast(packet);
                    }

                }
                //make me the destination and send it to myself.
                packet.destination = gameObject;
                performRecMessage(packet);

            }
            else //CDS isn't active, so broadcast regularlly
            {          
                    messages.Add(packet.id, packet);
                    foreach (GameObject neighbor in neighbors)
                    {
                        packet.sender = gameObject;
                        neighbor.GetComponent<ACOVB>().sendBroadcast(packet);

                    }
                    packet.destination = gameObject;
                    performRecMessage(packet);
                }
        }
        else
        {
            //do nothing, already seen.
        }
    }

    //update routing table data...
    protected void updateRoutes(ACORouteEntry route)
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

    #endregion

        //----------------------------Algorithm Functions--------------------------------
    #region Algorithm functions

    public override void addNeighbor(GameObject node)
    {
        base.addNeighbor(node);
        if(!routes.ContainsKey(node)){

        ACORouteEntry nRoute = new ACORouteEntry();
         nRoute.destination = node;
          nRoute.nextHop = node;
         nRoute.messageQueue = node.GetComponent<ACOVB>().currentRREQ.Count;
        nRoute.pheremoneLevel = node.GetComponent<ACOVB>().myPhereLevel;

        routes.Add(node, nRoute);
        
        }

    }
    //public function to be called by nodeController if we need to remove a connection
    public override void removeNeighbor(GameObject node)
    {
        base.removeNeighbor(node);
        if (routes.ContainsKey(node))
            routes.Remove(node);

    }

    //iterates through neighbors and updates list of pheremone levels
    protected void pollNeighbors()
    {
   //     lock (nodeLock)
        {
            connectedNodes = 0;
            foreach (GameObject neighbor in neighbors)
            {
                //check to see if I have a route entry for my neighbor and add one if I don't...
                if (!antRoutes.ContainsKey(neighbor))
                {
                    antRoutes.Add(neighbor, new ACORouteEntry());
                }
                antRoutes[neighbor].messageQueue = neighbor.GetComponent<ACOVB>().currentRREQ.Count;
                antRoutes[neighbor].pheremoneLevel = neighbor.GetComponent<ACOVB>().myPhereLevel;



                //update list of cds connections...
                if (neighbor.GetComponent<ACOVB>().memberOfCDS)
                {
                    if (!cdsConnections.Contains(neighbor))
                    {
                        cdsConnections.Add(neighbor);
                        if (myCurrentCDS != null)
                            if (!myCurrentCDS.getInCDS().Contains(neighbor))
                            {
                                myCurrentCDS.moveToInCDS(neighbor);

                            }
                    }
                    else
                    {
                        if (cdsConnections.Contains(neighbor))
                        {
                            cdsConnections.Remove(neighbor);
                        }

                    }
                }
                if (memberOfCDS)
                {
                    if (neighbor.GetComponent<ACOVB>().myCDSconnection == gameObject)
                        connectedNodes++;
                }
            }


            if (myCurrentCDS != null)
            {
                //if I'm a member of the current CDS
                if (memberOfCDS)
                {
                    //I don't have anyone connected to me, so remove me from the current CDS
                    if (connectedNodes == 0 && cdsConnections.Count ==0)
                    {
                        memberOfCDS = false;
                        initBroadcast("cmd", "updateCDS_remove");
                    }
                }
                else//I'm not a member of the current CDS
                {
                    float minDistToCDS = netValues.nodeCommRange;
                    GameObject newCDSNode = null;
                    float distance = 0;

                    //get Closest CDS Node

                        foreach (GameObject cdsNode in cdsConnections)
                        {
                            distance = Vector3.Distance(gameObject.transform.position, cdsNode.transform.position);
                            if (distance < minDistToCDS)
                            {
                                minDistToCDS = distance;
                                newCDSNode = cdsNode;
                            }
                        }
                        myCDSconnection = newCDSNode;

                    if(myCDSconnection==null){
                        repairCDS();
                    }
                    
                }

                //check when polling for change in topology
                List<GameObject> temp = new List<GameObject>(cdsConnections);

                foreach (GameObject node in temp)
                {
                    if (!neighbors.Contains(node))
                    {
                        cdsConnections.Remove(node);

                        if (myCurrentCDS != null)
                        {

                            if (myCurrentCDS.getInCDS().Contains(node))
                            {
                                myCurrentCDS.moveToOutCDS(node);
                            }
                        }
                    }
                }
            }
        }
    }
    int getRREQCount(){
        return currentRREQ.Count;
    }

    void repairCDS()
    {
        float maxPheromone = 0f;
        GameObject nodeToRequest = null;

        //get our neighbor with the highest pheremone level
        foreach (KeyValuePair<GameObject, ACORouteEntry> entry in antRoutes)
        {
            if (neighbors.Contains(entry.Key))
            {
                if (entry.Value.pheremoneLevel > maxPheromone)
                {
                    maxPheromone = entry.Value.pheremoneLevel;
                    nodeToRequest = entry.Key;
                }
            }
        }
        if (nodeToRequest)
        {
            initMessage(nodeToRequest, "cmd", "cdsAddRequest");
        }
        else if (nodeToRequest == null)
        {
            initMessage(superVisor, "cmd", "resetCDS");
            myCurrentCDS = null;
        }

    }

    //------------------------------------------------------------
    //  Function: checkFeasibility
    //  Algorithm: MCDS - GA
    //  Date: 5-28-2014
    //  Version: 3
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //
    //  Class Dependicies: Set of CDS 
    //
    //   Order-of: O(n^2) although actual implementation will be substantially less as 
    //   the loops are broken on first neighbor that meets criteria.
    //
    //  Description:  Takes a CDS to check for validity.  It does this by picking a node from the CDS's InCDS list
    //  and then then moving it's neighbors that are in th InCDS to the closed list.  When there are no more neighbors
    //  The process takes a node from closed, removes it from the closed list, and then checks its neighbors and adds them to closed
    //  if they are in the InCDS list.  This process continues until there are no more nodes left to check.  If any nodes remain in open
    //  this means that there is a disconnect in our CDS and it the solution will be rejected.
    //
    //  Crossover rate is based on parameter set by user.  The algorithm alternates between uniform crossover
    //  and 1-point cross over to increase chance of diversity.
    //
    //--------------------------------------------------------------
    public bool checkFeasibility(CDS CDStoCheck)
    {

        List<GameObject> openList = new List<GameObject>(CDStoCheck.getInCDS());
        List<GameObject> closedList = new List<GameObject>();
        GameObject node = null;
        if (openList.Count > 0)
        {
            node = openList[0];
        }
        else return false;
        openList.Remove(node);
        closedList.Add(node);
        while (openList.Count > 0)
        {

            foreach (GameObject neighbor in node.GetComponent<ACOVB>().neighbors)
            {
                if (openList.Contains(neighbor))
                {
                    openList.Remove(neighbor);
                    closedList.Add(neighbor);
                }
            }
            if (openList.Count > 0)
            {
                if (closedList.Count > 0)
                {
                    node = closedList[0];
                    closedList.Remove(node);
                }
                else
                {
                    return false;

                }
            }
        }

        return true;
    }


    //ANTCDS Build
    #region Build Ant CDS
    public void generateAntCDS(){
        if (netValues.enableCDS)
        {
            if (!netValues.runningCDSs.ContainsKey(gameObject.name + " - " + broadcastID.ToString()))
            {
                broadcastID++;
                CDSAnt myAnt = new CDSAnt(gameObject);
                myAnt.label = gameObject.name + " - " + broadcastID.ToString();
                myAnt.expirationTime = Time.time + active_route_timer;
                netValues.runningCDSs.Add(myAnt.label, Time.time + active_route_timer);
                performSendCDSAnt(myAnt);
                
            }
        }
    }
    //case 1 is building CDS
    //case 2 is reporting back to supervisor
    public void sendCDSAnt(CDSAnt ant, int flag)
    {
        switch (flag) { 
            case 1:
            if (!antUpdates.ContainsKey(ant.label))
            {
                broadcastID++;
                if (netValues.useLatency)
                    StartCoroutine(delaySendCDSAnt(ant));
                else
                    performSendCDSAnt(ant);
            }
                 break;
            case 2:
                 if (!antUpdates.ContainsKey(ant.label))
                 {
                     CDSAnt newAnt = new CDSAnt(ant);
                     antUpdates.Add(ant.label, newAnt);
                 }
                 if (antUpdates.ContainsKey(ant.label))
                 {
                     antUpdates[ant.label].done = true;
                     broadcastID++;
                     if (netValues.useLatency)
                         StartCoroutine(delaySendCDSAnt(ant));
                     else
                         performSendCDSAnt(ant);
                 }

                 break;
    }
    }

    IEnumerator delaySendCDSAnt(CDSAnt ant)
    {
        float distance = Vector3.Distance(gameObject.transform.position, ant.nodeData[ant.hop_count].previous.transform.position);
        distance = distance / delayFactor;
        yield return new WaitForSeconds(distance);
        performSendCDSAnt(ant);
    }

    void performSendCDSAnt(CDSAnt ant)
    {
        {
            if (!ant.done)
            {

                // memberOfCDS = true;
                if (ant.myCDS.getInCDS().Count == 0)
                    ant.myCDS.owner = gameObject;

                //Initialize heuristic values
                GameObject nextHop = null;
                float tempProb = 0;
                float totalTau = 0;
             //   float qualityValue = 0;

                //CDS build isn't finished...
                if (ant.myCDS.getOutCDS().Count > 0)
                {

                    //add this node the the CDS list
                    ant.myCDS.moveToInCDS(gameObject);
                    foreach (GameObject neighbor in neighbors)
                    {
                        //maintain our tau_j and N_j levels
                        if (!ant.canidates.ContainsKey(neighbor))
                        {
                            ant.myCDS.moveToEdgeCDS(neighbor);
                            NeighborData nData = new NeighborData();
                            nData.degree = neighbor.GetComponent<ACOVB>().neighbors.Count;
                            nData.tau = neighbor.GetComponent<ACOVB>().myPhereLevel;
                            ant.canidates.Add(neighbor, nData);
                        }
                        else
                        {
                            ant.canidates[neighbor].degree--;// = nData;
                        }

                    }
                }
                //if we still aren't done...
                if (ant.myCDS.getOutCDS().Count > 0)
                {
                    float maxTauN = 0;

                    float rand = (float)Random.Range(0, 100) / 100;

                    //choose whether to explore or exploit
                    if (rand > ant.exploreRate)//we are exploring
                    //choose a neighbor edge to explore rather than jumping across the graph
                    {
                        
                        foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                        {
                            float arg = 0;
                            arg = ant.canidates[canidate].degree * ant.canidates[canidate].tau;
                            if (arg > maxTauN)
                            {
                                maxTauN = arg;
                                nextHop = canidate;
                            }
                        }
                    }
                    else//we are exploiting
                    {
                    //    rand = (float)Random.Range(0, 100) / 100;
                    //    foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                    //    {
                    //        totalTau += (ant.canidates[canidate].tau * ant.canidates[canidate].degree);
                    //    }

                    //    foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                    //    {
                    //        if (neighbors.Contains(canidate))
                    //        {
                    //            float numer = ant.canidates[canidate].tau * ant.canidates[canidate].degree;
                    //            tempProb += (float)(numer / totalTau);

                    //            if (rand < tempProb)
                    //            {
                    //                nextHop = canidate;
                    //                qualityValue = (float)(numer / totalTau);
                    //            }
                    //        }
                    //    }
                    //}

                    ////out of direct hops, choose a random edge
                    //if (nextHop == null)
                    //{
                        foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                        {
                            foreach (GameObject neighbor2 in canidate.GetComponent<ACOVB>().neighbors)
                            {
                                if (ant.myCDS.getOutCDS().Contains(neighbor2))
                                {
                                    float numer = ant.canidates[canidate].tau * ant.canidates[canidate].degree;
                                    tempProb += (float)(numer / totalTau);

                                    if (rand < tempProb)
                                    {
                                        nextHop = canidate;
                                    }
                                    nextHop = canidate;
                                    break;
                                }
                            }
                            if (nextHop != null)
                                break;
                        }
                    }

                    ant.hop_count++;
                    RevAntPath pathBack = new RevAntPath();
                    pathBack.previous = gameObject;
                    pathBack.stop = nextHop;
                    ant.nodeData.Add(ant.hop_count, pathBack);

                    //Local update of PheremoneValues...
                 //   float tau_i = (1 - netValues.localUpdate) * nextHop.GetComponent<ACOVB>().myPhereLevel + netValues.localUpdate * qualityValue;
                    float tau_i = ant.canidates[nextHop].tau * ant.canidates[nextHop].degree; 
                    nextHop.GetComponent<ACOVB>().myPhereLevel = Mathf.Min(tau_i,100f);



  
                    //check to see if we have route to our destination , if we don't, send a ping        
                    if (!routes.ContainsKey(nextHop))
                    {
                        initMessage(nextHop, "cmd", "ping");
                    }

                    StartCoroutine(waitForPath(nextHop, ant, 1));

                }

                 //Last node reached, CDS is complete....
                else
                {
                    ant.done = true;
                    ant.destination = superVisor;
                    sendCDSAnt(ant,2);

                }
            }
            else
            {
                if (ant.myCDS.getInCDS().Contains(gameObject))
                    memberOfCDS = true;
                //CDS has been completed and is now being forwarded back to supervisor
                if (gameObject != superVisor)
                {
                    if (!antUpdates.ContainsKey(ant.label))
                    {
                        CDSAnt newAnt = new CDSAnt(ant);
                        newAnt.done = true;
                        antUpdates.Add(ant.label, newAnt);
                    }
                    if(antUpdates[ant.label].done == true) {              
                    
                        //check to see if we have route to our destination , if we don't, send a ping        
                        if (!routes.ContainsKey(ant.destination))
                        {

                            initMessage(ant.destination, "cmd", "ping");
                        }

                        StartCoroutine(waitForPath(ant.destination, ant, 2));

                    }
                //We are back at the supervisor
                }else{
                                    
                    netValues.runningCDSs.Remove(ant.label);
                    if(tempCDS ==null){
                          tempCDS = new CDS(ant.myCDS);
                            bestAnt = ant;

                    }else if(ant.myCDS.getInCDS().Count < tempCDS.getInCDS().Count)
                        {
                            tempCDS = new CDS(ant.myCDS);
                            bestAnt = ant;

                        }
                    }
      
                
            }
        }

    }

    public void sendBackAnt(CDSAnt ant)
    {
        netValues.messageCounter++;
         //if i haven't already seen this requeset...
        if (!antUpdates.ContainsKey(ant.label))
        {
            //add it to my list
            antUpdates.Add(ant.label, ant);
        if (netValues.useLatency)
            StartCoroutine(delayBackAnt(ant));
        else
            performBackAnt(ant);
        }
    }

    IEnumerator delayBackAnt(CDSAnt ant)
    {

        float distance = Vector3.Distance(gameObject.transform.position, ant.nodeData[ant.hop_count].previous.transform.position);
        distance = distance / delayFactor;
        yield return new WaitForSeconds(distance);
        performBackAnt(ant);
    }

    void performBackAnt(CDSAnt ant)
    {
        if (ant.expirationTime > lastUpdateTime)
        {
            lastUpdateTime = ant.expirationTime;
            myPhereLevel = Mathf.Max((float)((1 - netValues.newTrailInfluence) * myPhereLevel + (1 / ant.myCDS.getInCDS().Count)), 1f);

            myCurrentCDS = new CDS(ant.myCDS);

            if (ant.myCDS.getInCDS().Contains(gameObject))
            {
                memberOfCDS = true;
                foreach (GameObject neighbor in neighbors)
                {
                    netValues.messageCounter++;
                    neighbor.GetComponent<ACOVB>().sendBackAnt(ant);
                }
            }
            else
            {
                memberOfCDS = false;
            }
        }
        
    }

    IEnumerator waitForPath(GameObject nextHop, CDSAnt ant, int flag){
        while(!routes.ContainsKey(nextHop)){
        yield return null;
        }
        netValues.messageCounter++;
        nextHop.GetComponent<ACOVB>().sendCDSAnt(ant,flag);
    }


    public override void sendRREQ(RREQpacket dataOut)
    {
        string cameFrom = dataOut.intermediate.name;
        foreach (GameObject node in neighbors)
        {
            if (cameFrom != node.name && node.name != dataOut.destination.name)
            {
                if (myCurrentCDS != null)
                {
                    if (myCurrentCDS.getInCDS().Contains(node))
                    {
                        dataOut.intermediate = gameObject;
                        netValues.messageCounter++;
                        node.GetComponent<AODV>().recRREQ(dataOut);
                    }             
                }
                else
                {
                    dataOut.intermediate = gameObject;
                    netValues.messageCounter++;
                    node.GetComponent<AODV>().recRREQ(dataOut);
                }
            }
        }
    }




    #endregion


    #endregion
}
public class ACORouteEntry : AODVRouteEntry{
    public int messageQueue=0;
    public float pheremoneLevel=0;
    public int hopcount=0;
    public ACORouteEntry()
    {
    messageQueue=0;
    pheremoneLevel=0;
    hopcount=0;
    }
}


public class Ant: ACORouteEntry{
    public int maxHop;
    public GameObject source;
    public GameObject sender;
    public List<GameObject> seenNodes;
    public Dictionary<int, RevAntPath> nodeData;

   public Ant(GameObject source_, GameObject dest)
    {
        hop_count = 0;
        source = source_;
        destination = dest;
        nodeData = new Dictionary<int, RevAntPath>();
        seenNodes = new List<GameObject>();
    }

   public bool seenNode(GameObject node)
   {
       if (seenNodes.Contains(node))
           return true;
       return false;
   }
    
}
public class CDSAnt: ACORouteEntry
{
    public string label="";
    public float exploreRate;
    public int maxHop;
    public GameObject source;
    public Dictionary<int, RevAntPath> nodeData;
    public CDS myCDS;
    public Dictionary<GameObject, NeighborData> canidates;
    public List<GameObject> updateList;
    public bool done = false;
    public bool reported = false;


   public CDSAnt(GameObject source_)
    {
        updateList = new List<GameObject>();
        exploreRate = GameObject.Find("Spawner").GetComponent<ACOVBGUI>().weightFactor;
        hop_count = 0;
        source = source_;
        nodeData = new Dictionary<int, RevAntPath>();
        canidates = new Dictionary<GameObject, NeighborData>();
        myCDS = new CDS(source_);
        myCDS.owner = source_;
        done = false;
    }

   public CDSAnt(CDSAnt source_)
   {
       updateList = new List<GameObject>(source_.updateList);
       exploreRate = source_.exploreRate;
       hop_count = source_.hop_count;
       source = source_.source;
       nodeData = new Dictionary<int, RevAntPath>(source_.nodeData);
       canidates = new Dictionary<GameObject, NeighborData>(source_.canidates);
       myCDS = new CDS(source_.myCDS);
       myCDS.owner = source_.myCDS.owner;
       done = false;
   }
}

public class RevAntPath{
    public GameObject stop;
    public GameObject previous;

}

public class NeighborData
{
    public int degree;
    public float tau;
}


//for future use to build in state behavior
public interface CDSstate
{
    void performSendCDSAnt(CDS ant);

}

 //Ant implementation by Talbi
    #region TalbiAnt
/*
    public void returnAnt(Ant ant)
    {
        
        
        if (netValues.useLatency)
            StartCoroutine(delayReturnAnt(ant));
        else
            performReturnAnt(ant);
    }


    IEnumerator delayReturnAnt(Ant ant)
    {
        float distance = Vector3.Distance(gameObject.transform.position, ant.nodeData[ant.hop_count].previous.transform.position);
        distance = distance / delayFactor;
        yield return new WaitForSeconds(distance);
        performReturnAnt(ant);
    }

    void performReturnAnt(Ant ant)
    {
        gameObject.renderer.material.color = Color.red;
        if (ant.hop_count >1)
        {
            gameObject.GetComponent<ACOVB>().myPhereLevel += (float)simValues.numNodes / ant.maxHop;
            foreach (GameObject neighbor in neighbors)
            {
                neighbor.GetComponent<ACOVB>().myPhereLevel = neighbor.GetComponent<ACOVB>().myPhereLevel / (1 + (float)simValues.numNodes / ant.maxHop);
            }


            ant.hop_count--;
            ant.nodeData[ant.hop_count].previous.GetComponent<ACOVB>().returnAnt(ant);
        }
        else
        {
        }
    }



    public void sendAnt(Ant ant){
        
        if (netValues.useLatency)
            StartCoroutine(delaySendAnt(ant));
        else
            performSendAnt(ant);
    }

    IEnumerator delaySendAnt(Ant ant)
    {
     
        float distance = Vector3.Distance(gameObject.transform.position, ant.nodeData[ant.hop_count].previous.transform.position);
        distance = distance / delayFactor;
        yield return new WaitForSeconds(distance);
        performSendAnt(ant);
    }

    void performSendAnt(Ant ant)
    {
    
        gameObject.renderer.material.color = Color.green;
        if (ant.destination != gameObject)
        {
            ant.hop_count++;
            ant.maxHop = ant.hop_count;
            GameObject nextNode = selectTrail(ant);
            RevAntPath temp = new RevAntPath();
            temp.previous = gameObject;
            temp.stop = nextNode;
            if(!ant.seenNodes.Contains(nextNode))
            ant.seenNodes.Add(nextNode);
            ant.nodeData.Add(ant.hop_count,temp);
            netValues.messageCounter++;
            nextNode.GetComponent<ACOVB>().sendAnt(ant);
        }
        else
        {
            returnAnt(ant);
        }
    }

    GameObject selectTrail(Ant ant){
        //take a snapshot of current routes as to not interfer with other message passing
        Dictionary<GameObject, ACORouteEntry> tempRoutes = new Dictionary<GameObject, ACORouteEntry>(antRoutes);
        Dictionary<GameObject,float> messages = new Dictionary<GameObject,float>();
        Dictionary<GameObject,float> nFactor = new Dictionary<GameObject,float>();
        bool visitedAll = true;
        float total = 0;
          foreach (GameObject neighbor in neighbors)
          {
              if (tempRoutes.ContainsKey(neighbor))
              {
                  messages[neighbor] = tempRoutes[neighbor].messageQueue;
              }
              else
              {
                  antRoutes.Add(neighbor, new ACORouteEntry());
                  messages[neighbor] = 1;
              }
              total += messages[neighbor];
              if (total == 0)
                  total = 1;
              if (ant.seenNode(neighbor))
              {
                      visitedAll = false;
              }
             }
          foreach (GameObject neighbor in neighbors)
          {
              nFactor[neighbor] = (float)(1 - messages[neighbor] / total);
          }
          if (visitedAll)
          {
              int rand = Random.Range(0, neighbors.Count);
              return neighbors[rand];
          }
          else
          {
              bool selected = false;
              float w = float.Parse(netValues.weightFactor);
              float tempProb;
              int counter = 0;
              while (!selected)
              {
                  tempProb = 0;
                    foreach(GameObject neighbor in neighbors){
                        if (ant.seenNode(neighbor))
                        {
                           tempProb += 0f;   
                        }
                        else
                        {
                            float phere = w * antRoutes[neighbor].pheremoneLevel;
                            float heur =  (float)(1 - w) * nFactor[neighbor];
                            float normal =  (float) (1 - w) / (neighbors.Count - 1);
                            tempProb += (float)(phere + heur) / (w + normal);

                        }
                        if (tempProb == 0)
                        {
                            int rand2 = Random.Range(0, neighbors.Count);
                            return neighbors[rand2];
 
                        }

                           float rand = (float)Random.Range(0, 100) / 100;
                            if (rand < tempProb)
                            {
                                selected = true;
                                return neighbor;
                            }
 
                  }
                    counter++;
                    if (counter > 50)
                    {
                        break;
                    }
              }
          }
              return null;
          }

*/
#endregion