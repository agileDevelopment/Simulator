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
    public ANNPopulationManager populationManager;
	public ArrayList inputs = new ArrayList(new float[] {0,0,0,0,0,0,0,0,0,0,0,0,0});
    public ArrayList newDirection;
    Vector3 goal;
    Vector3 goalDirection;
	Vector3 prevPosition;
    float yaw = 0, pitch = 0, speed = 0;
    int maxSpeed = 0, maxAcceleration = 0, numCheckpoints = 0;
    public Transform tmpTransform;
	bool isAlive = true;
    ANNNavGUI guiValues;

    Sensor[] sensors = new Sensor[5];
    int[] checkpointsReached;

    // Use this for initialization
    void Start()
    {
        guiValues = GameObject.Find("Spawner").GetComponent<ANNNavGUI>();
        populationManager = guiValues.popManager;
        gameObject.transform.position = guiValues.getSpawnLocation();
        goal = guiValues.getGoalLocation();

        maxSpeed = guiValues.nodeMaxSpeed;
        maxAcceleration = maxSpeed / 15;

		numCheckpoints = guiValues.numCheckpoints;
        checkpointsReached = new int[numCheckpoints];
        for (int i = 0; i < numCheckpoints; i++) checkpointsReached[i] = 0;

        tmpTransform = new GameObject().transform;
        tmpTransform.parent = gameObject.transform;

        for (int i = 0; i < sensors.Length; i++)
        {
            sensors[i] = new Sensor(gameObject, i, 64);
        }

		prevPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
    }

    public bool checkBounds()
    {
        if (0 <= transform.position.x && transform.position.x <= guiValues.floorSize &&
            0 <= transform.position.y && transform.position.y <= guiValues.floorSize &&
            0 <= transform.position.z && transform.position.z <= guiValues.floorSize)
        {
            return true;
        }
        return false;
    }

    public override void updateLocation()
    {
		try
		{
			Vector3 tmp = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
			tmpTransform.position = tmp;
			tmpTransform.LookAt (goal);
			float in_pitch = (0.0f - tmpTransform.eulerAngles.x); // Make it be between 0 and 1
			float in_yaw = (0.0f - tmpTransform.eulerAngles.y);

			bool stimulate_top = (0 < in_pitch);
			bool stimulate_front = (-90 < in_yaw && in_yaw <= 90);
			bool stimulate_right = (in_yaw <= 0);
			
			for (int i = 0; i < 13; i++) inputs[i] = 0; // Reset input array list

			for (int i = 0; i < sensors.Length; i++) inputs[i] = sensors[i].getSensorData();
			// The following designed to match what's in ESHyperNEATNavigationExperiment
			if (stimulate_top && stimulate_right && !stimulate_right) inputs[5] = 1;
			if (stimulate_top && stimulate_right && stimulate_right) inputs[6] = 1;
			if (!stimulate_top && stimulate_right && !stimulate_right) inputs[7] = 1;
			if (!stimulate_top && stimulate_right && stimulate_right) inputs[8] = 1;
			if (stimulate_top && !stimulate_right && !stimulate_right) inputs[9] = 1;
			if (stimulate_top && !stimulate_right && stimulate_right) inputs[10] = 1;
			if (!stimulate_top && !stimulate_right && !stimulate_right) inputs[11] = 1;
			if (!stimulate_top && !stimulate_right && stimulate_right) inputs[12] = 1;

            //print("Forward Sensor: " + inputs[2]);

            newDirection = populationManager.updateLocation(gameObject, inputs, isAlive); // Must do this to update age

			if (isAlive) {
                if (checkBounds() && prevPosition != transform.position) // Only get fitness points in the maze
				{
                    populationManager.checkpointNotify(gameObject, guiValues.floorSize/2/(Vector3.Distance(gameObject.transform.position, goal) + 1) );
					prevPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
				}

				float biggestVote = 0;
				int biggestVoteIndex = 0;
				for (int i = 1; i < newDirection.Count; i++)
				{
					if (newDirection[i] > biggestVote) 
					{
						biggestVoteIndex = i;
						biggestVote = newDirection[i];
					}
				}

				float yaw_change = 0;
				float pitch_change = 0;
				switch (biggestVoteIndex)
				{
				case 1: // top, front, left vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				case 2: // top, front, right vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				case 3: // bottom, front, left vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				case 4: // bottom, front, right vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				case 5: // top, back, left vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				case 6: // top, back, right vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				case 7: // bottom, back, left vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				default: // bottom, back, right vote
					yaw_change = newDirection[2] - newDirection[1]; // Turn it negative by subtracting from right hand vote. Also adjusts magnitude accordingly.
					pitch_change = newDirection[1] - newDirection[3]; // Subtract the downward vote.
					break;
				}

				yaw = (yaw + yaw_change * 30) % 360; // Max yaw change rate of 15 degrees
				pitch = (pitch + pitch_change * 30) % 360; // Max descent change rate of 5 degrees

				speed += ((float)newDirection [2] - 0.5f) * 2 * maxAcceleration;
				if (speed >= maxSpeed)
					speed = (float)maxSpeed;
				if (speed <= -maxSpeed)
					speed = (float)-maxSpeed;

				transform.eulerAngles = new Vector3 (yaw, pitch, transform.eulerAngles.z);
				transform.position += transform.forward * speed * Time.deltaTime;
			}

            if (transform.position.y < -10)
                transform.position = new Vector3(transform.position.x, -10, transform.position.z);
        }
        catch { } // If the object was already destroyed we don't really care...
    }

    public override void checkpointNotify(int index)
    {
		if (index == 0) 
		{
            if (checkpointsReached[index] == 0)
            {
                checkpointsReached[index] = 1;
                print("Goal reached by " + gameObject.name);
                populationManager.goalReachedNotify(gameObject);
            }
		} 
		else 
		{
            if (checkpointsReached[index] == 0) // Only get the reward for hitting it once.
            {
                checkpointsReached[index] = 1;
                populationManager.checkpointNotify(gameObject, Mathf.Pow((float)3 * numCheckpoints / index, 2));
            }
		}
    }

	public override void hitObstacle()
	{
		isAlive = false;
	}
}