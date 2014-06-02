using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ACOVBGUI : AODVGUI {
    public object myLock;
    public CDS currentCDS;
    public string weightFactor;
    public string newTrailInfluence;
    public string localUpdate;
    public string startStr = "Start";
    float maxPheremoneLevel;
    public bool start= false;
    public bool displayCDS = false;
    int counter = 0;

	// Use this for initialization
	protected override void Start () {
        base.Start();
        myLock = new object();
        currentCDS = null;
        maxPheremoneLevel = 0f;
        weightFactor = ".8";
        newTrailInfluence = ".8";
        localUpdate = ".8";

	}
	
	// Update is called once per frame
    protected override void Update()
    {
        counter++;
        if (counter % 2 == 0)
        {
        //    displayCurrentCDS();
            counter = 0;
        }
	}


    //--------------------------Algorithm Functions--------------------------------
    public override void showRunningGUI()
    {
        GUI.Box(new Rect(5, Screen.height / 2, 220, 420), "Network Options");
        GUILayout.BeginArea(new Rect(10, Screen.height / 2 + 5, 200, 400));
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
      //  GUILayout.Label("Local Factor:", GUILayout.Width(120));
     //   localUpdate= GUILayout.TextField(localUpdate, 4);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Source Node: ", GUILayout.Width(100));
        GUILayout.Label(sourceStr, GUILayout.Width(100));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Node to Find", GUILayout.Width(100));
        nodeToFindString = GUILayout.TextField(nodeToFindString, GUILayout.Width(30));
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
                source.GetComponent<AODV>().initMessage(nodeToFind, "Test Message");
                nodeToFind.renderer.material.color = Color.magenta;

            }
        }
        if (GUILayout.Button("Send Ant", GUILayout.Width(140)))
        {
            if (nodeToFindID < simValues.numNodes)
            {
                nodeToFind = GameObject.Find("Node " + nodeToFindID);
                source.GetComponent<ACOVB>().startAnt(nodeToFind);
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
         //   GameObject.Find("Node " + rand).GetComponent<ACOVB>().generateAntCDS();
            GameObject.Find("Node 10").GetComponent<ACOVB>().generateAntCDS();
        }
        displayCDS = GUILayout.Toggle(displayCDS, "Display CDS");


        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

   


}
