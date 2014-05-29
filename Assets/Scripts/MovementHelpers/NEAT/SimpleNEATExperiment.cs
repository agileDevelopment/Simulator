using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Decoders;
using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Decoders.Neat;
using SharpNeat.Phenomes;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;

public abstract class SimpleNeatExperiment : INeatExperiment
{
    NeatEvolutionAlgorithmParameters _eaParams;
    NeatGenomeParameters _neatGenomeParams;
    string _name;
    int _populationSize;
    int _specieCount;
    NetworkActivationScheme _activationScheme;
    string _complexityRegulationStr;
    int? _complexityThreshold;
    string _description;
    public int _lifespan;

    #region Abstract properties that subclasses must implement
    public abstract IPhenomeEvaluator<IBlackBox> PhenomeEvaluator { get; }
    public abstract int InputCount { get; }
    public abstract int OutputCount { get; }
    public abstract bool EvaluateParents { get; }
    #endregion

    #region INeatExperiment Members
    public string Description
    {
        get { return _description; }
    }

    public string Name
    {
        get { return _name; }
    }

    /// <summary>
    /// Gets the default population size to use for the experiment.
    /// </summary>
    public int DefaultPopulationSize
    {
        get { return _populationSize; }
    }

    /// <summary>
    /// Gets the NeatEvolutionAlgorithmParameters to be used for the experiment. Parameters on this object can be 
    /// modified. Calls to CreateEvolutionAlgorithm() make a copy of and use this object in whatever state it is in 
    /// at the time of the call.
    /// </summary>
    public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters
    {
        get { return _eaParams; }
    }

    /// <summary>
    /// Gets the NeatGenomeParameters to be used for the experiment. Parameters on this object can be modified. Calls
    /// to CreateEvolutionAlgorithm() make a copy of and use this object in whatever state it is in at the time of the call.
    /// </summary>
    public NeatGenomeParameters NeatGenomeParameters
    {
        get { return _neatGenomeParams; }
    }

    /// <summary>
    /// Initialize the experiment with some optional XML configutation data.
    /// </summary>
    public void Initialize(string name, XmlElement xmlConfig)
    {
        _name = name;
        _populationSize = XmlUtils.GetValueAsInt(xmlConfig, "PopulationSize");
        _specieCount = XmlUtils.GetValueAsInt(xmlConfig, "SpecieCount");
        _activationScheme = ExperimentUtils.CreateActivationScheme(xmlConfig, "Activation");
        _complexityRegulationStr = XmlUtils.TryGetValueAsString(xmlConfig, "ComplexityRegulationStrategy");
        _complexityThreshold = XmlUtils.TryGetValueAsInt(xmlConfig, "ComplexityThreshold");
        _description = XmlUtils.TryGetValueAsString(xmlConfig, "Description");
        _lifespan = XmlUtils.GetValueAsInt(xmlConfig, "Lifespan");

        _eaParams = new NeatEvolutionAlgorithmParameters();
        _eaParams.SpecieCount = _specieCount;
        _neatGenomeParams = new NeatGenomeParameters();
    }

    /// <summary>
    /// Create a genome2 factory for the experiment.
    /// Create a genome2 factory with our neat genome2 parameters object and the appropriate number of input and output neuron genes.
    /// </summary>
    public IGenomeFactory<NeatGenome> CreateGenomeFactory()
    {
        return new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
    }

    /// <summary>
    /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
    /// of the algorithm are also constructed and connected up.
    /// This overload requires no parameters and uses the default population size.
    /// </summary>
    public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(Func lambda)
    {
        return CreateEvolutionAlgorithm(DefaultPopulationSize, lambda);
    }

    /// <summary>
    /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
    /// of the algorithm are also constructed and connected up.
    /// This overload accepts a population size parameter that specifies how many genomes to create in an initial randomly
    /// generated population.
    /// </summary>
    public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(int populationSize, Func lambda)
    {
        // Create a genome2 factory with our neat genome2 parameters object and the appropriate number of input and output neuron genes.
        IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();

        // Create an initial population of randomly generated genomes.
        List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

        // Create evolution algorithm.
        return CreateEvolutionAlgorithm(genomeFactory, genomeList, lambda);
    }

    /// <summary>
    /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
    /// of the algorithm are also constructed and connected up.
    /// This overload accepts a pre-built genome2 population and their associated/parent genome2 factory.
    /// </summary>
    public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList, Func lambda)
    {
        // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weigth difference.
        IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
        ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);

        // Create complexity regulation strategy.
        IComplexityRegulationStrategy complexityRegulationStrategy = ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);

        // Create the evolution algorithm.
        NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);

        // Create genome2 decoder.
        IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(_activationScheme);

        // Create a genome2 list evaluator. This packages up the genome2 decoder with the genome2 evaluator.
        IGenomeListEvaluator<NeatGenome> genomeListEvaluator = new SerialGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, PhenomeEvaluator);

        // Wrap the list evaluator in a 'selective' evaulator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
        // that were in the population in previous generations (elite genomes). This is determiend by examining each genome2's evaluation info object.
        if (!EvaluateParents)
            genomeListEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(genomeListEvaluator,
                                     SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());

        // Initialize the evolution algorithm.
        ea.Initialize(genomeListEvaluator, genomeFactory, genomeList, lambda);

        // Finished. Return the evolution algorithm
        return ea;
    }

    /// <summary>
    /// Creates a new genome decoder that can be used to convert a genome into a phenome.
    /// </summary>
    public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
    {
        return new NeatGenomeDecoder(_activationScheme);
    }

    #endregion


    public List<NeatGenome> LoadPopulation(XmlReader xr)
    {
        throw new NotImplementedException();
    }

    public void SavePopulation(XmlWriter xw, IList<NeatGenome> genomeList)
    {
        throw new NotImplementedException();
    }
}
