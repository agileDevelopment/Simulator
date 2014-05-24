//------------------------------------------------------------
//  Title: Orbit
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: OrbitGUI

//  Description: Defines motion of the nodes.


//  Extends NodeMove (which Implements IFlightBehavior)
//
//--------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class Orbit :  NodeMove{
	OrbitGUI orbitValues;
	public Vector3 center;
	Vector3 axis = Vector3.up;
	Vector3 desiredPosition;
	public float radius;
	public float radiusSpeed;
	public float rotationSpeed;
	
	// Use this for initialization
	void Start () {
		simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
		orbitValues = GameObject.Find("Spawner").GetComponent<OrbitGUI>();
		radius = (float)orbitValues.radius*(gameObject.GetComponent<NodeController>().idNum % 
		simValues.nodesSqrt);
		radius += gameObject.GetComponent<NodeController>().idNum;
		radiusSpeed = Random.Range(1,orbitValues.nodeMaxSpeed);
		rotationSpeed = Random.Range(5,orbitValues.nodeMaxSpeed);
		center = new Vector3(orbitValues.center,10, orbitValues.center);
		transform.position = (transform.position - center).normalized * radius + center;	
		}
		
	public override void updateLocation(){
	//	if(center !=null){
		gameObject.transform.RotateAround (center, axis, rotationSpeed * Time.deltaTime);
		desiredPosition = (transform.position - center).normalized * radius + center;
		gameObject.transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * radiusSpeed);	
//	}
	}
	

}