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
using SharpNeat.Genomes.HyperNeat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;
using UnityEngine;

public class ESHyperNEATNavigationExperiment : SimpleNeatExperiment
{
    IPhenomeEvaluator<IBlackBox> _phenomeEvaluator;
    NetworkActivationScheme _activationSchemeCppn;
    int RealInputCount = 5 + 8; // 5 sensors, 8 quadrant sensors, current velocity
    int RealOutputCount = 1 + 8; // 1 accelration and 8 "quadrant votes" outputs

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
	/// Create a genome2 factory for the experiment.
	/// Create a genome2 factory with our neat genome2 parameters object and the appropriate number of input and output neuron genes.
	/// </summary>
	public override IGenomeFactory<NeatGenome> CreateGenomeFactory()
	{
		return new CppnGenomeFactory(InputCount, OutputCount);
	}

    /// <summary>
    /// Creates a new ESHyperNEAT genome decoder that can be used to convert a genome into a phenome.
    /// </summary>
    public override IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
    {
        SubstrateNodeSet inputLayer = new SubstrateNodeSet(RealInputCount);
        SubstrateNodeSet outputLayer = new SubstrateNodeSet(RealOutputCount);

        // Each node in each layer needs a unique ID.
        // The input nodes use ID range [1,9] and
        // the output nodes use [10,18].
        uint inputId = 1;
        uint outputId = (uint)(inputId + RealInputCount);

        // I will represent the inputs on a substrate as such:
		//
		// The forward, right, left, down, up sensors will exist on vertical and horizontal slices of a cube
		// in positions relative to the center based on the direction they should impulse. The quadrant sensors
		// will exist at the various 3d diagonals from the center.
		//
		// The velocity output will be forward of center at 0.5 (for movement forward). The theta/phi angle outputs
		// will be at the center.
		//
        // Hopefully the structure of this will be good...
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 0, 0, 0.5 })); // Forward Sensor input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { -0.5, 0, 0 })); // Right Sensor input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 0.5, 0, 0 })); // Left Sensor input
        inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 0, 0.5, 0 })); // Up Sensor input
		inputLayer.NodeList.Add(new SubstrateNode(inputId++, new double[] { 0, -0.5, 0 })); // Down Sensor input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5, 0.5, 0.5 })); // Top, front, left quadrant input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5, 0.5, 0.5 })); // Top, front, right quadrant input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5,-0.5, 0.5 })); // Bottom, front, left quadrant input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5,-0.5, 0.5 })); // Bottom, front, right quadrant input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5, 0.5,-0.5 })); // Top, back, left quadrant input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5, 0.5,-0.5 })); // Top, back, right quadrant input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5,-0.5,-0.5 })); // Bottom, back, left quadrant input
		inputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5,-0.5,-0.5 })); // Bottom, back, right quadrant input

		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0, 0, 0.5 })); // Velocity output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5, 0.5, 0.5 })); // Top, front, left quadrant vote output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5, 0.5, 0.5 })); // Top, front, right quadrant vote output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5,-0.5, 0.5 })); // Bottom, front, left quadrant vote output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5,-0.5, 0.5 })); // Bottom, front, right quadrant vote output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5, 0.5,-0.5 })); // Top, back, left quadrant vote output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5, 0.5,-0.5 })); // Top, back, right quadrant vote output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] { 0.5,-0.5,-0.5 })); // Bottom, back, left quadrant vote output
		outputLayer.NodeList.Add(new SubstrateNode(outputId++, new double[] {-0.5,-0.5,-0.5 })); // Bottom, back, right quadrant vote output

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
        float bandingThreshold = 0.5f;
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
        get { return 6; } // 2 * (x,y,z) = 6 inputs for CPPN
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
