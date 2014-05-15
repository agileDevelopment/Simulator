//------------------------------------------------------------
//  Title: Spawner
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None
//
//  Description:  Generated Nodes based on GUI input.
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
public GameObject nodePrefab;

float center;
float floorSize;
IFlightGUIOptions flightGUI;
IFlightBehavior flightBehavior;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void StartSimulation(int choice){
	switch(choice){
	case 0:
	flightGUI = gameObject.GetComponent<GridGUI>();
	break;
	case 1:
	flightGUI = gameObject.GetComponent<OrbitGUI>();
	break;
	
	default:
	break;
	}
		LoadOptionsGUI loadData = gameObject.GetComponent<LoadOptionsGUI>();
		flightGUI.setFloor();


		for(int i = 0; i < loadData.numNodes; i++){
				GameObject node = (GameObject)GameObject.Instantiate(nodePrefab);
				node.name =  "Node " + i.ToString();
				NodeController data = node.GetComponent<NodeController>();
				data.idNum = i;
				data.idString = "Node " + i.ToString();
			switch(choice){
			case 0:
				node.AddComponent<Grid>();
				node.GetComponent<NodeController>().flightBehavior = node.GetComponent<Grid>();
				break;
			case 1:
				node.AddComponent<Orbit>();	
				node.GetComponent<NodeController>().flightBehavior = node.GetComponent<Orbit>();
				break;
				
			default:
				break;
			}
			node.GetComponent<SphereCollider>().radius = loadData.nodeCommRange/100;
		}
		
		flightGUI.setSpawnLocation();
		gameObject.GetComponent<LoadOptionsGUI>().paused = false;


	}
}
