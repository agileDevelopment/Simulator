using System;
using System.Xml;
using System.Text;
using System.IO;
using log4net.Config;
using System.Collections;
using System.Collections.Generic;
using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.EvolutionAlgorithms;
using UnityEngine;

public class NEATPopulationManager : PopulationManager, IMovementManager, IPhenomeEvaluator<IBlackBox>
{
    const double StopFitness = 10.0;
    ulong _evalCount;
    bool _stopConditionSatisfied = false;
    Vector3 goal;
    int _lifespan;

    static NeatEvolutionAlgorithm<NeatGenome> algorithm;
    public Dictionary<GameObject, NEATNavigator> population = new Dictionary<GameObject, NEATNavigator>();

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
    public void Evaluate(IBlackBox box, SharpNeat.Core.Func<double> lambda)
    {
        StartCoroutine(_Evaluate(box, lambda));
    }

    IEnumerator _Evaluate(IBlackBox box, SharpNeat.Core.Func<double> lambda)
    {
        double fitness = 0;
        GameObject node = buildMemberNode();
        //print("Built new Node");
        NEATNavigator nav = new NEATNavigator(box);
        population.Add(node, nav);

        while (nav.age < _lifespan)
        {
            yield return null;
        }

        //print("Node expired");

        fitness = Math.Max(nav.fitness, 0);
        _evalCount++;
        lambda(fitness);
        removeMemberNode(node);
    }

    /// <summary>
    /// Reset the internal state of the evaluation scheme if any exists.
    /// Note. The XOR problem domain has no internal state. This method does nothing.
    /// </summary>
    public void Reset()
    {
    }

    #endregion

    public new void initializePopulation(string movementBehaviorClassName, string networkClassBehavior)
    {
        base.initializePopulation(movementBehaviorClassName, networkClassBehavior);

        NavigationExperiment experiment = new NavigationExperiment(this);

        XmlDocument xmlConfig = new XmlDocument();
        xmlConfig.Load("NavigationParameters.xml");
        experiment.Initialize("Navigation Experiment", xmlConfig.DocumentElement);
        _lifespan = experiment._lifespan;

        algorithm = experiment.CreateEvolutionAlgorithm(() =>
        {
            algorithm.UpdateEvent += new EventHandler(ea_UpdateEvent);
            algorithm.StartContinueMainThread();
        });
    }

    static void ea_UpdateEvent(object sender, EventArgs e)
    {
        Debug.Log(string.Format("gen={0:N0} bestFitness={1:N6}", algorithm.CurrentGeneration, algorithm.Statistics._maxFitness));

        // Save the best genome to file
        //var doc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { _ea.CurrentChampGenome }, false);
        //doc.Save(CHAMPION_FILE);
    }

    public ArrayList updateLocation(GameObject node, ArrayList sensorInfo)
    {
        NEATNavigator navigator = population[node];
        ArrayList destination = navigator.updateLocation(sensorInfo);
        return destination;
    }

    public void checkpointNotify(GameObject node, double checkpointReward)
    {
        NEATNavigator navigator = population[node];
        navigator.checkpointNotify(checkpointReward);
    }

	public void goalReachedNotify(GameObject node) 
	{
		NEATNavigator navigator = population[node];
		navigator.goalReachedNotify();
	}
}
