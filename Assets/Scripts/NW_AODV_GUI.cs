using UnityEngine;
using System.Collections;

public class NW_AODV_GUI : MonoBehaviour, INetworkGUIOptions {
    LoadOptionsGUI simValues;
    public GameObject nodeToFind;
    public GameObject source;
    public string nodeToFindString;
    public int nodeToFindID;
    public float timeToFind;
    public int numHops;
    public string nextHop;
    public string sourceStr;
	// Use this for initialization
	void Start () {
        simValues = gameObject.GetComponent<LoadOptionsGUI>();
        nodeToFindString = "0";
        timeToFind = 0;
        numHops = 0;
        nextHop = "";
        sourceStr = "";

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI(){
        if(!simValues.showMainGui)
        {
            GUI.Box(new Rect(0,Screen.height / 2, 175, 200), "AODV Options");
            GUILayout.BeginArea(new Rect(10, Screen.height / 2 +5, 175, 200));            
            GUILayout.Space(30);
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
            if (GUILayout.Button("Find",GUILayout.Width(140)))
            {
                GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

                foreach (GameObject node in nodes)
                {
                    if (node)
                        node.renderer.material.color = Color.blue;

                }
                nodeToFindID = int.Parse(nodeToFindString);
                simValues.foundTime = 0;
                simValues.startTime = Time.time;
                simValues.endTime = 0;

                if (nodeToFindID < simValues.numNodes)
                {
                    nodeToFind = GameObject.Find("Node " + nodeToFindID);
                    source.GetComponent<NW_AODV>().discoverPath(nodeToFind);
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
                simValues.foundTime = 0;
                simValues.startTime = Time.time;
                simValues.endTime = 0;

                if (nodeToFindID < simValues.numNodes)
                {
                    nodeToFind = GameObject.Find("Node " + nodeToFindID);
                    source.GetComponent<NW_AODV>().initMessage(nodeToFind);
                    nodeToFind.renderer.material.color = Color.magenta;

                }
            }


            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
}



    public void showGUI()
    {

    }

    public void setGuiValues()
    {

    }


}
