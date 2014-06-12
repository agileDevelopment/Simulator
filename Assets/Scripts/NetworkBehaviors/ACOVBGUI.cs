//-----------------Header-------------------------
//  Title: ACOVBGUI.cs
//  Date: 6-7-2014
//  Version: 3.4
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//  Class Dependicies: AODVGUI
//

//  Description: Establishes and stores algorithm parameters and data logging functions.  
//  Delegates algorithm implementation to ACOVB script running on every node.


//  Extends AODVGUI (which Implements INetworkBehavior)
//
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ACOVBGUI : AODVGUI {
    //------------------Class variables---------------------
    #region fields
    public object myLock;
    public CDS currentCDS;
    public int maxCDS;
    public float weightFactor;
    public float newTrailInfluence;
    public float localUpdate;
    public bool enableCDS = false;
    public Dictionary<string, float> runningCDSs;
    GameObject displayNode;
    public int recBroadcasts=0;
    public bool enableTest;
    public float packetClock = 0f;
    public float packetHopCounter = 0f;
    public int testCycle=0;


    //Logging and experiments setting variables
    List<ACOLogData> logdata;
    ACOLogData avgEntry;
    System.IO.StreamWriter outputFile;
    Dictionary<int, ACOLogData> avgdata;
    string fileName = "";
    string path = "";
    public string iterationsStr = "";
    float stoptime = 0;
    public int timeCounter = 0;
    int maxIterations;
    int iteration;
    int counter = 0;
    bool log = true;
    bool runningSim = false;
    public int CDSCounter;
    public int numOfCDS;
    public int time = 0;
    public bool reset = false;
 


    #endregion
    //------------------Unity Functions--------------------------
    #region Unity Functions
    // Use this for initialization
	protected override void Start () {
        base.Start();
        maxCDS = 0;
        myLock = new object();
        currentCDS = null;
        runningCDSs = new Dictionary<string, float>();
        weightFactor = .75f;
        newTrailInfluence = .5f;
        localUpdate = .5f;
        myUIElements.Add("Broadcasts", "");
        myUIElements.Add("pLevel", "");
        myUIElements.Add("CDS Running", "");
        iterationsStr = "30";
        enableTest = false;
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (runningSim && (!enableCDS ||currentCDS != null ))
        {
            counter++;

            if (log && runningSim)
            {
                if (Time.time >= stoptime)
                {
                    timeCounter = timeCounter + 3;
                    stoptime = Time.time + 3;
                    logData(timeCounter);


                }
                if (simValues.simRunTime != 0)
                {
                    if (Time.time > startTime + simValues.simRunTime)
                    {
                        finishIteration(logdata);
                    }
                }
            }
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
        myUIElements["Broadcasts"] = "Broadcasts: " + broadcastCounter.ToString();

        if (runningCDSs.Count > 0)
            myUIElements["CDS Running"] = "CDS Running";
        else
        {
            myUIElements["CDS Running"] = "";
        }


    }
    #endregion


    //--------------------Graphic updates----------------------
    #region Graphic Updates
    private void displayCurrentCDS()
    {
        //for future use
    }
    public override void showRunningGUI()
    {
        GUI.Box(new Rect(5, Screen.height / 2-150, 220, 600), "Network Options");
        GUILayout.BeginArea(new Rect(10, Screen.height / 2 - 145, 200,580));
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
            log = GUILayout.Toggle(log, "Logging Enabled");
            enableTest = GUILayout.Toggle(enableTest, "Testing enabled");
            enableCDS = GUILayout.Toggle(enableCDS, "Enable CDS");
            GUILayout.BeginHorizontal();
                GUILayout.Label("Num Iter:", GUILayout.Width(120));
                iterationsStr = GUILayout.TextField(iterationsStr, 3);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Begin CDS Test", GUILayout.Width(140)))
            {
                testCycle = 1;
                enableCDS = true;
                startCDS();
            }
            if (GUILayout.Button("Begin No CDS Test", GUILayout.Width(140)))
            {
                testCycle = 1;
                startCDS();
            }
            if (GUILayout.Button("Print CDS", GUILayout.Width(140)))
            {
                if (currentCDS != null)
                {
                    foreach (GameObject node in currentCDS.getInCDS())
                    {
                        print(node.name);
                    }
                }
            }
            GUILayout.Label("On Iteration:  " + iteration + "/" + iterationsStr, GUILayout.Width(120));
            if (currentCDS != null)
                GUILayout.Label("Size of CDS: " + currentCDS.getInCDS().Count);
        GUILayout.EndArea();
    }

    #endregion 
    //--------------------Experiment Functions--------------------
    #region Experiment Functions
    //Sets up file for data logging
    void startCDS()
    {
        initializeTestParameters(testCycle);
        if (log)
        {
            
            GridGUI flightGUI = GameObject.Find("Spawner").GetComponent<GridGUI>();
            fileName = "Test"+testCycle+"- nodes" + simValues.numNodes + "CDS" + enableCDS +  "- Sp" + flightGUI.nodeMaxSpeed+ "- wt"+weightFactor + "- newFac"+newTrailInfluence+ "- LU"+localUpdate+ ".txt";
            path = Application.dataPath + "/../ACO Test Data/";
            avgdata = new Dictionary<int, ACOLogData>();
            outputFile = new System.IO.StreamWriter(path + fileName);
            maxIterations = int.Parse(iterationsStr);
            iteration = 1;
            runningSim = true;
        }
        startTestEvent();
    }


    void startTestEvent(){
        drawLine = false;
        enableTest = true;
        logdata = new List<ACOLogData>();
        timeCounter = 0;
        time = 0;
        CDSCounter = 0;
        numOfCDS = 0;
        stoptime = Time.time + 3;
        startTime = Time.time;
    }

    void endTestCycle()
    {
        enableTest = false;
        if (enableCDS)
        {
            enableCDS = false;
                 startCDS();
        }
        else
        {
            enableCDS = true;
            testCycle++;
            if (testCycle < 10)
            {
                startCDS();
            }

            else
            {
                log = false;
                runningSim = false;
            }
        }

    }
    void initializeTestParameters(int test)
    {
        switch (test)
        {
            case 1:
                weightFactor=.5f;
                newTrailInfluence=.5f;
                localUpdate=.5f;
                break;
            case 2:
                weightFactor = .5f;
                newTrailInfluence = .5f;
                localUpdate = .75f;
                break;
            case 3:
                weightFactor = .5f;
                newTrailInfluence = .5f;
                localUpdate = .9f;
                break;
            case 4:
                weightFactor = .5f;
                newTrailInfluence = .75f;
                localUpdate = .5f;
                break;
            case 5:
                weightFactor = .5f;
                newTrailInfluence = .9f;
                localUpdate = .5f;
                break;
            case 6:
                weightFactor = .75f;
                newTrailInfluence = .5f;
                localUpdate = .5f;
                break;
            case 7:
                weightFactor = .9f;
                newTrailInfluence = .5f;
                localUpdate = .5f;
                break;
            case 8:
                weightFactor = .75f;
                newTrailInfluence = .75f;
                localUpdate = .75f;
                break;
            case 9:
                weightFactor = .9f;
                newTrailInfluence = .9f;
                localUpdate = .9f;
                break;
            default:
                weightFactor=.5f;
                newTrailInfluence=.5f;
                localUpdate=.5f;
                break;

        }
    }
    #endregion
    //--------------------Log Functions-------------------------
    #region Log Functions
    private void logData(int time)
    {
        if (iteration == 1)
        {
            ACOLogData newEntry = new ACOLogData();
            avgdata.Add(time, newEntry);
        }
        if (avgdata.ContainsKey(time))
        {
            avgEntry = avgdata[time];
            ACOLogData entry = new ACOLogData();

            avgEntry.time = entry.time = time;
            avgEntry.totBroadcasts += entry.totBroadcasts = (int)broadcastCounter;
            avgEntry.recBroadcasts += entry.recBroadcasts = (int)recBroadcasts;
            avgEntry.bCastsPerSec += entry.bCastsPerSec = (float)broadcastCounter / 3;
            avgEntry.currentCDSsize += currentCDS.getInCDS().Count;
            avgEntry.lostPackets += entry.lostPackets = (int)broadcastCounter - recBroadcasts;
            avgEntry.avgPacketTime += entry.avgPacketTime = packetClock / recBroadcasts;
            avgEntry.avgPacketHops += entry.avgPacketHops = packetHopCounter / recBroadcasts;

            logdata.Add(entry);
            broadcastCounter = 0;
            recBroadcasts = 0;
            packetClock = 0f;
            packetHopCounter = 0f;
            CDSCounter = 0;
            numOfCDS = 0;

        }

    }
    public void updateLogData(float pclock, int hopCounter)
    {
        lock (myLock)
        {
            recBroadcasts++;
            packetClock += pclock;
            packetHopCounter += hopCounter;
        }
    }


    private void finishIteration(List<ACOLogData> temp)
    {

        string timeLine ="Time: \t";
        string totBcastsLine = "Total Broadcasts = \t";
        string recBcastsLine = "Recieved Broadcasts =  \t";
        string bCastsPerSecLine = "Broadcast per sec \t";
        string currentCDSsizeLine = "CDS Size \t";
        string lostPacketsLine = "Lost Packets \t";
        string avgPacketTimeLine = "Delivery Time \t";
        string avgPacketHopsLine = "Packet Hops \t";

        //foreach (ACOLogData entry in logdata)
        //{
        //    timeLine += entry.time + ",";
        //    totBcastsLine += entry.totBroadcasts + ",";
        //    recBcastsLine += entry.recBroadcasts + ",";
        //    bCastsPerSecLine += entry.bCastsPerSec + ",";
        //    currentCDSsizeLine += entry.currentCDSsize + ",";
        //    lostPacketsLine += entry.lostPackets + ",";
        //    avgPacketTimeLine += entry.avgPacketTime + ",";
        //    avgPacketHopsLine += entry.avgPacketHops + ",";
        //}

        iteration++;

        if (iteration < maxIterations)
        {
            startTestEvent();
        }
        if (iteration == maxIterations)
        {
            foreach (KeyValuePair<int, ACOLogData> entryData in avgdata)
            {
                ACOLogData entry = entryData.Value;
                timeLine += entry.time + "\t";
                totBcastsLine += ((float)entry.totBroadcasts / avgdata.Count).ToString() + "\t";
                recBcastsLine += ((float)entry.recBroadcasts / avgdata.Count).ToString() + "\t";
                bCastsPerSecLine += ((float)entry.bCastsPerSec / avgdata.Count).ToString() + "\t";
                currentCDSsizeLine += (entry.currentCDSsize).ToString() + "\t";
                lostPacketsLine += ((float)entry.lostPackets / avgdata.Count).ToString() + "\t";
                avgPacketTimeLine += ((float)entry.avgPacketTime).ToString() + "\t";
                avgPacketHopsLine += ((float)entry.avgPacketHops).ToString() + "\t";
            }
            outputFile.WriteLine();
            outputFile.WriteLine(fileName);
            outputFile.WriteLine(timeLine);
            outputFile.WriteLine(totBcastsLine);
            outputFile.WriteLine(recBcastsLine);
            outputFile.WriteLine(bCastsPerSecLine);
            outputFile.WriteLine(lostPacketsLine);
            outputFile.WriteLine(avgPacketTimeLine);
            outputFile.WriteLine(avgPacketHopsLine);
            if (enableCDS)
            {
                outputFile.WriteLine(currentCDSsizeLine);
            }

            outputFile.Close();
            endTestCycle();


        }
    }
    #endregion
}

//------------------------------------------------------------
//  Title: Logdata
//  Date: 5-26-2014
//  Version: 3.4
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None

//  Description: Helper class to store data used in the log.

//
//--------------------------------------------------------------
public class ACOLogData
{

    public int time = 0;
    public int totBroadcasts = 0;
    public int recBroadcasts = 0;
    public float bCastsPerSec = 0f;
    public float currentCDSsize = 0;
    public int lostPackets = 0;
    public float avgPacketTime = 0f;
    public float avgPacketHops = 0f;


    public ACOLogData() { }

    public ACOLogData(ACOLogData a)
    {
     time = a.time;
     totBroadcasts = a.totBroadcasts;
    recBroadcasts = a.recBroadcasts;
    bCastsPerSec = a.bCastsPerSec;
     currentCDSsize = a.currentCDSsize;
    lostPackets = a.lostPackets;
    avgPacketTime = a.avgPacketTime;
    avgPacketHops = a.avgPacketHops;
    }

}
