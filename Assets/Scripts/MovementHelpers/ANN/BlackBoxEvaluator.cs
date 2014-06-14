using System;
using System.Collections;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.EvolutionAlgorithms;
using UnityEngine;

public class BlackBoxEvaluator : IPhenomeEvaluator<IBlackBox>
{
    ANNPopulationManager _popManager;
    GameObject maxFitnessNode;
    ulong _evalCount = 0;
    bool _stopConditionSatisfied = false;
    double maxFitness = 0;

    public BlackBoxEvaluator(ANNPopulationManager popManager)
    {
        _popManager = popManager;
    }

    #region IPhenomeEvaluator<IBlackBox> Members

    /// <summary>
    /// Gets the total number of evaluations that have been performed.
    /// </summary>
    public ulong EvaluationCount
    {
        get { return _evalCount; }
    }

    /// <summary>
    /// Gets a value indicating whether some goal fitness has been achieved and that
    /// the the evolutionary algorithm/search should stop. This property's value can remain false
    /// to allow the algorithm to run indefinitely.
    /// </summary>
    public bool StopConditionSatisfied
    {
        get { return _stopConditionSatisfied; }
    }

    /// <summary>
    /// Evaluate the provided IBlackBox against the XOR problem domain and return its fitness score.
    /// </summary>
    /// 
    public void Evaluate(int experimentId, IBlackBox box, SharpNeat.Core.Func<double> lambda)
    {
        Coroutiner.StartCoroutine(_Evaluate(experimentId, box, lambda));
    }

    IEnumerator _Evaluate(int experimentId, IBlackBox box, SharpNeat.Core.Func<double> lambda)
    {
        double fitness = 0;
        maxFitness = 0;

        Color nodeColor;
        switch (experimentId)
        {
            case 1:
                nodeColor = Color.cyan;
                //nodeColor.a = 0.5f;
                break;
            case 2:
                nodeColor = Color.white;
                //nodeColor.a = 0.5f;
                break;
            default:
                nodeColor = Color.blue;
                //nodeColor.a = 0.5f;
                break;
        }

        GameObject node = _popManager.buildMemberNode(nodeColor);
        maxFitnessNode = node;

        BlackBoxNavigator nav = new BlackBoxNavigator(box);
        _popManager.population.Add(node, nav);

        while (nav.age < _popManager._lifespan)
        {
            if (nav.fitness > maxFitness && node != maxFitnessNode)
            {
                maxFitness = nav.fitness;
                Color yellowColor = Color.yellow;
                yellowColor.a = 1f;
                node.renderer.material.color = yellowColor;
                maxFitnessNode.renderer.material.color = nodeColor;
               // Debug.Log("Current leader is " + node.name);
                maxFitnessNode = node;
            }
            yield return null;
        }

        fitness = Math.Max(nav.fitness, 0);
        _evalCount++;
        lambda(fitness);
        _popManager.removeMemberNode(node);
    }

    /// <summary>
    /// Reset the internal state of the evaluation scheme if any exists.
    /// Note. The XOR problem domain has no internal state. This method does nothing.
    /// </summary>
    public void Reset()
    {
    }

    #endregion
}
