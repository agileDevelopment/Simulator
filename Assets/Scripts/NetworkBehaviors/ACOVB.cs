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

public class ACOVB : AODV {
    public Dictionary<GameObject, GameObject> VBlines;
    public float myPhereLevel;
    int maxCDS = 0;
    public Dictionary<GameObject, ACORouteEntry> antRoutes;
    CDS myCurrentCDS;
    ACOVBGUI guiSettings;
    int counter=0;
    Dictionary<string, CDSAnt> antUpdates;
    public bool partOfCDS = false;
    bool cdsUpdated = false;


	// Use this for initialization
    protected override void Start()
    {    
        base.Start();
        myCurrentCDS = null;
        VBlines = new Dictionary<GameObject, GameObject>();
        guiSettings = GameObject.Find("Spawner").GetComponent<ACOVBGUI>();
        myPhereLevel = 0.01f;
        antRoutes = new Dictionary<GameObject, ACORouteEntry>();
        antUpdates = new Dictionary<string, CDSAnt>();  
    }
  	

	// Update is called once per frame
    protected override void Update()
    {
  
        pollNeighbors();
        base.Update();
        counter++;
        if (counter % 60 == 0)
        {
            //   displayCDS();
            counter = 0;
            if (myCurrentCDS != null)
            {
                int rand = Random.Range(0, 100);
                if (rand < 5)
                    initMessage(GameObject.Find("Node 10"),"genCDS");
            }

        }
	}

