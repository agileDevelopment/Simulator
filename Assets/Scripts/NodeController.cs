//------------------------------------------------------------
//  Title: NodeController
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None
//
//  Description:  Main script for controlling node.  This script will call other scripts for different
//  functionality such as move and network.
//
//--------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class NodeController : MonoBehaviour {
	public NodeMove flightBehavior;
    public INetworkBehavior networkBehavior;
	LoadOptionsGUI simValues;
	NodeLine lineController;
	public int idNum;
	public string idString;
    public bool selected;
	
	// Use this for initialization
	void Awake(){
		simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
	//	flightBehavior = gameObject.AddComponent<Orbit>();
	}
	void Start () {
        selected = false;
		lineController = gameObject.GetComponent<NodeLine>();
		//change this to implement a different movement controller
	
	}

    void OnMouseDown()
    {
        unselectNodes();
        selected = true;
        gameObject.renderer.material.color = Color.green;

    }

    private void unselectNodes()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

        foreach (GameObject node in nodes)
        {
            if(node)
            node.GetComponent<NodeController>().selected = false;
            node.renderer.material.color = Color.blue;
        }

    }
	

	//----------------Unity Functions------------------------------
	void OnTriggerEnter (Collider col)
	{
        if (col.gameObject.tag == "Node") 
		{
			//GameObject otherNode = col.gameObject;
			////lineController.addLine(otherNode);
			//if (networkBehavior != null) networkBehavior.addNeighbor(otherNode);
		} else if (col.gameObject.tag == "Goal") 
		{
			//print ("Collided with " + col.gameObject.name);
			int index = int.Parse (col.gameObject.name.Substring (9));
			flightBehavior.checkpointNotify (index);
		} else if (col.gameObject.tag == "Obstacle") 
		{
			flightBehavior.hitObstacle();
        }
        else if (col.gameObject.tag == "Boundary")
        {
            flightBehavior.hitObstacle();
        }
	}
	
	void OnTriggerExit(Collider col){
		if(col.gameObject.tag == "Node")
		{
			//GameObject otherNode = col.gameObject;
            //lineController.removeLine(otherNode);
            //if (networkBehavior != null) networkBehavior.removeNeighbor(otherNode);
		}
		else if (col.gameObject.tag == "Goal")
		{
			//int index = int.Parse(col.gameObject.name.Substring(9));
			//flightBehavior.checkpointNotify(-index);
        }
	}
	
	
	// Update is called once per frame
	void Update () {
		//if(gameObject.GetComponent<SphereCollider>().radius < simValues.nodeCommRange/20)
		//	gameObject.GetComponent<SphereCollider>().radius += .1f;
        //if (selected)
        //    gameObject.renderer.material.color = Color.green;

        updateLocation();
	}
		
	void LateUpdate(){
		//if(!simValues.paused && simValues.enableUpdate){
		//}
	}
	//-------------------Custome Functions---------------------------------	
	void updateLocation(){
        //print("Object is: " + gameObject + ", Behavior is: " + flightBehavior);
		flightBehavior.updateLocation();
	}
	

}
