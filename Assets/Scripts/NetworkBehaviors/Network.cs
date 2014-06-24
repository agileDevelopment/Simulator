using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Network : MonoBehaviour, INetworkBehavior {
    public LoadOptionsGUI simValues;
    public NetworkGUI netValues;
    public List<GameObject> neighbors;
    public Object nodeLock = new Object();
    public NodeLine lineController;

    protected int clearMsgCtr = 0;
 //   int routeCount=0;
    protected Dictionary<GameObject, RouteEntry> routes;
    protected Dictionary<string, MSGPacket> messages;
    public int broadcastID;
    public int countOfRoutes;
    public int countOfMessages;

	// Use this for initialization


//--------------------Unity Functions------------------------
	protected virtual void Start () {
        simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
        netValues = simValues.networkGUI;
        neighbors = new List<GameObject>();
        lineController = gameObject.GetComponent<NodeLine>();
        gameObject.GetComponent<SphereCollider>().radius = netValues.nodeCommRange / 200;
        routes = new Dictionary<GameObject, RouteEntry>();
        messages = new Dictionary<string, MSGPacket>();
        broadcastID = 0;


 	}
	
	// Update is called once per frame-----------------------
	protected virtual void Update () {
   //     routeCount = routes.Count;
        if (gameObject.GetComponent<NodeController>().selected)
        {
            netValues.myUIElements["nodeID"] = gameObject.name;
            netValues.myUIElements["numRoutes"] = "# of Rts:" + routes.Count.ToString();
        }
                if (simValues.networkChoice != "none")
        {
            if (gameObject.GetComponent<SphereCollider>().radius * gameObject.transform.localScale.x < simValues.networkGUI.nodeCommRange /2)
                gameObject.GetComponent<SphereCollider>().radius += .1f;
        }

	}

    //every 60 frames, clear messages.    
    protected virtual void LateUpdate()
    {
            clearMsgCtr = 0;
            Dictionary<string, MSGPacket> temp = new Dictionary<string,MSGPacket>(messages);
            foreach(MSGPacket packet in temp.Values){
                if (packet.startTime + netValues.active_route_timer < Time.time)
                {
                   messages.Remove(packet.id);
                }
            }
        

        countOfMessages = messages.Count;
        countOfRoutes = routes.Count;
    }

    void OnMouseDown()
    {
        netValues.source = gameObject;
        netValues.sourceStr = gameObject.name;
    }

    protected virtual void Fixed()
    {

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

    public virtual void sendBroadcast(MSGPacket packet)
    {

    }

    public void initBroadcast(string mType, string message)
    {
        broadcastID++;
        MSGPacket packetToSend = new MSGPacket();
        packetToSend.id = gameObject.name + " - " + broadcastID;
        packetToSend.sender = gameObject;
        packetToSend.destination = null;
        packetToSend.messageType = mType;
        packetToSend.message = message;
        packetToSend.retries = (int)simValues.numNodes / 2;
        packetToSend.MaxTTL = packetToSend.TTL = (int)simValues.numNodes;
        packetToSend.source = gameObject;
        packetToSend.startTime = Time.time;

        sendBroadcast(packetToSend);
    }


    public void initMessage(GameObject destination, string mType, string message)
    {
 
        MSGPacket packetToSend = new MSGPacket();
        packetToSend.id = gameObject.name + " - " + broadcastID;
        packetToSend.destination = destination;
        packetToSend.messageType = mType;
        packetToSend.message = message;
        packetToSend.retries = (int)simValues.numNodes / 2;
        packetToSend.MaxTTL = packetToSend.TTL = (int)simValues.numNodes;
        packetToSend.source = gameObject;
        packetToSend.startTime = Time.time;

        sendMessage(packetToSend);
    }

    public virtual void recMessage(MSGPacket packet)
    {
    //    print("recMessage");
        netValues.messageCounter++;
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

    protected virtual void updateRoutes(RouteEntry newRoute)
    {
        if (!routes.ContainsKey(newRoute.destination))
        {
            routes.Add(newRoute.destination, newRoute);
        }
        else
        {
            routes[newRoute.destination].nextHop = newRoute.nextHop;
        }
    }
};





public struct MSGPacket
{
    public string id;
    public GameObject source;
    public GameObject sender;
    public GameObject destination;
    public float startTime;
    public string messageType;
    public string message;
    public int retries;
    public int MaxTTL;
    public int TTL;
}

public class RouteEntry
{
    public GameObject destination;
    public GameObject nextHop;
}
