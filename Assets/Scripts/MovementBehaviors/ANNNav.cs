//------------------------------------------------------------
//  Title: Grid
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Joshua Christman
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

public class ANNNav : NodeMove
{
    //other data
    public RTPopulationManager populationManager;
    public Vector3 desiredPosition;
    public int speed = 0;

    // Use this for initialization
    void Start()
    {
        ANNNavGUI guiValues = GameObject.Find("Spawner").GetComponent<ANNNavGUI>();
        populationManager = GameObject.Find("Spawner").GetComponent<RTPopulationManager>();
        speed = Random.Range(1, guiValues.nodeMaxSpeed);
        desiredPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    public override void updateLocation()
    {
        if (!populationManager.checkMember(gameObject))
        {
            Destroy(gameObject);
            return;
        }

        desiredPosition.x += speed;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
    }
}