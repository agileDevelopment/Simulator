using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Network : MonoBehaviour, INetworkBehavior {
    public LoadOptionsGUI simValues;
    public NetworkGUI netValues;
    public List<GameObject> neighbors;
    public Object nodeLock = new Object();
    public NodeLine lineController;
    protected Hashtable routes;
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

    public int getNeighborCount(){
        return neighbors.Count;
    }

    public virtual void drawLines() { }

    protected virtual void performRecMessage(MSGPacket packet) { }

    public virtual void sendMessage(MSGPacket packet) {
    }

    public virtual void discoverPath(GameObject node)
    {

    }


    public void initMessage(GameObject destination)
    {
        MSGPacket packetToSend = new MSGPacket();
        packetToSend.destination = destination;
        packetToSend.message = " I am a test message";
        packetToSend.retries = (int)simValues.numNodes / 10;
        packetToSend.TTL = (int)simValues.numNodes / 2;
        packetToSend.source = gameObject;
        packetToSend.startTime = Time.time;
        //    print(gameObject.name + " Initiating MSG to " + destination.name);

        sendMessage(packetToSend);
    }

    public virtual void recMessage(MSGPacket packet)
    {
        if (netValues.useLatency)
            StartCoroutine(delayRecMessage(packet));
        else
            performRecMessage(packet);
    }

    IEnumerator delayRecMessage(MSGPacket packet)
    {

        float distance = Vector3.Distance(gameObject.transform.position, packet.sender.transform.position);
        distance = distance / 20000;
        yield return new WaitForSeconds(distance);
        performRecMessage(packet);
    }

    public void setValues(){
        simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
        netValues = simValues.networkGUI;
        neighbors = new List<GameObject>();
        lineController = gameObject.GetComponent<NodeLine>();
        gameObject.GetComponent<SphereCollider>().radius = netValues.nodeCommRange / 200;
        routes = new Hashtable();
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
    public int TTL;
}

public class RouteEntry
{
    public GameObject destination;
    public GameObject nextHop;
}
