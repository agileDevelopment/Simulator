using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ACOVBGUI : AODVGUI {
    public object myLock;
    public CDS currentCDS;
    public int maxCDS;
    public float weightFactor;
    public float newTrailInfluence;
    public float localUpdate;
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
        weightFactor = .8f;
        newTrailInfluence = .8f;
        localUpdate = .8f;
        myUIElements.Add("pLevel", "");
        myUIElements.Add("CDS Running", "");

        

	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
        counter++;
        if (counter % 2 == 0)
        {
            if (displayCDS)
            {
                if (supervisor.GetComponent<ACOVB>().myCurrentCDS != null)
                {

                    currentCDS = supervisor.GetComponent<ACOVB>().myCurrentCDS;
                    if (currentCDS != null)
                    {
                        displayCurrentCDS();
                    }
                }else
            {
                print("nope, cds is blanks");
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

    private void displayCurrentCDS()
    {

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
        GUILayout.Label("Weight Factor: " + weightFactor.ToString(), GUILayout.Width(200));
        weightFactor = GUILayout.HorizontalSlider(weightFactor, 0, 1);
        GUILayout.Label("Freshness Factor: " + newTrailInfluence.ToString(), GUILayout.Width(200));
        newTrailInfluence = GUILayout.HorizontalSlider(newTrailInfluence, 0, 1);
        GUILayout.Label("Local Factor: " + localUpdate.ToString(), GUILayout.Width(200));
        localUpdate = GUILayout.HorizontalSlider(localUpdate, 0,1);
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
           supervisor.GetComponent<ACOVB>().generateAntCDS();
        }
        displayCDS = GUILayout.Toggle(displayCDS, "Display CDS");
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

}
