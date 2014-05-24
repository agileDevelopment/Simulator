using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Network : MonoBehaviour, INetworkBehavior {
    public LoadOptionsGUI simValues;
    public NetworkGUI netValues;
    public List<GameObject> neighbors;
    public Object nodeLock = new Object();
    public NodeLine lineController;
	// Use this for initialization


//--------------------Unity Functions------------------------
	void Start () {
 	}
	
	// Update is called once per frame-----------------------
	void Update () {
	
	}

//---------------------INetworkBehavior Implemenations
    public virtual void addNeighbor(GameObject node)
    {
        if (!neighbors.Contains(node))
        {
            neighbors.Add(node);
            lineController.addLine(node);
        }
    }
    //public function to be called by nodeController if we need to remove a connection
    public virtual void removeNeighbor(GameObject node)
    {
        if (neighbors.Contains(node))
        {
            neighbors.Remove(node);
            lineController.removeLine(node);
        }
    }

    public virtual void recMessage(MSGPacket packet) { }

    public virtual void sendMessage(MSGPacket packet) { }

    public void setValues(){
        simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
        netValues = simValues.networkGUI;
        neighbors = new List<GameObject>();
        lineController = gameObject.GetComponent<NodeLine>();
        gameObject.GetComponent<SphereCollider>().radius = netValues.nodeCommRange / 200;
    }
};

public struct MSGPacket
{
    public GameObject source;
    public GameObject sender;
    public GameObject destination;
    public float startTime;
    public string message;
    public int retries;
}
