using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Network : MonoBehaviour, INetworkBehavior {
    protected LoadOptionsGUI simValues;
    protected NetworkGUI netValues;
    protected List<GameObject> neighbors;
	// Use this for initialization


//--------------------Unity Functions------------------------
	void Start () {


	}
	
	// Update is called once per frame-----------------------
	void Update () {
	
	}

//---------------------INetworkBehavior Implemenations
    public void addNeighbor(GameObject node)
    {
        if (!neighbors.Contains(node))
        {
            neighbors.Add(node);
        }
    }
    //public function to be called by nodeController if we need to remove a connection
    public void removeNeighbor(GameObject nodeID)
    {
        if (neighbors.Contains(nodeID))
        {
            neighbors.Remove(nodeID);
        }
    }

    public virtual void recMessage(MSGPacket packet) { }

    public virtual void sendMessage(MSGPacket packet) { }

    public void setValues(){
        simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
        netValues = simValues.networkGUI;
        neighbors = new List<GameObject>();
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
