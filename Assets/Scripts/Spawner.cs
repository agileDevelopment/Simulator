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

INetworkGUIOptions networkGUI;
INetworkBehavior networkBehavior;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void StartSimulation(int flightChoice, int networkChoice ){
        switch (flightChoice)
        {
	        case 0:
	         flightGUI = gameObject.GetComponent<GridGUI>();
	          break;
	        case 1:
	          flightGUI = gameObject.GetComponent<OrbitGUI>();
	         break;
	        default:
	           break;
	}
        switch (networkChoice)
        {
            case 0:
                networkGUI = gameObject.GetComponent<NW_AODV_GUI>();
                break;
            case 1:
                break;
            default:
                break;
        }

		LoadOptionsGUI loadData = gameObject.GetComponent<LoadOptionsGUI>();
		flightGUI.setFloor();


		for(int i = 0; i < loadData.numNodes; i++){
				GameObject node = (GameObject)GameObject.Instantiate(nodePrefab);
				node.name =  "Node " + i.ToString();
                node.renderer.material.color = Color.blue;
				NodeController data = node.GetComponent<NodeController>();
				data.idNum = i;
				data.idString = "Node " + i.ToString();
                switch (flightChoice)
                {
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
                switch (networkChoice)
                {
                    case 0:
                        node.AddComponent<NW_AODV>();
                        node.GetComponent<NodeController>().networkBehavior = node.GetComponent<NW_AODV>();
                        break;
                    case 1:
                        break;
                    default:
                        break;
                }

			node.GetComponent<SphereCollider>().radius = loadData.nodeCommRange/200;
		}
		
		flightGUI.setSpawnLocation();
		gameObject.GetComponent<LoadOptionsGUI>().paused = false;


	}
}
