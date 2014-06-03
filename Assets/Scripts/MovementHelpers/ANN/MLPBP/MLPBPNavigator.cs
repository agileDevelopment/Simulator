using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MLPBPNavigator
{
    double[] inputs = new double[] { 0,0,0,0,0,0,0,0 };
    double[] outputs = new double[] { 0,0,0 };

    BackPropNeuralNet bnn;

    public MLPBPNavigator(double learnRate, double momentum)
    {
        bnn = new BackPropNeuralNet(inputs.Length, 100, outputs.Length);
        bnn.GenRandWeights();
    }
}