    protected void displayCDS()
    {
        float minDistance = guiSettings.nodeCommRange;
        GameObject cdsNode = gameObject;
        if (myCurrentCDS != null)
        {
            if (myCurrentCDS.getInCDS().Contains(gameObject))
                partOfCDS = true;

            if (cdsUpdated)
            {
                cdsUpdated = false;
                GameObject line = null;
                Dictionary<GameObject, GameObject> temp = new Dictionary<GameObject, GameObject>(VBlines);
                foreach (GameObject node in temp.Keys)
                {
                    line = VBlines[node];
             //       print("deleting Line" + line.name);
                    Destroy(line);
                }
                VBlines.Clear();
            }
            //If I'm part of a CDS..
            if (partOfCDS)
            {
                gameObject.renderer.material.color = Color.black;
                foreach (GameObject node in neighbors)
                //foreach (GameObject node in myCurrentCDS.network[gameObject])
                {
                    if (node.GetComponent<ACOVB>().partOfCDS)
                    {
                        if (!VBlines.ContainsKey(node))
                        {
                            GameObject line = (GameObject)GameObject.Instantiate(simValues.linePrefab);
                            line.tag = "VBLine";
                            line.name = "VBline_" + gameObject.GetComponent<NodeController>().idNum.ToString() +
                                node.GetComponent<NodeController>().idNum.ToString();
                            line.transform.parent = gameObject.transform;
                            line.GetComponent<LineRenderer>().SetColors(Color.black, Color.black);
                            line.GetComponent<LineRenderer>().SetWidth(2, 2);
                            gameObject.GetComponent<ACOVB>().VBlines.Add(node, line);
                        }
                        else
                        {
                            VBlines[node].GetComponent<LineRenderer>().SetWidth(2, 2);
                            VBlines[node].GetComponent<LineRenderer>().SetColors(Color.black, Color.black);
                        }
                    }
                }
            }
            else
            {  // I'm not part of the CDS
                foreach (GameObject neighborCDS in neighbors)
                {
                    if (myCurrentCDS.getInCDS().Contains(neighborCDS))
                    {
                        float distance = Vector3.Distance(gameObject.transform.position, neighborCDS.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            cdsNode = neighborCDS;
                        }
                    }
                }
                if (!VBlines.ContainsKey(cdsNode))
                {
                    GameObject line = (GameObject)GameObject.Instantiate(simValues.linePrefab);
                    line.tag = "VBLine";
                    line.name = "VBline_" + gameObject.GetComponent<NodeController>().idNum.ToString() +
                        cdsNode.GetComponent<NodeController>().idNum.ToString();
                    line.transform.parent = gameObject.transform;
                    line.GetComponent<LineRenderer>().SetColors(Color.blue, Color.blue);
                    line.GetComponent<LineRenderer>().SetWidth(2, 2);
                    gameObject.GetComponent<ACOVB>().VBlines.Add(cdsNode, line);
                }
                else
                {
                    VBlines[cdsNode].GetComponent<LineRenderer>().SetWidth(2, 3);
                    VBlines[cdsNode].GetComponent<LineRenderer>().SetColors(Color.blue, Color.blue);
                }

            }
        }

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


    protected override void FixedUpdate()
    {
        counter++;
        base.FixedUpdate();
        if (guiSettings.start && counter > 30)
        {

          }
        
    }
    //---------------------------Utility Functions----------------------------------
    #region Utility

        protected override void performRecMessage(MSGPacket packet)
    {
      //  gameObject.renderer.material.color = Color.white;
        if (gameObject == packet.destination)
        {
            if(packet.message == "ping"){
               initMessage(packet.source,"ack");
            }
            if (packet.message == "ack")
            {

            }
            if(packet.message == "clear"){
                maxCDS = 0;
            }
            if (packet.message == "genCDS")
            {
                maxCDS = 0;
                generateAntCDS();
            }
      //      gameObject.renderer.material.color = Color.green;
      //      float tTime  = Time.time - packet.startTime;
      //      print(packet.message + "Time to Send: " + tTime );
        }
        else
        {
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

        if (node.GetComponent<ACOVB>().partOfCDS)
        {
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
            if(myCurrentCDS.network.ContainsKey(gameObject))
                if (myCurrentCDS.network[gameObject].Contains(node))
                     myCurrentCDS.network[gameObject].Remove(node);
            bool validCDS = true;
            foreach (GameObject neighbor in neighbors)
            {
                if (myCurrentCDS.getInCDS().Contains(neighbor))
                {
                    initMessage(myCurrentCDS.owner, "genCDS");
                //    validCDS = checkFeasibility(myCurrentCDS);
                    break;
                }
            }
            if (!validCDS)
            {
                
                generateAntCDS();
            }
        }

        if (VBlines.ContainsKey(node))
        {
            GameObject line = VBlines[node];
            VBlines.Remove(node);
            Destroy(line);
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
    //Ant implementation by Talbi
    #region TalbiAnt
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
            nextNode.GetComponent<ACOVB>().sendAnt(ant);
        }
        else
        {
            returnAnt(ant);
        }
    }

    public void startAnt(GameObject dest)
    {
        Ant ant = new Ant(gameObject, dest);
        performSendAnt(ant);
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
              float w = float.Parse(guiSettings.weightFactor);
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
          print("here");
              return null;
          }
    #endregion

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
        int counter = simValues.numNodes * simValues.numNodes;
        List<GameObject> openList = new List<GameObject>(CDStoCheck.getInCDS());
        List<GameObject> closedList = new List<GameObject>();
        GameObject node = openList[0];
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

    void performSendCDSAnt(CDSAnt ant)
    {
        if (ant.myCDS.getInCDS().Count == 0)
            ant.myCDS.owner = gameObject;
        GameObject nextHop = null;
        float tempProb = 0;
        float totalTau = 0;
        float qualityValue = 0;
     //   gameObject.renderer.material.color = Color.black;

        //CDS build isn't finished...
        if (ant.myCDS.getOutCDS().Count > 0)
        {
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

                        //       print("num: = " + numer + "    TotalTau: " + totalTau);
                        tempProb += (float)(numer / totalTau);

                        //      print("rand: " + rand + " <->" + tempProb);
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

            //check to see if we have route to our destination          
            if (!routes.ContainsKey(nextHop))
            {
                initMessage(nextHop, "ping");
            }

            ant.hop_count++;
            RevAntPath pathBack = new RevAntPath();
            pathBack.previous = gameObject;
            pathBack.stop = nextHop;
            ant.nodeData.Add(ant.hop_count, pathBack);
            
            //Local update of PheremoneValues...
            float tau_i = (1 - float.Parse(guiSettings.localUpdate)) * nextHop.GetComponent<ACOVB>().myPhereLevel + 2;//float.Parse(guiSettings.localUpdate)* qualityValue;
            nextHop.GetComponent<ACOVB>().myPhereLevel = tau_i;

            StartCoroutine(waitForPath(nextHop, ant));
        
            }

        //Last node reached, CDS is complete....

        
        else
        {
            bool sendResult = true;
            if (myCurrentCDS != null)
                if (ant.myCDS.getInCDS().Count > myCurrentCDS.getInCDS().Count)
                    sendResult = false;
           if(sendResult) 
            {
                print("Generated CDS is feasible: " + checkFeasibility(ant.myCDS));

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
            myPhereLevel = (float)((1 - float.Parse(guiSettings.newTrailInfluence)) * myPhereLevel + (1 / ant.myCDS.getInCDS().Count));

            myCurrentCDS = ant.myCDS;
            cdsUpdated = true;
            if (ant.source == gameObject)
            {
         //       print("Sending out new ant...");
         //       generateAntCDS(); 
            }
            //and send it on if I'm a CDS node
            if (ant.myCDS.getInCDS().Contains(gameObject))
            {
                gameObject.renderer.material.color = Color.black;
          //      print(gameObject + "forwarding update");
                foreach (GameObject neighbor in neighbors)
                {
                    neighbor.GetComponent<ACOVB>().sendPheremoneUpdate(ant);
                }
            }
            else
            {
                gameObject.renderer.material.color = Color.white;
            }
        }
    }

    IEnumerator waitForPath(GameObject nextHop, CDSAnt ant){
        while(!routes.ContainsKey(nextHop)){
        yield return null;
        }
      //  print("got a path to " + nextHop.name);
        nextHop.GetComponent<ACOVB>().sendCDSAnt(ant);
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


public class Ant
{
    public int hop_count;
    public int maxHop;
    public GameObject source;
    public GameObject destination;
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
public class CDSAnt
{
    public string label="";
    public float exploreRate;
    public int hop_count;
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


