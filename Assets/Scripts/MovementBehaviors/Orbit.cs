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
using System.Collections.Generic;

public class Orbit :  NodeMove{
	OrbitGUI orbitValues;
	public Vector3 center;
	Vector3 axis;
    Vector3 axis2;
	Vector3 desiredPosition;
	public float radius;
	public float radiusSpeed;
	public float rotationSpeed;
    List<Vector3> axisList;
	
	// Use this for initialization
	void Start () {
        axisList = new List<Vector3>();
        axisList.Add(Vector3.up);
        axisList.Add(Vector3.right);
        axisList.Add(Vector3.back);
        axisList.Add(Vector3.down);
        axisList.Add(Vector3.left);
        axisList.Add(Vector3.back);
        int k = Random.Range(0, axisList.Count);
        axis = axisList[k];
        axisList.Remove(axis);

        k = Random.Range(0, axisList.Count);
        axis2 = axisList[k];
        axisList.Remove(axis2);

		simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
		orbitValues = GameObject.Find("Spawner").GetComponent<OrbitGUI>();
        radius = Random.Range(0, (float)orbitValues.radius);
		radiusSpeed = Random.Range(1,orbitValues.nodeMaxSpeed);
		rotationSpeed = Random.Range(5,orbitValues.nodeMaxSpeed);
		center = new Vector3(orbitValues.center,10, orbitValues.center);
		transform.position = (transform.position - center).normalized * radius + center;
   


		}
		
	public override void updateLocation(){
	//	if(center !=null){
	    gameObject.transform.RotateAround (center, axis, rotationSpeed * Time.deltaTime);
        gameObject.transform.RotateAround(center, axis2, rotationSpeed * Time.deltaTime);
		desiredPosition = (transform.position - center).normalized * radius + center;
		gameObject.transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * radiusSpeed);	
//	}
	}
	

}