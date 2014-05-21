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

public class Grid : NodeMove
{
    //other data
    GridGUI gridValues;
    Vector3 center;
    Vector3 axis = Vector3.up;
    Vector3 desiredPosition;
    float radius;
    float radiusSpeed;
    float rotationSpeed;

    // Use this for initialization
    void Start()
    {
        gridValues = GameObject.Find("Spawner").GetComponent<GridGUI>();
        radius = (float)gridValues.radius;
        radiusSpeed = Random.Range(5, gridValues.nodeMaxSpeed) + gameObject.GetComponent<NodeController>().idNum;
        rotationSpeed = Random.Range(5, gridValues.nodeMaxSpeed);
        float direction = Random.Range(0, 10);
        if (direction < 5)
        {
            rotationSpeed = 0 - rotationSpeed;
        }
        center = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        center.x = center.x - radius;
        transform.position = (transform.position).normalized * radius + center;
    }

    public override void updateLocation()
    {

        transform.RotateAround(center, axis, rotationSpeed * Time.deltaTime);
        desiredPosition = (transform.position - center).normalized * radius + center;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * radiusSpeed);
    }


}