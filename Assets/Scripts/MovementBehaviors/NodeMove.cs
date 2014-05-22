//------------------------------------------------------------
//  Title: NodeMove
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None

//  Description:  NodeMove is a Parent Class for UAV flightBehavior.  All children
//  must implement updateLocation() so the nodeController can call it durning update to get new positions.
//
//  Global Variables: LoadOptionsGui simValues  - shortcut for getting simulator setting values
//--------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class NodeMove : MonoBehaviour, IFlightBehavior {

	protected LoadOptionsGUI simValues;
	
	void Start () {
	//	simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
    }

	public abstract void updateLocation();
	
	public void setOptions(LoadOptionsGUI a){
		simValues = a;
	}
	

	
}






