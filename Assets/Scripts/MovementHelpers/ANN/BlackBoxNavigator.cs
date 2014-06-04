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
    
    public BlackBoxNavigator(IBlackBox brain, int numOutputs)
    {
        this.brain = brain;
		outpus = new ArrayList(new float[numOutputs]);
    }

    public void setInputSignalArray(ISignalArray inputs, ArrayList sensorInfo)
    {
		for (int i = 0; i < sensorInfo.Count; i++)
			inputs[i] = (float)sensorInfo[i];
    }

	public void setOutputs(ISignalArray outputSigArray) 
	{
		for (int i = 0; i < outputs.Count; i++)
			outputs[i] = (float)outputSigArray[i];
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
		setOutputs ();
        return outputs;
    }

    public void checkpointNotify(double checkpointReward)
    {
        fitness += checkpointReward / (age/2 + 1);
    }

	public virtual void goalReachedNotify() 
	{
		fitness *= 2;
	}
}
