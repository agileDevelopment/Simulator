//-----------------Header-------------------------
//  Title: ACOVB.cs
//  Date: 5-30-2014
//  Version: 1.0
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ACOVB : AODV {
    new ACOVBGUI netValues;
    public CDS myCurrentCDS;

    public Dictionary<GameObject, GameObject> VBlines;
    public Dictionary<GameObject, ACORouteEntry> antRoutes;
    Dictionary<string, CDSAnt> antUpdates;
    public float myPhereLevel;
    public int maxCDS;
    int counter=0;
    public bool memberOfCDS = false;
    public bool connected = false;


	// Use this for initialization
    protected override void Start()
    {
        base.Start();
        myCurrentCDS = null;
        VBlines = new Dictionary<GameObject, GameObject>();
        myPhereLevel = 0.01f;
        maxCDS = 0;
        antRoutes = new Dictionary<GameObject, ACORouteEntry>();
        antUpdates = new Dictionary<string, CDSAnt>();
        netValues = (ACOVBGUI)simValues.networkGUI;
    }
  	

	// Update is called once per frame
    protected override void Update()
    {
        pollNeighbors();
        base.Update();
        counter++;
        if (counter % 2 == 0)
        {


            counter = 0;
            if (myCurrentCDS != null)
            {
                if (netValues.displayCDS)
                    displayCDS();
            }

        }
        if (netValues.source == gameObject)
        {
            netValues.myUIElements["pLevel"] = "Phereomon: " + myPhereLevel.ToString();
        }
	}


    //LateUpdate is called once per frame after Update
    protected override void LateUpdate()
    {
        base.LateUpdate();
    }


    protected void displayCDS()
    {
        float minDistance = netValues.nodeCommRange;
        GameObject cdsNode = gameObject;
        if (myCurrentCDS != null)
        {

            foreach (KeyValuePair<GameObject, GameObject> entry in VBlines)
            {
                GameObject line = (GameObject)entry.Value;
                if (line)//check to see if its been destroyed already
                {
                    line.GetComponent<LineRenderer>().enabled = true;
                    GameObject dest = entry.Key;


                    line.GetComponent<LineRenderer>().SetPosition(0, gameObject.transform.position);
                    line.GetComponent<LineRenderer>().SetPosition(1, dest.transform.position);
                }
            }
        }
        
    }


    //---------------------------Utility Functions----------------------------------
    #region Utility

        protected override void performRecMessage(MSGPacket packet)
    {
        bool fwd=false;
        if (gameObject == packet.destination)
        {
            if(packet.messageType == "mess"){
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
                if (packet.message == "clear")
                {
                    if (netValues.runningCDSs.Count == 0)
                    {
                        maxCDS = 0;
                        netValues.maxCDS = 0;
                        netValues.currentCDS = null;
                        myCurrentCDS = null;
                    }
   
                }
                if (packet.message == "genCDS")
                {
                    maxCDS = 0;
                    generateAntCDS();
                }
            }
        }
        else
        {
            fwd = true;
        }
        if(fwd)
        {
            if(packet.messageType == "CDS"){
                if (memberOfCDS)
                {
                    sendMessage(packet);
                }
            }
             sendMessage(packet);
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

        if (node.GetComponent<ACOVB>().memberOfCDS)
        {
            if (myCurrentCDS != null)
            if(!myCurrentCDS.getInCDS().Contains(node)){
                myCurrentCDS.moveToInCDS(node);
                
            }

        }

    }
    //public function to be called by nodeController if we need to remove a connection
    public override void removeNeighbor(GameObject node)
    {
        base.removeNeighbor(node);
        if (routes.ContainsKey(node))
            routes.Remove(node);
        if (myCurrentCDS != null)
        {
            int cdsNeighborCount = 0;

            if (myCurrentCDS.getInCDS().Contains(node))
            {
                myCurrentCDS.moveToOutCDS(node);
            }
            foreach (GameObject neighbor in neighbors)
            {
                if (neighbor.GetComponent<ACOVB>().memberOfCDS)
                {
                    cdsNeighborCount++;
                }

            }
            if (cdsNeighborCount == 0)
            {
                initBroadcast("cmd", "clear");
                initMessage(myCurrentCDS.owner, "cmd", "genCDS");
            }
            else
            {
                if (!checkFeasibility(myCurrentCDS))
                {
                    initBroadcast("cmd", "clear");
                    initMessage(myCurrentCDS.owner, "cmd", "genCDS");
                
                }
            }


            if (VBlines.ContainsKey(node))
            {
                GameObject line = VBlines[node];
                VBlines.Remove(node);
                Destroy(line);
            }
        }

    }

    //iterates through neighbors and updates list of pheremone levels
    protected void pollNeighbors()
    {
        foreach (GameObject neighbor in neighbors)
        {
            if (!antRoutes.ContainsKey(neighbor))
            {
                antRoutes.Add(neighbor, new ACORouteEntry());
            }
                antRoutes[neighbor].messageQueue = neighbor.GetComponent<ACOVB>().currentRREQ.Count;
                antRoutes[neighbor].pheremoneLevel = neighbor.GetComponent<ACOVB>().myPhereLevel;
            
        }
    }
    int getRREQCount(){
        return currentRREQ.Count;
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
    #region CDSAnt
    public void generateAntCDS(){
        broadcastID++;
        CDSAnt myAnt = new CDSAnt(gameObject);
        myAnt.label = gameObject.name + broadcastID.ToString();
        netValues.runningCDSs.Add(myAnt.label, Time.time + 2);
        foreach (GameObject neighbor in neighbors)
        {
            myAnt.myCDS.moveToEdgeCDS(neighbor);
        }
         performSendCDSAnt(myAnt);
    }

    public void sendCDSAnt(CDSAnt ant)
    {
        broadcastID++;
        if (netValues.useLatency)
            StartCoroutine(delaySendCDSAnt(ant));
        else
            performSendCDSAnt(ant);
    }

    IEnumerator delaySendCDSAnt(CDSAnt ant)
    {
        float distance = Vector3.Distance(gameObject.transform.position, ant.nodeData[ant.hop_count].previous.transform.position);
        distance = distance / delayFactor;
        yield return new WaitForSeconds(distance);
        performSendCDSAnt(ant);
    }


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



   // void initCDSBroadcast()

    void performSendCDSAnt(CDSAnt ant)
    {
        memberOfCDS = true;
        if (ant.myCDS.getInCDS().Count == 0)
            ant.myCDS.owner = gameObject;

        //Initialize heuristic values
        GameObject nextHop = null;
        float tempProb = 0;
        float totalTau = 0;
        float qualityValue = 0;

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
         //           neighbor.renderer.material.color = Color.grey;
                    NeighborData nData = new NeighborData();
                    nData.degree = neighbor.GetComponent<ACOVB>().neighbors.Count-1;
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
       if (ant.myCDS.getOutCDS().Count > 0){
            
            float rand = (float)Random.Range(0, 100) / 100;
            
            //choose whether to explore or exploit
            if (rand > ant.exploreRate)//we are exploring
                //choose a neighbor edge to explore rather than jumping across the graph
            {
                float maxTauN = 0;
                foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                {
                    float arg=0;
                    arg = ant.canidates[canidate].degree * ant.canidates[canidate].tau;
                    if (arg >= maxTauN)
                    {
                        
                        maxTauN = arg;
                        nextHop = canidate;
                    }
                }
            }
            else//we are exploiting
            {
                rand = (float)Random.Range(0, 100) / 100;
                 foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                {
                    
                    totalTau += (ant.canidates[canidate].tau * ant.canidates[canidate].degree);
                }

                 foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                {
                    if (neighbors.Contains(canidate))
                    {
                        float numer = ant.canidates[canidate].tau * ant.canidates[canidate].degree;
                        tempProb += (float)(numer / totalTau);

                        if (rand < tempProb)
                        {
                            nextHop = canidate;
                            qualityValue = (float)(numer / totalTau);
                        }
                    }
                }
            }
           //out of direct hops, choose a random edge
            if (nextHop == null)
            {
               foreach (GameObject canidate in ant.myCDS.getEdgeCDS())
                {
                    foreach (GameObject neighbor2 in canidate.GetComponent<ACOVB>().neighbors)
                    {
                        if (ant.myCDS.getOutCDS().Contains(neighbor2))
                        {
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
            float tau_i = (1 - float.Parse(netValues.localUpdate)) * nextHop.GetComponent<ACOVB>().myPhereLevel + float.Parse(netValues.localUpdate)* qualityValue;
            nextHop.GetComponent<ACOVB>().myPhereLevel = tau_i;




            //check to see if we have route to our destination , if we don't, send a ping        
            if (!routes.ContainsKey(nextHop))
            {
              //  discoverPath(nextHop);
               initMessage(nextHop, "cmd", "ping");
            }

            StartCoroutine(waitForPath(nextHop, ant));
        
            }

        //Last node reached, CDS is complete....
        else
        {
            netValues.runningCDSs.Remove(ant.label);
            if (netValues.currentCDS != null)
            {
                if (netValues.currentCDS.getInCDS().Count > ant.myCDS.getInCDS().Count)
                {
                    netValues.currentCDS = ant.myCDS;
                }

            }
            else
            {
                netValues.currentCDS = ant.myCDS;
            }

            bool sendResult = true;
            if (myCurrentCDS != null)
                if (ant.myCDS.getInCDS().Count > myCurrentCDS.getInCDS().Count)
                    sendResult = false;
           if(sendResult) 
           {

                foreach (GameObject node in ant.myCDS.getInCDS())
                {
                    node.renderer.material.color = Color.black;
                }
                foreach (GameObject node in ant.myCDS.getEdgeCDS())
                {
                    node.renderer.material.color = Color.grey;
                }
                foreach (GameObject node in ant.myCDS.getOutCDS())
                {
                    node.renderer.material.color = Color.white;
                }
                foreach (GameObject neighbor in neighbors)
                {
                    if (ant.myCDS.getInCDS().Contains(neighbor))
                        netValues.messageCounter++;
                        neighbor.GetComponent<ACOVB>().sendPheremoneUpdate(ant);
                }
            }
        }
    }


    public void sendPheremoneUpdate(CDSAnt ant)
    {
        if (netValues.useLatency)
            StartCoroutine(delayPheremoneUpdate(ant));
        else
            performPheremoneUpdate(ant);
    }

    IEnumerator delayPheremoneUpdate(CDSAnt ant)
    {

        float distance = Vector3.Distance(gameObject.transform.position, ant.nodeData[ant.hop_count].previous.transform.position);
        distance = distance / delayFactor;
        yield return new WaitForSeconds(distance);
        performPheremoneUpdate(ant);
    }

    void performPheremoneUpdate(CDSAnt ant)
    {
        //if i haven't already seen this requeset...
        if (!antUpdates.ContainsKey(ant.label))
        {
            //add it to my list
            antUpdates.Add(ant.label, ant);
            myPhereLevel = (float)((1 - float.Parse(netValues.newTrailInfluence)) * myPhereLevel + (1 / ant.myCDS.getInCDS().Count));

            myCurrentCDS = ant.myCDS;
            if (ant.source == gameObject)
            {

            }
            //and send it on if I'm a CDS node
            if (ant.myCDS.getInCDS().Contains(gameObject))
            {
                memberOfCDS = true;
                gameObject.renderer.material.color = Color.black;
                foreach (GameObject neighbor in neighbors)
                {
                    netValues.messageCounter++;
                    neighbor.GetComponent<ACOVB>().sendPheremoneUpdate(ant);
                }
            }
        }
    }

    IEnumerator waitForPath(GameObject nextHop, CDSAnt ant){
        while(!routes.ContainsKey(nextHop)){
        yield return null;
        }
        netValues.messageCounter++;
        nextHop.GetComponent<ACOVB>().sendCDSAnt(ant);
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


   public CDSAnt(GameObject source_)
    {
        updateList = new List<GameObject>();
        exploreRate = float.Parse(GameObject.Find("Spawner").GetComponent<ACOVBGUI>().weightFactor);
        hop_count = 0;
        source = source_;
        nodeData = new Dictionary<int, RevAntPath>();
        canidates = new Dictionary<GameObject, NeighborData>();
        myCDS = new CDS(source_);
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