using System;
using System.Collections;
using UnityEngine;

public class NEATManager : IMovementManager
{
    public int maxSpeed;

    public NEATManager(int populationSize, int maxSpeed)
    {
        this.maxSpeed = maxSpeed;
    }

    /*
     * This particular getDestination expects the following inputs:
     * inputs[0] = Vector3 of current position
     * inputs[1] = Vector3 of goal position
     * inputs[2] = Sensors in the future
     */
    public Vector3 getDestination(ArrayList inputs)
    {
        return (Vector3)inputs[1];
    }
}
