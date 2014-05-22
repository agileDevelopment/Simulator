using UnityEngine;
using System.Collections;

public class NetworkGUI : MonoBehaviour, INetworkGUIOptions
{
    public ComboBox commLinesComboBox;// = new ComboBox();
    public string drawLinesString = "Hide Lines";
    public bool drawLine = false;
    public bool adaptiveNetworkColor = true;
    public int commLinesChoice = 0;
    public int commLinesOn = 0;
    public string nodeCommRangeString = "100";
    public int nodeCommRange;
    public bool updateLines = false;
    public LoadOptionsGUI simValues;
    public GameObject nodeToFind;
    public GameObject source;
    public string nodeToFindString;
    public int nodeToFindID;
    public float timeToFind;
    public int numHops;
    public string nextHop;
    public string sourceStr;
    public float foundTime;
    public float startTime;
    public float endTime;

//----------------Unity Functions------------------------------------

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    //Here we are figuring out whether to update lines or not.
    void Update()
    {

        if (simValues.counter == simValues.slowMoRate)
            {
                updateLines = true;
            }

        
        if (!simValues.slowMotion)
        {
   
            updateLines = false;
            if (simValues.counter >= 64 / (1f / Time.deltaTime * 2))
            {
                updateLines = true;
            }
        }
    }

    void OnGUI()
    {


            if (!simValues.showMainGui)
            {
                GUI.Box(new Rect(0, Screen.height / 2, 175, 400), "Network Options");
                GUILayout.BeginArea(new Rect(10, Screen.height / 2 + 5, 175, 400));
                GUILayout.Space(30);


                if (GUILayout.Button(drawLinesString, GUILayout.Width(140)))
                {
                    if (drawLine)
                    {
                        drawLinesString = "Show Lines";
                    }
                    else drawLinesString = "Hide Lines";
                    drawLine = !drawLine;
                }
            //    if (drawLine)
                    adaptiveNetworkColor = GUILayout.Toggle(adaptiveNetworkColor, "Adaptive Color");

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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Next Hop: ", GUILayout.Width(100));
            GUILayout.Label(nextHop, GUILayout.Width(175));
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
                    source.GetComponent<AODV>().initMessage(nodeToFind);
                    nodeToFind.renderer.material.color = Color.magenta;

                }
            }


            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

//------------------INetworkGUIOptions Functions----------------------
   public void setGuiValues(){
       nodeCommRange = int.Parse(nodeCommRangeString);

    }

   public void showGUI()
   {
        GUI.BeginGroup(new Rect(((Screen.width - simValues.buttonWidth) / 2) + 250, Screen.height / 2 + 50, 250, 100));
        GUI.Box(new Rect(0, 0, 250, 400), "Network Options");
        GUILayout.BeginArea(new Rect(5, 30, simValues.buttonWidth, simValues.buttonHeight * simValues.numberButtons));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Node Comm Range", GUILayout.Width(simValues.menuLabelWidth));
        nodeCommRangeString = GUILayout.TextField(nodeCommRangeString, 4);
        GUILayout.EndHorizontal();
    
        GUILayout.EndArea();
        GUI.EndGroup();
    
   }

   public void setOptions()
   {
       simValues = gameObject.GetComponent<LoadOptionsGUI>();
       nodeToFindString = "0";
       timeToFind = 0;
       numHops = 0;
       nextHop = "";
       sourceStr = "";

       drawLine = true;
       updateLines = true;
       adaptiveNetworkColor = true;

   }
}