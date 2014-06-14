using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using UnityEngine;

class MLPBPBlackBox : IBlackBox
{
	int _inputCount;
	int _outputCount;
	ISignalArray _inputArray, _outputArray;
	double[] signalsArray;
    int _hiddenNodes = 20;
    public int _id;
    public double fitness = 0;

    System.Random _rng;

    public BackPropNeuralNet bnn;

    public MLPBPBlackBox(int inputCount, int outputCount, int id, System.Random rng)
	{
		_inputCount = inputCount;
		_outputCount = outputCount;
        _id = id;
        _rng = rng;

		signalsArray = new double[InputCount + OutputCount];
		_inputArray = new SignalArray (signalsArray, 0, InputCount);
		_outputArray = new SignalArray (signalsArray, InputCount, OutputCount);

        bnn = new BackPropNeuralNet(InputCount, _hiddenNodes, OutputCount, _rng);
        bnn.GenRandWeights();
    }

    public void trainWithError(double error, double learnRate, double momentum)
    {

        //double[] weights = bnn.GetWeights();
        //string weightsStr = "";
        //foreach (double weight in weights)
        //{
        //    weightsStr += weight + ",";
        //}
        //Debug.Log(_id + " Weights: " + weightsStr);

        bnn.UpdateWeights(error, learnRate, momentum);
    }

	#region IBlackBox implementation

	public void Activate ()
	{
        double[] tmp = new double[InputCount];
        InputSignalArray.CopyTo(tmp, 0);
        double[] outputs = bnn.ComputeOutputs(tmp);
        OutputSignalArray.CopyFrom(outputs, 0);
	}

	public void ResetState ()
	{
		_inputArray.Reset();
		_outputArray.Reset();
	}

	public int InputCount {
		get {
			return _inputCount;
		}
	}

	public int OutputCount {
		get {
			return _outputCount;
		}
	}

	public ISignalArray InputSignalArray {
		get {
			return _inputArray;
		}
	}

	public ISignalArray OutputSignalArray {
		get {
			return _outputArray;
		}
	}

	public bool IsStateValid {
		get {
			return true;
		}
	}

	#endregion
}
