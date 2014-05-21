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
    public IMovementManager movementManager;
    public RTPopulationManager populationManager;
    public ArrayList inputs;
    public int speed = 0;

    // Use this for initialization
    void Start()
    {
        ANNNavGUI guiValues = GameObject.Find("Spawner").GetComponent<ANNNavGUI>();
        populationManager = guiValues.popManager;
        movementManager = guiValues.movementManager;
        speed = Random.Range(1, guiValues.nodeMaxSpeed);

        inputs = new ArrayList();
        inputs.Add(transform.position);
        inputs.Add(new Vector3(guiValues.goalPointX, guiValues.goalPointY, guiValues.goalPointZ));
    }

    public override void updateLocation()
    {
        if (!populationManager.checkMember(gameObject))
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, movementManager.getDestination(inputs), Time.deltaTime * speed);
    }
}