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

public class ANNPopulationManager : PopulationManager, IMovementManager
{
    Vector3 goal;
    public int _lifespan;
    static string CHAMPION_FILE = "File.xml";
    static string DOT_FILE = "File.dot";

    static NeatEvolutionAlgorithm<NeatGenome> neatAlgorithm;
    static NeatEvolutionAlgorithm<NeatGenome> esHyperNeatAlgorithm;
    public Dictionary<GameObject, BlackBoxNavigator> population = new Dictionary<GameObject, BlackBoxNavigator>();

    public new void initializePopulation(string movementBehaviorClassName, string networkClassBehavior)
    {
        base.initializePopulation(movementBehaviorClassName, networkClassBehavior);

        XmlDocument xmlConfig = new XmlDocument();
        xmlConfig.Load("NavigationParameters.xml");

        print("Initializing NEAT experiment");
        NEATNavigationExperiment neatExperiment = new NEATNavigationExperiment(new BlackBoxEvaluator(this));
        neatExperiment.Initialize("NEAT Experiment", xmlConfig.DocumentElement);
        _lifespan = neatExperiment._lifespan;
        neatAlgorithm = neatExperiment.CreateEvolutionAlgorithm(0, () =>
        {
            //neatAlgorithm.UpdateEvent += new EventHandler(ea_UpdateEvent);
            neatAlgorithm.StartContinueMainThread();
        });
        print("NEAT running with PhenomeEvaluator: " + neatExperiment.PhenomeEvaluator);

        print("Initializing ES-HyperNEAT experiment");
        ESHyperNEATNavigationExperiment esHyperNeatExperiment = new ESHyperNEATNavigationExperiment(new BlackBoxEvaluator(this));
        esHyperNeatExperiment.Initialize("ES-HyperNEAT Experiment", xmlConfig.DocumentElement);
        esHyperNeatAlgorithm = esHyperNeatExperiment.CreateEvolutionAlgorithm(1, () =>
        {
            esHyperNeatAlgorithm.UpdateEvent += new EventHandler(ea_UpdateEvent);
            esHyperNeatAlgorithm.StartContinueMainThread();
        });
        print("ES-HyperNEAT running with PhenomeEvaluator: " + esHyperNeatExperiment.PhenomeEvaluator);
    }

    static void ea_UpdateEvent(object sender, EventArgs e)
    {
        Debug.Log(string.Format("NEAT - Generation={0:N0}, Evaluations={1:N0}, Best Fitness={2:N6}, Mean Fitness={3:N6}, Mean Complexity={4:N6}",
            neatAlgorithm.CurrentGeneration, neatAlgorithm.Statistics._totalEvaluationCount, neatAlgorithm.Statistics._maxFitness,
            neatAlgorithm.Statistics._meanFitness, neatAlgorithm.Statistics._meanComplexity));

        Debug.Log(string.Format("ES-HyperNEAT - Generation={0:N0}, Evaluations={1:N0}, Best Fitness={2:N6}, Mean Fitness={3:N6}, Mean Complexity={4:N6}",
            esHyperNeatAlgorithm.CurrentGeneration, esHyperNeatAlgorithm.Statistics._totalEvaluationCount, esHyperNeatAlgorithm.Statistics._maxFitness,
            esHyperNeatAlgorithm.Statistics._meanFitness, esHyperNeatAlgorithm.Statistics._meanComplexity));

        // Save the best genome to file
        var neatDoc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { neatAlgorithm.CurrentChampGenome }, false);
        neatDoc.Save("champNeat" + CHAMPION_FILE);

        var esHyperNeatDoc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { esHyperNeatAlgorithm.CurrentChampGenome }, false);
        esHyperNeatDoc.Save("champeESHyperNeat" + CHAMPION_FILE);

        CPPNDotWriterStatic.saveCPPNasDOT(neatAlgorithm.CurrentChampGenome, "neatChamp" + DOT_FILE);
        CPPNDotWriterStatic.saveCPPNasDOT(esHyperNeatAlgorithm.CurrentChampGenome, "esHyperNEATChamp" + DOT_FILE);
    }

    public ArrayList updateLocation(GameObject node, ArrayList sensorInfo, bool isAlive)
    {
        BlackBoxNavigator navigator = population[node];
        ArrayList destination = navigator.updateLocation(sensorInfo, isAlive);
        return destination;
    }

    public void checkpointNotify(GameObject node, double checkpointReward)
    {
        BlackBoxNavigator navigator = population[node];
        navigator.checkpointNotify(checkpointReward);
    }

	public void goalReachedNotify(GameObject node) 
	{
		BlackBoxNavigator navigator = population[node];
		navigator.goalReachedNotify();
	}
}
