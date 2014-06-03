using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;

class MLPBPBlackBox : IBlackBox
{
	int _inputCount;
	int _outputCount;
	double _learnRate, _momentum;
	ISignalArray _inputArray, _outputArray;
	double[] signalsArray;

    public BackPropNeuralNet bnn;

    public MLPBPBlackBox(int inputCount, int outputCount)
	{
		_inputCount = inputCount;
		_outputCount = outputCount;

		signalsArray = new double[InputCount + OutputCount];
		_inputArray = new SignalArray (signalsArray, 0, InputCount);
		_outputArray = new SignalArray (signalsArray, InputCount, OutputCount);

		ResetState ();

        bnn = new BackPropNeuralNet(InputCount, 10, OutputCount);
        bnn.GenRandWeights();
    }

	#region IBlackBox implementation

	public void Activate ()
	{
		double[] tmp = new double[InputCount];
		InputSignalArray.CopyTo (tmp, 0);
		OutputSignalArray.CopyFrom(bnn.ComputeOutputs (tmp), InputCount);
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
