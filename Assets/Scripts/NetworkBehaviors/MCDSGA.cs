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
//--------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MCDSGA : Network
{
    NetworkGUI guiSettings;
    public bool inCDS = false;
    public bool hasCDS = false;
    int nodeSeqNum;
    int broadcastID;
    public Dictionary<GameObject, List<GameObject>> spanningTree;
    public List<string> treeRequests;
    public List<GameObject> myCDSLinks;
    public int spanTreeCount=0;

    // Use this for initialization
    void Start()
    {
        myCDSLinks = new List<GameObject>();
        setValues();
        nodeSeqNum = 0;
        broadcastID = 0;
        spanningTree = new Dictionary<GameObject, List<GameObject>>();
        treeRequests = new List<string>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        generateSpanTree();

    }

    //--------Custome functions----------------


    public override void drawLines()
    {
        lock (nodeLock)
        {
            GameObject source = gameObject;
            Color lineColor = Color.cyan;

            //loop through all the lines in our container and update accordingly
            foreach (DictionaryEntry entry in lineController.lines)
            {
                GameObject line = (GameObject)entry.Value;
                if (line)
                {//check to see if its been destroyed already
                    line.GetComponent<LineRenderer>().enabled = true;
                    GameObject dest = GameObject.Find("Node " + entry.Key);
                    line.GetComponent<LineRenderer>().SetPosition(0, source.transform.position);
                    line.GetComponent<LineRenderer>().SetPosition(1, dest.transform.position);
                    if (!spanningTree.ContainsKey(source))
                    {
                        lineColor = Color.red;
                    }
                    if (!spanningTree.ContainsKey(dest))
                    {
                        lineColor = Color.red;
                    }
                }
                line.GetComponent<LineRenderer>().SetColors(lineColor, lineColor);
            }
        }
    }

    public override void addNeighbor(GameObject node)
    {
        if (!neighbors.Contains(node))
        {
            if (inCDS)
            {
                if (node.GetComponent<MCDSGA>().inCDS)
                {

                }
            }

            neighbors.Add(node);
        }
    }
    //public function to be called by nodeController if we need to remove a connection
    public override void removeNeighbor(GameObject node)
    {
        if (neighbors.Contains(node))
        {
            neighbors.Remove(node);
            lineController.removeLine(node);
            if (spanningTree.ContainsKey(gameObject))
            {
                if (spanningTree[gameObject].Contains(node))
                {
                    generateSpanTree();
                }
            }
        }
    }

    private Hashtable crossover(Hashtable p1, Hashtable p2)
    {
        Hashtable child1 = new Hashtable();
        return child1;
    }

    private Hashtable mutate(Hashtable x)
    {
        Hashtable y = new Hashtable(x);
        return y;
    }

    private bool checkCDSFeasibility(Hashtable y)
    {
        return true;
    }

    public void recTreeBuild(TreePacket tPacket)
    {
        StartCoroutine(delayTreeBuild(tPacket));
    }
    IEnumerator delayTreeBuild(TreePacket tPacket)
    {
        float random;
        if (netValues.useLatency)
        {
            random = Random.Range(.0f, .02f);
        }
        else
        {
            random = 0f;
        }
            yield return new WaitForSeconds(random);
        performTreeBuild(tPacket);
    }
    public void performTreeBuild(TreePacket tPacket)
    {

        lock (nodeLock)
        {
            int counter = 0;
            if (!treeRequests.Contains(tPacket.reqId))
            {
                if (tPacket.sentBy.GetComponent<NodeController>().idNum < gameObject.GetComponent<NodeController>().idNum)
                {
                    lineController.addLine(tPacket.sentBy);
                }
                else
                {
                    tPacket.sentBy.GetComponent<NodeLine>().addLine(gameObject);
                }

                gameObject.renderer.material.color = Color.white;
                hasCDS = true;
                treeRequests.Add(tPacket.reqId);
                tPacket.source.GetComponent<MCDSGA>().addTreeEdge(tPacket.sentBy, gameObject);
                tPacket.sentBy = gameObject;
                foreach (GameObject node in neighbors)
                {
                    if (!node.GetComponent<MCDSGA>().hasCDS)
                    {


                        node.GetComponent<MCDSGA>().recTreeBuild(tPacket);
                        counter++;

                    }
                }
            }
        }
    }

    public void addTreeEdge(GameObject node1, GameObject node2)
    {
        lock (nodeLock)
        {
            if (spanningTree.ContainsKey(node1))
            {
                spanningTree[node1].Add(node2);
            }
            else
            {
                List<GameObject> nodeList = new List<GameObject>();
                nodeList.Add(node2);
                spanningTree.Add(node1, nodeList);
            }
        }
        spanTreeCount++;
        if (spanTreeCount == simValues.numNodes-1)
        {
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
            foreach (GameObject node in nodes)
            {
                node.GetComponent<MCDSGA>().updateSpanTree(spanningTree);;
            }

        }

    }

    public void generateSpanTree()
    {
        lock (nodeLock)
        {
            resetSpanTree();

            GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
            foreach (GameObject node in nodes)
            {
                node.GetComponent<MCDSGA>().inCDS = false;
                node.GetComponent<MCDSGA>().hasCDS = false;
                node.renderer.material.color = Color.blue;

            }

            inCDS = true;
            hasCDS = true;
            List<GameObject> temp = new List<GameObject>(neighbors);

            TreePacket tPacket = new TreePacket();


            tPacket.reqId = gameObject.name + broadcastID;
            tPacket.source = gameObject;
            tPacket.sentBy = gameObject;
            tPacket.TTL = simValues.numNodes / 2;
            treeRequests.Add(tPacket.reqId);
            broadcastID++;
            foreach (GameObject node in temp)
            {
                node.GetComponent<MCDSGA>().recTreeBuild(tPacket);
            }
        }
    }

    public void updateSpanTree(Dictionary<GameObject, List<GameObject>> spanningTree_)
    {
        lock (nodeLock)
        {
            spanningTree = spanningTree_;

            if (spanningTree.ContainsKey(gameObject))
            {
                myCDSLinks = spanningTree[gameObject];
                if (myCDSLinks.Count == 0)
                {

                    gameObject.renderer.material.color = Color.red;
                }
            }
            else
            {
                gameObject.renderer.material.color = Color.red;
            }
        }
    }

    public void resetSpanTree()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach (GameObject node in nodes)
        {
            node.GetComponent<MCDSGA>().spanningTree.Clear();
            node.GetComponent<NodeLine>().lines.Clear();
        }
        GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
    }


}

public class CDSMember
{

}

public class MCDSGARouteEntry : RouteEntry
{
    public int numberHops;
}

public struct TreePacket
{
    public GameObject source;
    public GameObject sentBy;
    public string reqId;
    public int TTL;
}


