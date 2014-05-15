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
	LoadOptionsGUI simValues;
	NodeLine lineController;
	public int idNum;
	public string idString;

	
	
	// Use this for initialization
	void Awake(){
		simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
	//	flightBehavior = gameObject.AddComponent<Orbit>();
	}
	void Start () {
		lineController = gameObject.GetComponent<NodeLine>();
		//change this to implement a different movement controller
	
	}
	

	//----------------Unity Functions------------------------------
	void OnTriggerEnter (Collider col)
	{	
		
		if(col.gameObject.tag == "Node")
		{
			GameObject otherNode = col.gameObject;
			lineController.addLine(otherNode);
		}
		
	}
	
	void OnTriggerExit(Collider col){
		if(col.gameObject.tag == "Node")
		{
			GameObject otherNode = col.gameObject;
			lineController.removeLine(otherNode);
		}
	}
	
	
	// Update is called once per frame
	void Update () {
		if(gameObject.GetComponent<SphereCollider>().radius < simValues.nodeCommRange/10)
			gameObject.GetComponent<SphereCollider>().radius += .1f;
	
		}
		
	void LateUpdate(){
		if(!simValues.paused && simValues.enableUpdate){
			updateLocation();
		}
	}
	//-------------------Custome Functions---------------------------------	
	void updateLocation(){
		flightBehavior.updateLocation();
	}
	

}
