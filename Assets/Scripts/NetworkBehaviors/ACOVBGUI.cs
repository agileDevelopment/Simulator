using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ACOVBGUI : AODVGUI {
    public object myLock;
    public CDS currentCDS;
    public int maxCDS;
    public string weightFactor;
    public string newTrailInfluence;
    public string localUpdate;
    public string startStr = "Start";
    public float maxPheremoneLevel;
    public bool start= false;
    public bool displayCDS = false;
    public Dictionary<string, float> runningCDSs;
    int counter = 0;
    GameObject displayNode;

	// Use this for initialization
	protected override void Start () {
        maxCDS = 0;
        base.Start();
        myLock = new object();
        currentCDS = null;
        runningCDSs = new Dictionary<string, float>();
        maxPheremoneLevel = 0f;
        weightFactor = ".8";
        newTrailInfluence = ".8";
        localUpdate = ".8";
        myUIElements.Add("pLevel", "");
        myUIElements.Add("CDS Running", "");

        

	}
	
	// Update is called once per frame
    protected override void Update()
    {
        counter++;

        if (counter % 2 == 0)
        {

            if (displayCDS)
            {
                if (currentCDS != null)
                {
           //         displayCurrentCDS();
                }
            }

            counter = 0;
        }
	}

    protected virtual void LateUpdate()
    {

        Dictionary<string, float> temp = new Dictionary<string, float>(runningCDSs);
        foreach (KeyValuePair<string, float> entry in temp)
        {
            if (entry.Value < Time.time)
            {
                runningCDSs.Remove(entry.Key);
            }
        }

        myUIElements["Tot Messages"] = "Tot# Mess: " + messageCounter.ToString();


        if (runningCDSs.Count > 0)
            myUIElements["CDS Running"] = "CDS Running";
        else
        {
            myUIElements["CDS Running"] = "";
        }
    }


    //--------------------------Algorithm Functions--------------------------------
    public override void showRunningGUI()
    {
        GUI.Box(new Rect(5, Screen.height / 2-150, 220, 520), "Network Options");
        GUILayout.BeginArea(new Rect(10, Screen.height / 2 - 145, 200,500));
        GUILayout.Space(30);

        useLatency = GUILayout.Toggle(useLatency, "Latency Enabled");
        drawLine = GUILayout.Toggle(drawLine, "Draw Network Connections?");
        if (drawLine)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            adaptiveNetworkColor = GUILayout.Toggle(adaptiveNetworkColor, "Adaptive Color");
            GUILayout.EndHorizontal();
        }
        GUILayout.Label("Weight Factor:", GUILayout.Width(120));
        weightFactor = GUILayout.TextField(weightFactor, 4);
        GUILayout.Label("Freshness Factor:", GUILayout.Width(120));
        newTrailInfluence = GUILayout.TextField(newTrailInfluence, 4);
        GUILayout.Label("Local Factor:", GUILayout.Width(120));
        localUpdate= GUILayout.TextField(localUpdate, 4);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Source Node: ", GUILayout.Width(100));
        GUILayout.Label(sourceStr, GUILayout.Width(100));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Node to Find", GUILayout.Width(100));
        nodeToFindString = GUILayout.TextField(nodeToFindString, GUILayout.Width(30));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Time Required: ", GUILayout.Width(100));
        GUILayout.Label(timeToFind.ToString(), GUILayout.Width(100));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Number of Hops: ", GUILayout.Width(100));
        GUILayout.Label(numHops.ToString(), GUILayout.Width(100));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Find", GUILayout.Width(140)))
        {
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

            foreach (GameObject node in nodes)
            {
                if (node)
                    node.renderer.material.color = Color.blue;

            }
            nodeToFindID = int.Parse(nodeToFindString);
            foundTime = 0;
            startTime = Time.time;
            endTime = 0;

            if (nodeToFindID < simValues.numNodes)
            {
                nodeToFind = GameObject.Find("Node " + nodeToFindID);
                source.GetComponent<AODV>().discoverPath(nodeToFind);
                nodeToFind.renderer.material.color = Color.magenta;


            }
        }
        if (GUILayout.Button("Send Message", GUILayout.Width(140)))
        {
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

            foreach (GameObject node in nodes)
            {
                if (node)
                    node.renderer.material.color = Color.blue;

            }
            nodeToFindID = int.Parse(nodeToFindString);
            foundTime = 0;
            startTime = Time.time;
            endTime = 0;

            if (nodeToFindID < simValues.numNodes)
            {
                nodeToFind = GameObject.Find("Node " + nodeToFindID);
                source.GetComponent<AODV>().initMessage(nodeToFind, "mess", "Test Message");
                nodeToFind.renderer.material.color = Color.magenta;

            }
        }

        if (GUILayout.Button(startStr, GUILayout.Width(140)))
        {
            if (start)
            {
                startStr = "Stop";
            }
            else startStr = "Start";
            start = !start;
        }

        if (GUILayout.Button("Build CDS", GUILayout.Width(140)))
        {
              GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

              foreach (GameObject node in nodes)
              {
                  node.renderer.material.color = Color.white;
              }
            int rand = Random.Range(0, simValues.numNodes);
            GameObject.Find("Node " + rand).GetComponent<ACOVB>().generateAntCDS();
        }
        displayCDS = GUILayout.Toggle(displayCDS, "Display CDS");


        GUILayout.EndVertical();
        GUILayout.EndArea();
    }


    private void displayCurrentCDS()
    {
        if (currentCDS != null)
        {
            //clear current graphics
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
            foreach (GameObject node in nodes)
            {
                //node.renderer.material.color = Color.blue;
                node.GetComponent<ACOVB>().connected = false;
                node.GetComponent<ACOVB>().VBlines.Clear();
            }

            GameObject[] lines = GameObject.FindGameObjectsWithTag("VBLine");
            foreach (GameObject line in lines)
            {
                Destroy(line);
            }

            List<GameObject> openList = new List<GameObject>(currentCDS.getInCDS());
            List<GameObject> edgeList = new List<GameObject>(currentCDS.getEdgeCDS());
            List<GameObject> closedList = new List<GameObject>();
            GameObject checkNode = openList[(int)openList.Count /2];
            openList.Remove(checkNode);
            closedList.Add(checkNode);

            while (openList.Count > 0)
            {
                foreach (GameObject neighbor in checkNode.GetComponent<ACOVB>().neighbors)
                {
                    if (openList.Contains(neighbor))
                    {


                        neighbor.GetComponent<ACOVB>().connected = true;
                        GameObject line = (GameObject)GameObject.Instantiate(simValues.linePrefab);
                        line.tag = "VBLine";
                        line.name = "VBline_" + checkNode.GetComponent<NodeController>().idNum.ToString() +
                            neighbor.GetComponent<NodeController>().idNum.ToString();
                        line.transform.parent = checkNode.transform;
                        line.GetComponent<LineRenderer>().SetColors(Color.black, Color.black);
                        line.GetComponent<LineRenderer>().SetWidth(3, 3);
                        checkNode.GetComponent<ACOVB>().VBlines.Add(neighbor, line);
                        openList.Remove(neighbor);
                        closedList.Add(neighbor);
                    }

                }
                if (openList.Count > 0)
                {
                    if (closedList.Count > 0)
                    {
                        checkNode = closedList[0];
                        closedList.Remove(checkNode);
                    }

                }
            }


        }
    }



}
