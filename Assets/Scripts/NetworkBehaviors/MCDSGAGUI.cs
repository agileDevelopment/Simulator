//-----------------Header-------------------------
//  Title: MCDSAGUI.cs
//  Date: 5-26-2014
//  Version: 3.3
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: OrbitGUI

//  Description: Establishes and stores algorithm parameters and data.  
//  Delegates algorithm implementation to MCDSGA script running on every node.


//  Extends AODVGUI (which Implements INetworkBehavior)
//
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MCDSGAGUI : AODVGUI
{
    //------------------Class variables---------------------
    #region fields
    public CDS currentCDS;
    public List<CDS> population;
    public object lockGUI = new Object();
    List<LogData> logdata;
    LogData avgEntry;
    System.IO.StreamWriter outputFile;
    Dictionary<int, LogData> avgdata;
    
    public float bestValue;
    float deltaTime = 0;
    
    public string maxPop = "20";
    public string crossoverCountStr = "20";
    public string iterationsStr = "30";
    public string currentCDSstr;
    string fileName = "";
    string path = "";

    public int crossoverCount = 20;
    public int timeCounter = 0;
    public int totalGenerations = 0;
    public int generations = 0;
    public int rejects = 0;
    public int additions = 0;
    public int sizeOfPopulation;
    public int mutate = 0;
    int logCounter = 1;
    int logInc = 1;
    int maxIterations;
    int iteration;
    int counter = 0;
   
    bool log = true;
    bool runningSim = false;
    bool enableMovement = false;
    bool showCDSlines = false;
    bool firstCDS = true;
    #endregion 

    //--------------------Unity Functions-----------------------
    #region Unity Functions


    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        setOptions();

        mutate = 0;
        rejects = 0;
        population = new List<CDS>();
       
        useDefaultLine = false;
        useDefaultLineStr = " Default Lines Disabled";
        maxPop = "20";

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (runningSim)
        {
            counter++;
            timeCounter++;
            sizeOfPopulation = population.Count;

            GameObject.Find("Node 0").GetComponent<MCDSGA>().crossover(population);

            if (log && runningSim)
            {
                deltaTime += Time.deltaTime;
                if (timeCounter == logCounter)
                {
                    logCounter = timeCounter + logInc;
                    logInc++;
                    deltaTime = 0;
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

            if (counter % 2 == 0)
            {
                CDS best = bestCDS();
                if (population.Contains(best))
                {
                    if (!GameObject.Find("Node 0").GetComponent<MCDSGA>().checkFeasibility(best))
                    {
                        population.Remove(best);
                        counter = 0;
                    }
                    else
                    {
                        currentCDS = best;
                    }

                }
                if (showCDSlines)
                    displayCurrentCDS();
                counter = 0;

                CDS toAdd = GameObject.Find("Node 0").GetComponent<MCDSGA>().generateCDS();
                addToPopulation(toAdd);
            }
        }
    }
    #endregion
    //--------------------Log Functions-------------------------
    #region Log Functions
    private void logData(int time)
    {
        if (iteration == 0)
        {
            LogData newEntry = new LogData();
            avgdata.Add(time, newEntry);
        }
        if (avgdata.ContainsKey(time))
        {
            avgEntry = avgdata[time];
            LogData entry = new LogData();
            totalGenerations += generations;


            avgEntry.time = entry.time = time;
            avgEntry.fitness += entry.fitness = bestValue;
            avgEntry.tGenerations += entry.tGenerations = totalGenerations;
            avgEntry.generations += entry.generations = generations;
            avgEntry.mutations += entry.mutations = mutate;
            avgEntry.additions += entry.additions = additions;
            avgEntry.rejections += entry.rejections = rejects;
            logdata.Add(entry);
            mutate = 0;
            additions = 0;
            rejects = 0;
            generations = 0;
        }

    }
    private void finishIteration(List<LogData> temp)
    {

        string timeLine = "Iter: " + iteration + ", Time: ,";
        string fitLine = "  ,F(x) = ,";
        string genLine = "  ,Generations ,";
        string tGenLine = "  ,Total Gens ,";
        string mutLine = "  ,Mutations ,";
        string addLine = " ,Adds ,";
        string rejectLine = " ,Rejects ,";

        foreach (LogData entry in logdata)
        {
            timeLine += entry.time + ",";
            fitLine += entry.fitness + ",";
            tGenLine += entry.tGenerations + ",";
            genLine += entry.generations + ",";
            mutLine += entry.mutations + ",";
            addLine += entry.additions + ",";
            rejectLine += entry.rejections + ",";
        }
        if (iteration == 0)
        {
            fileName = "num" + simValues.numNodes + "- ran" + nodeCommRange + " - dur" + simValues.simRunTime + "- it" + iterationsStr + " - gen" + crossoverCountStr + " - max" + maxPop + " - finish.png";
            displayCurrentCDS();
            Application.CaptureScreenshot(path + fileName);
        }

        iteration++;

        if (iteration < maxIterations)
        {
            initializeEvent();
        }
        outputFile.WriteLine();
        outputFile.WriteLine(timeLine);
        outputFile.WriteLine(fitLine);
        outputFile.WriteLine(genLine);
        outputFile.WriteLine(tGenLine);
        outputFile.WriteLine(mutLine);
        outputFile.WriteLine(addLine);
        outputFile.WriteLine(rejectLine);

        if (iteration >= maxIterations)
        {
            timeLine = "AVG: , Time: ,";
            fitLine = "  ,Avg F(x) = ,";
            genLine = "  ,Avg Generations ,";
            tGenLine = "  ,Avg Total Gens ,";
            mutLine = "  ,Avg Mutations ,";
            addLine = " ,Avg Adds ,";
            rejectLine = " ,Avg Rejects ,";

            foreach (KeyValuePair<int, LogData> entryData in avgdata)
            {
                LogData entry = entryData.Value;
                timeLine += entry.time + ",";
                fitLine += (float)entry.fitness / maxIterations + ",";
                tGenLine += (float)entry.tGenerations / maxIterations + ",";
                genLine += (float)entry.generations / maxIterations + ",";
                mutLine += (float)entry.mutations / maxIterations + ",";
                addLine += (float)entry.additions / maxIterations + ",";
                rejectLine += (float)entry.rejections / maxIterations + ",";
            }
            outputFile.WriteLine();
            outputFile.WriteLine(timeLine);
            outputFile.WriteLine(fitLine);
            outputFile.WriteLine(genLine);
            outputFile.WriteLine(tGenLine);
            outputFile.WriteLine(mutLine);
            outputFile.WriteLine(addLine);
            outputFile.WriteLine(rejectLine);

            log = false;
            runningSim = false;
            outputFile.Close();


        }
    }
    #endregion
    //---------------------Graphic updates----------------------
    #region Graphic Updates
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
        showCDSlines = GUILayout.Toggle(showCDSlines, "Draw CDS Connections");

        GUILayout.BeginVertical();
        GUILayout.Space(30);

        GUILayout.BeginArea(new Rect(10, 120, 160, 300));
        {
            GUILayout.Label("MCDSGA Test Parameters", GUILayout.Width(120));
            if (!runningSim)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("X_over Iter:", GUILayout.Width(120));
                crossoverCountStr = GUILayout.TextField(crossoverCountStr, 4);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Pop:", GUILayout.Width(120));
                maxPop = GUILayout.TextField(maxPop, 3);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Num Iter:", GUILayout.Width(120));
                iterationsStr = GUILayout.TextField(iterationsStr, 3);
                GUILayout.EndHorizontal();
                log = GUILayout.Toggle(log, "Logging Enabled");
                enableMovement = GUILayout.Toggle(enableMovement, "Enable Movement");
            }
            if (runningSim)
            {
                GUILayout.Label("X_over Iter:" + crossoverCountStr, GUILayout.Width(120));
                GUILayout.Label("Max Pop:" + maxPop, GUILayout.Width(120));
                GUILayout.Label("On Iteration:  " + (iteration + 1) + "/" + iterationsStr, GUILayout.Width(120));
                GUILayout.Label("Fitness:  " + bestValue, GUILayout.Width(120));
            }
            GUILayout.Space(30);
            if (GUILayout.Button("Start CDS", GUILayout.Width(140)))
            {
                startCDS();
            }

            GUILayout.EndVertical();
        }
        GUILayout.EndArea();
        GUILayout.EndArea();
    }

    private void screenshot()
    {
        fileName = "num" + simValues.numNodes + "- ran" + nodeCommRange + " - dur" + simValues.simRunTime + "- it" + iterationsStr + " - gen" + crossoverCountStr + " - max" + maxPop + " - start.png";
            displayCurrentCDS();
            Application.CaptureScreenshot(path + fileName);
 
    }


    private void displayCurrentCDS()
    {
        lock (lockGUI)
        {
            //clear current graphics
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
            foreach (GameObject node in nodes)
            {
                node.renderer.material.color = Color.blue;
                node.GetComponent<MCDSGA>().connected = false;
                node.GetComponent<MCDSGA>().VBlines.Clear();
            }

            GameObject[] lines = GameObject.FindGameObjectsWithTag("VBLine");
            foreach (GameObject line in lines)
            {
                Destroy(line);
            }


            List<GameObject> openList = new List<GameObject>(currentCDS.getInCDS());
            List<GameObject> edgeList = new List<GameObject>(currentCDS.getEdgeCDS());
            List<GameObject> closedList = new List<GameObject>();
            GameObject checkNode = openList[currentCDS.owner.GetComponent<NodeController>().idNum];
            openList.Remove(checkNode);
            closedList.Add(checkNode);

            while (openList.Count > 0)
            {
                foreach (GameObject neighbor in checkNode.GetComponent<MCDSGA>().neighbors)
                {
                    if (openList.Contains(neighbor))
                    {


                        neighbor.renderer.material.color = Color.green;
                        neighbor.GetComponent<MCDSGA>().connected = true;
                        GameObject line = (GameObject)GameObject.Instantiate(simValues.linePrefab);
                        line.tag = "VBLine";
                        line.name = "VBline_" + checkNode.GetComponent<NodeController>().idNum.ToString() +
                            neighbor.GetComponent<NodeController>().idNum.ToString();
                        line.transform.parent = checkNode.transform;
                        line.GetComponent<LineRenderer>().SetColors(Color.black, Color.black);
                        line.GetComponent<LineRenderer>().SetWidth(3, 3);
                        checkNode.GetComponent<MCDSGA>().VBlines.Add(neighbor, line);
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

            foreach (GameObject node in edgeList)
            {
                foreach (GameObject myNeighbor in node.GetComponent<MCDSGA>().neighbors)
                {
                    if (currentCDS.getInCDS().Contains(myNeighbor))
                    {
                      //  node.renderer.material.color = Color.red;
                        node.GetComponent<MCDSGA>().connected = true;
                        GameObject line = (GameObject)GameObject.Instantiate(simValues.linePrefab);
                        line.tag = "VBLine";
                        line.name = "VBline_" + checkNode.GetComponent<NodeController>().idNum.ToString() +
                            myNeighbor.GetComponent<NodeController>().idNum.ToString();
                        line.transform.parent = myNeighbor.transform;
                        line.GetComponent<LineRenderer>().SetColors(Color.blue, Color.blue);
                        line.GetComponent<LineRenderer>().SetWidth(2, 2);
                        myNeighbor.GetComponent<MCDSGA>().VBlines.Add(node, line);
                        break;
                    }
                }
            }
        }
    }


    #endregion
    //---------------------MCDSGA functions-------------------
    #region MCDSGA Functions


    //------------------------------------------------------------
    //  Function: addToPopulation()
    //  Algorithm: MCDS - GA
    //  Date: 5-26-2014
    //  Version: 3
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //
    //  Class Dependicies: CDS, MCDSGA
    //
    //  Parameters: CDS 
    //
    //  Order-of: O(n) : n = max population size to search through
    //
    //  Description:  takes input CDS and performs a lookup on the current population to see if its better
    //  than the worst CDS.  If it is, it removes the worst and adds the input CDS to the current population
    //
    //--------------------------------------------------------------

    public void addToPopulation(CDS cdsToAdd)
    {
        lock (lockGUI)
        {
            if (firstCDS && iteration == 0 && cdsToAdd != null)
            {
                currentCDS = cdsToAdd;
                bestValue = cdsToAdd.getFitness();
                screenshot();
                firstCDS = false;
            }
            if (!population.Contains(cdsToAdd))
            {
                if (population.Count >= int.Parse(maxPop))
                {
                    CDS worst = worstCDS();
                    if (worst != null)
                    {
                        if (cdsToAdd.getFitness() > worst.getFitness())
                        {
                            population.Remove(worst);
                            population.Add(cdsToAdd);
                            additions++;
                        }
                    }
                    else
                    {
                        population.Add(cdsToAdd);
                        additions++;
                    }
                }
                else
                {
                    population.Add(cdsToAdd);
                    additions++;
                }
            }

        }

    }

    public CDS bestCDS()
    {
        lock (lockGUI)
        {
            CDS best = null;
            float maxValue = 0f;
            foreach (CDS val in population)
            {
                if (val.getFitness() > maxValue)
                {
                    maxValue = val.getFitness();
                    best = val;
                }
            }
            if (best != null)
                bestValue = best.getFitness();
            return best;
        }
    }

    public CDS worstCDS()
    {
        lock (lockGUI)
        {
            CDS worst = null;
            float minValue = 100000f;
            foreach (CDS val in population)
            {
                if (val.getFitness() < minValue)
                    minValue = val.getFitness();
                worst = val;
            }
            return worst;
        }
    }
    public void genPopulation()
    {
        lock (lockGUI)
        {
            population.Clear();
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
            for (int i = 0; i < 10; i++)
            {
                addToPopulation(nodes[i].GetComponent<MCDSGA>().generateCDS());
            }
        }
    }
    void startCDS()
    {
        if (log)
        {
            fileName = "num" + simValues.numNodes + "- ran" + nodeCommRange + " - dur" + simValues.simRunTime + "- it" + iterationsStr + " - gen" + crossoverCountStr + " - max" + maxPop + ".txt";
            path = Application.dataPath + "/../MCDSGA Data/";
            avgdata = new Dictionary<int, LogData>();
            outputFile = new System.IO.StreamWriter(path + fileName);
            maxIterations = int.Parse(iterationsStr);
            iteration = 0;
            runningSim = true;
            if (!enableMovement)
                simValues.paused = true;
            GridGUI flightGUI = GameObject.Find("Spawner").GetComponent<GridGUI>();
            outputFile.WriteLine("Number of Nodes: " + simValues.numNodes + " - Network Range: " + nodeCommRange + " - Spacing: " +
                flightGUI.nodeSpacingString + " - Orbit Radius: " + flightGUI.radius + " - Speed: " + flightGUI.nodeMaxSpeed
                + " - Max Population" + maxPop + " - Generations/Frame: " + crossoverCountStr);
        }
        initializeEvent();
    }
    void initializeEvent()
    {
        logdata = new List<LogData>();
        timeCounter = 0;
        logCounter = 1;
        logInc = 1;
        generations = 0;
        totalGenerations = 0;
        mutate = 0;
        rejects = 0;
        additions = 0;
        startTime = Time.time;
        genPopulation();
    }




    #endregion
}
//---------------------Supprot Classes
#region Support Classes


public class CDS 
{
    //-------------------Comments-------------------------
    //  Title: CDS
    //  Date: 5-26-2014
    //  Version: 3.3
    //  Project: UAV Swarm
    //  Authors: Corey Willinger
    //  OS: Windows x64/X86
    //  Language:C#
    //
    //  Class Dependicies: LoadOptionsGUI

    //  Description: Class object to store Connected Dominating set solutions. Also stores
    //  Genetic Algorithm Encoding. 

    //
    //--------------------------------------------------------------
    public Object CDSLock;
    LoadOptionsGUI simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
    public bool[] gaEncoding;
    List<GameObject> inCDS;
    List<GameObject> outCDS;
    List<GameObject> edgeCDS;
    public Dictionary<GameObject, List<GameObject>> network;
    public GameObject owner;
    float fitness;
    public int size;

    public CDS(GameObject owner_)
    {
        CDSLock = new Object();
        owner = owner_;
        fitness = 0;
        size = 0;
        inCDS = new List<GameObject>();
        outCDS = new List<GameObject>();
        edgeCDS = new List<GameObject>();
        network = new Dictionary<GameObject, List<GameObject>>();

        //init all nodes to the out of CDS list
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach (GameObject node in nodes)
        {
            outCDS.Add(node);
        }
        //init genetic encoding to all 0's
        gaEncoding = new bool[simValues.numNodes];
        for (int i = 0; i < simValues.numNodes; i++)
        {
            gaEncoding[i] = false;
        }

    }

    public CDS(CDS toCopy)
    {
        CDSLock = new Object();
        owner = toCopy.owner;
        fitness = 0;
        size = 0;
        gaEncoding = toCopy.gaEncoding;
        inCDS = new List<GameObject>(toCopy.inCDS);
        outCDS = new List<GameObject>(toCopy.outCDS);
        edgeCDS = new List<GameObject>(toCopy.edgeCDS);
    }

    public void moveToInCDS(GameObject node)
    {
        lock (CDSLock)
        {
            if (edgeCDS.Contains(node))
            {
                inCDS.Add(node);
                edgeCDS.Remove(node);
                size++;
                gaEncoding[node.GetComponent<NodeController>().idNum] = true;
            }
            else if (outCDS.Contains(node))
            {
                inCDS.Add(node);
                outCDS.Remove(node);
                size++;
                gaEncoding[node.GetComponent<NodeController>().idNum] = true;
            }
            if (outCDS.Count == 0)
            {
                calcFitness();
            }
        }

    }

    public void moveToOutCDS(GameObject node)
    {
        if (edgeCDS.Contains(node))
        {
            outCDS.Add(node);
            edgeCDS.Remove(node);
            gaEncoding[node.GetComponent<NodeController>().idNum] = false;
        }
        else if (inCDS.Contains(node))
        {
            outCDS.Add(node);
            inCDS.Remove(node);
            size--;
            gaEncoding[node.GetComponent<NodeController>().idNum] = false;
        }
        if (outCDS.Count == 0)
        {
            calcFitness();
        }
    }

    public void moveToEdgeCDS(GameObject node)
    {
        if (outCDS.Contains(node))
        {
            edgeCDS.Add(node);
            outCDS.Remove(node);
            if (outCDS.Count == 0)
            {
                calcFitness();
            }
        }
    }
    public bool nodeInCDS(GameObject node)
    {
        if (inCDS.Contains(node))
            return true;
        return false;
    }

    public List<GameObject> getInCDS()
    {
        return inCDS;
    }
    public List<GameObject> getOutCDS()
    {
        return outCDS;
    }
    public List<GameObject> getEdgeCDS()
    {
        return edgeCDS;
    }

    public bool nodeInEdge(GameObject node)
    {
        if (edgeCDS.Contains(node))
            return true;
        return false;
    }

    public bool[] getEncoding()
    {
        return gaEncoding;
    }


    public float getFitness()
    {
        return fitness;
    }
    public void calcFitness()
    {
        fitness = edgeCDS.Count;
    }
}

//------------------------------------------------------------
//  Title: Logdata
//  Date: 5-26-2014
//  Version: 3.3
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None

//  Description: Helper class to store data used in the log.

//
//--------------------------------------------------------------
public class LogData
{
    public LogData() { }

    public LogData(LogData a)
    {
        fitness = a.fitness;
        time = a.time;
        generations = a.generations;
        additions = a.additions;
        rejections = a.rejections;
        mutations = a.mutations;
        tGenerations = a.tGenerations;

    }
    public float fitness = 0;
    public int time = 0;
    public int generations = 0;
    public int additions = 0;
    public int rejections = 0;
    public int mutations = 0;
    public int tGenerations = 0;
}

#endregion