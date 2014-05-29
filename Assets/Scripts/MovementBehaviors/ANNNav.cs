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
    public NEATPopulationManager populationManager;
    public ArrayList inputs = new ArrayList(new float[] {0,0,0,0,0,0,0,0});
    public ArrayList newDirection;
    Vector3 goal;
    Vector3 goalDirection;
    float yaw = 0, pitch = 0, speed = 0;
    int maxSpeed = 0, maxAcceleration = 0;
    public Transform tmpTransform;

    // Use this for initialization
    void Start()
    {
        ANNNavGUI guiValues = GameObject.Find("Spawner").GetComponent<ANNNavGUI>();
        populationManager = guiValues.popManager;
        gameObject.transform.position = guiValues.getSpawnLocation();
        goal = guiValues.getGoalLocation();
        maxSpeed = guiValues.nodeMaxSpeed;
        maxAcceleration = maxSpeed / 5;
        tmpTransform = new GameObject().transform;
        tmpTransform.parent = gameObject.transform;
    }

    public override void updateLocation()
    {
        try
        {
            populationManager.checkpointNotify(gameObject, 500.0 / Mathf.Pow(Vector3.Distance(transform.position, goal), 2.0f));

            Vector3 tmp = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            tmpTransform.position = tmp;
            tmpTransform.LookAt(goal);
            float in_pitch = 0.0f - tmpTransform.eulerAngles.x;
            float in_yaw = 0.0f - tmpTransform.eulerAngles.y;

            inputs[0] = in_yaw;
            inputs[1] = in_pitch;
            inputs[2] = 0.0f; inputs[3] = 0.0f; inputs[4] = 0.0f; inputs[5] = 0.0f; inputs[6] = 0.0f; inputs[7] = 0.0f;

            newDirection = populationManager.updateLocation(gameObject, inputs);

            yaw = (yaw + ((float)newDirection[0] - 0.5f) * 5); // Max turn rate of 15 degrees
            if (yaw < 0) yaw = 0;
            if (yaw > 360) yaw = 360;
            pitch = (pitch + ((float)newDirection[0] - 0.5f) * 5); // Max turn rate of 15 degrees
            if (pitch < 0) pitch = 0;
            if (pitch > 360) pitch = 360;
            speed += (float)newDirection[2] * maxAcceleration;
            if (speed >= maxSpeed) speed = (float)maxSpeed;

            //print("Yaw, pitch, speed: " + yaw + ", " + pitch + ", " + speed);

            transform.eulerAngles = new Vector3(yaw, pitch, transform.eulerAngles.z);
            transform.position += transform.forward * speed * Time.deltaTime;
            //print("After: " + transform.position);
        }
        catch { } // If the object was already destroyed we don't really care...
    }

    public void checkpointNotify()
    {
        populationManager.checkpointNotify(gameObject, 500.0 / Mathf.Pow(Vector3.Distance(transform.position, goal), 2.0f));
    }
}