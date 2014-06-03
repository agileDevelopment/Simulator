using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;

public class NEATNavigationExperiment : SimpleNeatExperiment
{
    IPhenomeEvaluator<IBlackBox> _phenomeEvaluator;

    public NEATNavigationExperiment(IPhenomeEvaluator<IBlackBox> phenomeEvaluator)
    {
        _phenomeEvaluator = phenomeEvaluator;
    }

    public override IPhenomeEvaluator<IBlackBox> PhenomeEvaluator
    {
        get { return _phenomeEvaluator; }
    }

    public override int InputCount
    {
        get { return 7; }
    }

    public override int OutputCount
    {
        get { return 3; }
    }

    public override bool EvaluateParents
    {
        get { return true; }
    }
}
