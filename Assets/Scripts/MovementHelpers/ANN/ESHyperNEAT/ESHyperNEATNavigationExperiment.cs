using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using SharpNeat.Domains;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Decoders.HyperNeat;
using SharpNeat.Decoders.ESHyperNeat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;
using UnityEngine;

public class ESHyperNEATNavigationExperiment : SimpleNeatExperiment
{
    IPhenomeEvaluator<IBlackBox> _phenomeEvaluator;
    NetworkActivationScheme _activationSchemeCppn;
    int RealInputCount = 7;
    //int FinalOutputCount = 3;

    public ESHyperNEATNavigationExperiment(IPhenomeEvaluator<IBlackBox> phenomeEvaluator)
    {
        _phenomeEvaluator = phenomeEvaluator;
    }

    public new void Initialize(string name, XmlElement xmlConfig)
    {
        base.Initialize(name, xmlConfig);
        _activationSchemeCppn = ExperimentUtils.CreateActivationScheme(xmlConfig, "ActivationCppn");
    }

    /// <summary>
    /// Creates a new ESHyperNEAT genome decoder that can be used to convert a genome into a phenome.
    /// </summary>
    public override IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
    {
        SubstrateNodeSet inputLayer = new SubstrateNodeSet(InputCount);
        SubstrateNodeSet outputLayer = new SubstrateNodeSet(OutputCount);

        // Each node in each layer needs a unique ID.
        // The input nodes use ID range [1,9] and
        // the output nodes use [10,18].
        uint inputId = 1;
        uint outputId = (uint)(inputId + RealInputCount);

        // I will represent the inputs on a substrate as depicted on the following picture
        //
        // Theta    |   Up      |   Phi
        // Left     |   Center  |   Right
        //          |   Down    |
        //
        // I will read outputs from the output substrate like so
        //
        // Theta    |           |   Phi
        //          |   Speed   |   
        //          |           |
        //
        // Hopefully the structure of this will be good...
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { -1, 1 })); // Theta input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 1, 1 }));  // Phi input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 0, 0 }));  // Forward Sensor input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 1, 0 }));  // Right Sensor input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { -1, 0 })); // Left Sensor input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 0, 1 }));  // Up Sensor input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 0, -1 })); // Down Sensor input

        outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { -1, 1 })); // Theta output
        outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 1, 1 }));  // Phi output
        outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0, 0 }));  // Velocity output

        List<SubstrateNodeSet> nodeSetList = new List<SubstrateNodeSet>(2);
        nodeSetList.Add(inputLayer);
        nodeSetList.Add(outputLayer);

        // Define a connection mapping from the input layer to the output layer.
        List<NodeSetMapping> nodeSetMappingList = new List<NodeSetMapping>(1);
        nodeSetMappingList.Add(NodeSetMapping.Create(0, 1, (double?)null));

        int initialDepth = 3;
        int maxDepth = 3;
        int weightRange = 5;
        float divisionThreshold = 0.03f;
        float varianceThreshold = 0.03f;
        float bandingThreshold = 0.3f;
        int ESIterations = 1;

        EvolvableSubstrate substrate = new EvolvableSubstrate(nodeSetList, DefaultActivationFunctionLibrary.CreateLibraryCppn(),
            0, 0.4, 5, nodeSetMappingList, initialDepth, maxDepth, weightRange, divisionThreshold, varianceThreshold, bandingThreshold, ESIterations);

        // Create genome decoder. Decodes to a neural network packaged with
        // an activation scheme that defines a fixed number of activations per evaluation.
        IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder =
            new ESHyperNeatDecoder(substrate, _activationSchemeCppn, _activationScheme, false);

        return genomeDecoder;
    }

    public override IPhenomeEvaluator<IBlackBox> PhenomeEvaluator
    {
        get { return _phenomeEvaluator; }
    }

    public override int InputCount
    {
        get { return 4; } // 2 * (x,y) = 4 inputs for CPPN
    }

    public override int OutputCount
    {
        get { return 2; } // 2 Outputs for the CPPN - 1 for weight and 1 for bias weight
    }

    public override bool EvaluateParents
    {
        get { return true; }
    }
}
