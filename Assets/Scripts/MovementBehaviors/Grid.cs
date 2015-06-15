//------------------------------------------------------------
//  Title: Grid
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: GridGUI

//  Description: Defines motion of the nodes.


//  Extends NodeMove (which Implements IFlightBehavior)
//
//--------------------------------------------------------------


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : NodeMove
{
    //other data
    GridGUI gridValues;
    Vector3 center;
    Vector3 axis = Vector3.up;
    Vector3 axis2 = Vector3.back;
    Vector3 desiredPosition;
    float radius;
    float radiusSpeed;
    float rotationSpeed;
    List<Vector3> axisList;

    // Use this for initialization
    void Start()
    {
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

        gridValues = GameObject.Find("Spawner").GetComponent<GridGUI>();
        radius = (float)gridValues.radius;
        radiusSpeed = Random.Range(5, gridValues.nodeMaxSpeed)/5;
        rotationSpeed = Random.Range(5, gridValues.nodeMaxSpeed)/5;
        float direction = Random.Range(0, 10);
        if (direction < 5)
        {
            rotationSpeed = 0 - rotationSpeed;
        }
        center = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        axis = center;
        center.x = center.x - radius;
        transform.position = (transform.position).normalized * radius + center;
    }

    public override void updateLocation()
    {

        transform.RotateAround(center, axis, rotationSpeed * Time.deltaTime);
        transform.RotateAround(center, axis2, rotationSpeed*1.2f * Time.deltaTime);
        desiredPosition = (transform.position - center).normalized * radius + center;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * radiusSpeed);
    }


}