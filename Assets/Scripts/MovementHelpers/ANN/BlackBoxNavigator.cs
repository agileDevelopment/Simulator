using System;
using System.Collections;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using UnityEngine;

public class BlackBoxNavigator
{
    IBlackBox brain;
    ArrayList outputs = new ArrayList(new float[] {0.0f,0.0f,0.0f});
    public int age = 0;
    public double fitness = 0;
    public bool isAlive = true;
    
    public BlackBoxNavigator(IBlackBox brain)
    {
        this.brain = brain;
    }

    public void setInputSignalArray(ISignalArray inputs, ArrayList sensorInfo)
    {
        //Debug.Log("Inputs: " + sensorInfo[0] + ", " + sensorInfo[1] + ", " + sensorInfo[2] + ", " + sensorInfo[3] + ", " + sensorInfo[4] + ", " + sensorInfo[5] + ", " + sensorInfo[6]);
        inputs[0] = (float)sensorInfo[0];
        inputs[1] = (float)sensorInfo[1];
        inputs[2] = (float)sensorInfo[2];
        inputs[3] = (float)sensorInfo[3];
        inputs[4] = (float)sensorInfo[4];
        inputs[5] = (float)sensorInfo[5];
        inputs[6] = (float)sensorInfo[6];
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
        //Debug.Log("Outputs: " + outputs[0] + ", " + outputs[1] + ", " + outputs[2]);
        outputs[0] = (float)brain.OutputSignalArray[0]; 
        outputs[1] = (float)brain.OutputSignalArray[1]; 
        outputs[2] = (float)brain.OutputSignalArray[2];
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
