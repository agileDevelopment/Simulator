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
    int maxSpeed = 0, maxAcceleration = 0, numCheckpoints = 0;
    public Transform tmpTransform;
	bool isAlive = true;
    Sensor[] sensors = new Sensor[6];

    // Use this for initialization
    void Start()
    {
        ANNNavGUI guiValues = GameObject.Find("Spawner").GetComponent<ANNNavGUI>();
        populationManager = guiValues.popManager;
        gameObject.transform.position = guiValues.getSpawnLocation();
        goal = guiValues.getGoalLocation();

        maxSpeed = guiValues.nodeMaxSpeed;
        maxAcceleration = maxSpeed / 15;

		numCheckpoints = guiValues.numCheckpoints;

        tmpTransform = new GameObject().transform;
        tmpTransform.parent = gameObject.transform;

        for (int i = 0; i < sensors.Length; i++)
        {
            sensors[i] = new Sensor(gameObject, i, 64);
        }
    }

    public override void updateLocation()
    {
		try
		{
			Vector3 tmp = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
			tmpTransform.position = tmp;
			tmpTransform.LookAt (goal);
			float in_pitch = (180.0f - tmpTransform.eulerAngles.x) / 360.0f; // Make it be between 0 and 1
			float in_yaw = (180.0f - tmpTransform.eulerAngles.y) / 360.0f;
			
			inputs [0] = in_yaw;
			inputs [1] = in_pitch;
            for (int i = 0; i < sensors.Length; i++)
            {
                inputs[i + 2] = sensors[i].getSensorData();
            }

            //print("Forward Sensor: " + inputs[2]);

            newDirection = populationManager.updateLocation(gameObject, inputs, isAlive); // Must do this to update age

			if (isAlive) {
				yaw = (yaw + ((float)newDirection [0] - 0.5f) * 2 * 30); // Max yaw change rate of 15 degrees
				if (yaw < 0)
						yaw = 0;
				if (yaw > 360)
						yaw = 360;
				pitch = (pitch + ((float)newDirection [1] - 0.5f) * 2 * 30); // Max descent change rate of 5 degrees
				if (pitch < 0)
						pitch = 0;
				if (pitch > 360)
						pitch = 360;
				speed += ((float)newDirection [2] - 0.5f) * 2 * maxAcceleration;
				if (speed >= maxSpeed)
					speed = (float)maxSpeed;
				if (speed <= -maxSpeed)
					speed = (float)-maxSpeed;

				transform.eulerAngles = new Vector3 (yaw, pitch, transform.eulerAngles.z);
				transform.position += transform.forward * speed * Time.deltaTime;
			}
        }
        catch { } // If the object was already destroyed we don't really care...
    }

    public override void checkpointNotify(int index)
    {
		if (index == 0) 
		{
			print ("Goal reached by " + gameObject.name);
			populationManager.goalReachedNotify(gameObject);
		} 
		else 
		{
			populationManager.checkpointNotify(gameObject, Mathf.Pow((float)numCheckpoints / index, 2));
		}
    }

	public override void hitObstacle()
	{
		isAlive = false;
	}
}