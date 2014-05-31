//------------------------------------------------------------
//  Title: MCDSGA.cs
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: Network (Inherits)
//
//  Description:  Script for setting up network connections usign Genetic Algorithm for
//  creation of a Minimally Connected Dominating Set virtual Backbone.  
//
//
//  Comments:  This currently does not actually support message passing as it only builds the
//  CDS.  More work is to be done to add the functionality of message passing.
//
//--------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCDSGA : AODV
{
    MCDSGAGUI guiSettings;

    public Dictionary<GameObject, GameObject> VBlines;
    public List<string> broadcasts;

    public bool[] gotBroadcast;
    public bool connected = false;

    //------------------------------Unity Functions---------------------------
    #region Unity Functions
    // Use this for initialization
    void Start()
    {

        broadcastID = 0;
        broadcasts = new List<string>();
        initializeValues();
        Random.seed = gameObject.GetComponent<NodeController>().idNum;
        VBlines = new Dictionary<GameObject, GameObject>();
        guiSettings = GameObject.Find("Spawner").GetComponent<MCDSGAGUI>();
        gotBroadcast = new bool[simValues.numNodes];
        for (int i = 0; i < simValues.numNodes; i++)
            gotBroadcast[i] = false;

    }
    //called every frame
    void Update()
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
    void OnMouseDown()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach (GameObject node in nodes)
        {
            node.renderer.material.color = Color.blue;
        }
        gameObject.renderer.material.color = Color.green;
    }


    #endregion
    //-----------------------------Network Functions--------------------------
    #region network functions
    //updates list of neighbors and adds a line.  Used for graphics generation and network management.
    //simulates "hello messages" .  Called by the node controller using a collider trigger.
    public override void addNeighbor(GameObject node)
    {
        if (!neighbors.Contains(node))
        {
            neighbors.Add(node);
            lineController.addLine(node);
        }
    }
    //updates list of neighbors and removes a line.  Used for graphics generation and network management.
    //simulates "hello messages" .  Called by the node controller using a collider trigger.
    //It also repairs the current CDS if it is still viable, but clears the population rather than checking each
    //CDS for viability.
    public override void removeNeighbor(GameObject node)
    {
        lock (guiSettings.lockGUI)
        {
            if (neighbors.Contains(node))
            {
                neighbors.Remove(node);
                lineController.removeLine(node);
            }
            if (guiSettings.currentCDS != null)
                if (guiSettings.currentCDS.getInCDS().Contains(node))
                {
                    bool otherConnection = false;
                    foreach (GameObject neighbor in neighbors)
                    {
                        if (guiSettings.currentCDS.getInCDS().Contains(neighbor))
                        {
                            otherConnection = true;
                            break;
                        }
                        else
                        {
                            foreach (GameObject neighbor2 in neighbor.GetComponent<MCDSGA>().neighbors)
                            {
                                if (guiSettings.currentCDS.getInCDS().Contains(neighbor2))
                                {
                                    CDS newCDS = new CDS(guiSettings.currentCDS);
                                    newCDS.moveToInCDS(neighbor);
                                    neighbor.renderer.material.color = Color.green;
                                    otherConnection = true;

                                    guiSettings.population.Clear();
                                    guiSettings.genPopulation();
                                    guiSettings.addToPopulation(newCDS);
                                    guiSettings.population.Remove(guiSettings.currentCDS);
                                    guiSettings.currentCDS = newCDS;

                                    break;
                                }
                            }

                        }
                    }
                    if (!otherConnection)
                    {
                        guiSettings.genPopulation();
                    }
                }
        }
    }


    #endregion
    //-----------------------------Algorithm Functions-------------------------
    #region MCDS-GA Algorithm Functions
    //------------------------------------------------------------
    //  Function: mutate
    //  Date: 5-12-2014
    //  Version: 1.0
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //
    //  Order-of: O(n) : n = number of nodes in system
    //  
    //  Class Dependicies: CDS
    //
    //  Description:  Takes a CDS as a parameter and mutates one node, either removing it from the CDS
    //  or adding it to it.  
    //
    //--------------------------------------------------------------
    public CDS mutate(CDS y)
    {
        List<GameObject> inList = new List<GameObject>(y.getInCDS());
        List<GameObject> edgeList = new List<GameObject>(y.getEdgeCDS());
        int rand = Random.Range(0, simValues.numNodes);
        foreach (GameObject node in inList)
        {
            if (rand == node.GetComponent<NodeController>().idNum)
            {
                y.moveToEdgeCDS(node);
                guiSettings.mutate++;
            }
        }
        foreach (GameObject node in edgeList)
        {
  
            if (rand == node.GetComponent<NodeController>().idNum)
            {
                y.moveToInCDS(node);
                guiSettings.mutate++;
            }
        }
        int randNode = Random.Range(0, inList.Count);
        y.owner = GameObject.Find("Node " + randNode);
        return y;
    }


    //------------------------------------------------------------
    //  Function: crossover
    //  Algorithm: MCDS - GA
    //  Date: 5-12-2014
    //  Version: 1.0
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //
    //  Class Dependicies: Set of CDS 
    //
    //  Order-of: O(n) for performing cross-over.  
    //
    //  Description:  Takes a Set of CDS as a parameter and produces two children to add back into
    //  the population.  Children are not considered as options when selecting the parents as they are
    //  directely added back to the main population.
    //
    //  Crossover rate is based on parameter set by user.  The algorithm alternates between uniform crossover
    //  and 1-point cross over to increase chance of diversity.
    //
    //--------------------------------------------------------------
    public void crossover(List<CDS> population_)
    {
        StartCoroutine(performCrossover(population_));
    }

    IEnumerator performCrossover(List<CDS> population_)
    {
        CDS p1 = new CDS(gameObject);
        CDS p2 = new CDS(gameObject);

     //   int crossoverRate = (int)(1.0f / Time.deltaTime);
     //   guiSettings.crossoverCountStr = crossoverRate.ToString();
        for (int i = 0; i < int.Parse(guiSettings.crossoverCountStr); i++)
        {
            guiSettings.generations++;
            yield return null;
            if (population_.Count > 1)
            {
                p1 = selectParent(population_,1);
                p2 = selectParent(population_,2);
            }
            else
            {
                p1 = guiSettings.currentCDS;
                p2 = guiSettings.currentCDS;
            }

            CDS child1 = new CDS(gameObject);
            CDS child2 = new CDS(gameObject);
            bool child1Valid = false;
            bool child2Valid = false;
            switch (i % 2 + 1)
            {
                case 1:
                for (int k = 0; k < simValues.numNodes; k++)
                {
                    GameObject node = GameObject.Find("Node " + k);
                    if (Random.value < .5f)
                    {
                        if (p1.getEncoding()[k])
                        {
                            child1.moveToInCDS(node);
                        }
                        if (p2.getEncoding()[k])
                        {
                            child2.moveToInCDS(node);
                        }
                    }
                    else
                    {
                        if (p2.getEncoding()[k])
                        {
                            child1.moveToInCDS(node);
                        }
                        if (p1.getEncoding()[k])
                        {
                            child2.moveToInCDS(node);
                        }
                    }
                }
                break;
                case 2:
                int xoPoint = Random.Range(0, simValues.numNodes);
                for (int x = 0; x < simValues.numNodes; x++)
                {
                    GameObject node = GameObject.Find("Node " + x);
                    if (x < xoPoint)
                    {

                        if (p1.getEncoding()[x])
                        {
                            child1.moveToInCDS(node);
                        }
                        if (p2.getEncoding()[x])
                        {
                            child2.moveToInCDS(node);
                        }
                    }
                    else
                    {
                        if (p2.getEncoding()[x])
                        {
                            child1.moveToInCDS(node);
                        }
                        if (p1.getEncoding()[x])
                        {
                            child2.moveToInCDS(node);
                        }
                    }
                }
                break;
            }


            child1 = mutate(child1);
            child2 = mutate(child2);
            child1Valid = checkFeasibility(child1);
            if (child1Valid)
                guiSettings.addToPopulation(child1);
            child2Valid = checkFeasibility(child2);
            if (child2Valid)
                guiSettings.addToPopulation(child2);
        }

    }

    //------------------------------------------------------------
    //  Function: selectParent
    //  Algorithm: MCDS - GA
    //  Date: 5-26-2014
    //  Version: 3
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //
    //  Class Dependicies: Set of CDS 
    //
    //  Order-of: O(n) in the extreme case of bad random generation, but still total of O(n)
    //
    //  Description:  Takes a Set of CDS as a parameter and selects parent for crossover.
    //  Parent 1 is selected based on fitness modified probability, where Parent 2 is selcted
    //  purely at random.
    //
    //  Crossover rate is based on parameter set by user.  The algorithm alternates between uniform crossover
    //  and 1-point cross over to increase chance of diversity.
    //
    //--------------------------------------------------------------

    public CDS selectParent(List<CDS> population, int i)
    {
        lock (nodeLock)
        {
            List<CDS> population_ = new List<CDS>(population);
            CDS maxCDS = null;

            bool parentSelected = false;

            while (!parentSelected)
            {
                if (i == 1) { 
                float rand = (float)Random.Range(0, 100)/100;
                float max = 0f;
                float avg = 0f;
                float tempFit = 0f;

                    foreach (CDS val in population_)
                      {
                    tempFit = val.getFitness();
                    if (tempFit > max){
                        max = tempFit;
                        maxCDS = val;
                    }
                    avg += tempFit;
                    }
                avg = avg / population_.Count;
                
                tempFit = maxCDS.getFitness()/avg*.5f;
                 if(rand < tempFit ){
                        return maxCDS;
                    }
                  else{
                      if (population_.Count != 0)
                      {
                          population_.Remove(maxCDS);
                      }
                      else
                      {
                          break;
                      }
                }
            }
                else if (i == 2)
                {
                    return population[Random.Range(0, population_.Count)];
                }
            }
            return generateCDS();
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
        int counter = simValues.numNodes * simValues.numNodes;
        List<GameObject> openList = new List<GameObject>(CDStoCheck.getInCDS());
        List<GameObject> closedList = new List<GameObject>();
        GameObject node = openList[0];
        openList.Remove(node);
        closedList.Add(node);
        while (openList.Count > 0)
        {

            foreach (GameObject neighbor in node.GetComponent<MCDSGA>().neighbors)
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
                    guiSettings.rejects++;
                    return false;

                }
            }
        }

        return true;
    }

    //------------------------------------------------------------
    //  Function: generateCDS
    //  Algorithm: MCDS - GA
    //  Date: 5-28-2014
    //  Version: 3
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //
    //  Class Dependicies: none
    //
    //  Returns valid CDS
    //
    //  Order-of: O(n^2) although a tighter approximation would be O(k * n) where k =
    //  avg|deg(n_1)|, for all n = the set.
    //
    //  Description:  Builds a CDS by picking a random node, adding it and its neighbors to the Edge Set.
    //  It then picks a node from the edge set, moves it to the CDS set and adds it neighbors to the edge set.
    //  It repeats this process until all nodes have been added to either the edge list or CDS list.  It then 
    //  checks its validity using the feasibility function and returns the CDS if it checks out.
    //
    //--------------------------------------------------------------

    public CDS generateCDS()
    {
        bool validCDS = false;
        CDS CDStoBuild = null;
        while (!validCDS)
        {
            int randNode = Random.Range(0, simValues.numNodes);
            GameObject owner = GameObject.Find("Node " + randNode);
            CDStoBuild = new CDS(owner);
            CDStoBuild.moveToInCDS(owner);
            foreach (GameObject node in owner.GetComponent<MCDSGA>().neighbors)
            {
                CDStoBuild.moveToEdgeCDS(node);
            }

            while (CDStoBuild.getOutCDS().Count != 0)
            {
                //       yield return new WaitForEndOfFrame();
                GameObject maxNode = null;

                int rand = Random.Range(0, CDStoBuild.getEdgeCDS().Count);
                maxNode = CDStoBuild.getEdgeCDS()[rand];
                if (maxNode)
                {
                    CDStoBuild.moveToInCDS(maxNode);

                    //add max nodes's neighbors to the edge list
                    foreach (GameObject node in maxNode.GetComponent<MCDSGA>().neighbors)
                    {
                        if (CDStoBuild.getOutCDS().Contains(node))
                        {
                            CDStoBuild.moveToEdgeCDS(node);
                        }
                    }
                }

            }
            randNode = Random.Range(0, CDStoBuild.getInCDS().Count);
            CDStoBuild.owner = GameObject.Find("Node " + randNode);


            if (checkFeasibility(CDStoBuild))
                validCDS = true;
        }
        return CDStoBuild;
    }

    #endregion
}

