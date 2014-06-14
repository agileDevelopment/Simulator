using System;
using System.Collections;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using UnityEngine;

public class BlackBoxNavigator
{
    IBlackBox brain;
	ArrayList outputs;
    public int age = 0;
    public double fitness = 0;
    public bool isAlive = true;
    
    public BlackBoxNavigator(IBlackBox brain)
    {
        this.brain = brain;
		outputs = new ArrayList(new float[9]);
    }

    public void setInputSignalArray(ISignalArray inputs, ArrayList sensorInfo)
    {
        //string debug = "";
        for (int i = 0; i < sensorInfo.Count; i++)
        {
            inputs[i] = (float)sensorInfo[i];
            //debug += inputs[i] + ", ";
        }
        //Debug.Log("Inputs: " + debug);
    }

	public void setOutputs(ISignalArray outputSigArray) 
	{
        //string debug = "";
        for (int i = 0; i < outputs.Count; i++)
        {
            outputs[i] = (float)outputSigArray[i];
            //debug += outputs[i] + ", ";
        }
        //Debug.Log("Outputs: " + debug);
	}

    public ArrayList updateLocation(ArrayList sensorInfo, bool isAlive)
    {
        if (this.isAlive && !isAlive)
        {
            this.isAlive = isAlive;
            fitness *= 0.95;
        }
        age++;

        brain.ResetState();
        setInputSignalArray(brain.InputSignalArray, sensorInfo);
        brain.Activate();
		setOutputs (brain.OutputSignalArray);
        return outputs;
    }

    public void checkpointNotify(double checkpointReward)
    {
        double reward = checkpointReward / (age/2 + 1);
        if (!isAlive) reward *= 0.75;
        fitness += reward;
    }

	public virtual void goalReachedNotify() 
	{
		fitness *= 2;
	}
}
