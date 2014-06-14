﻿using System;
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
    static string resultsFile = "resultsESHyperNEAT" + (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + ".csv";

    static NeatEvolutionAlgorithm<NeatGenome> neatAlgorithm;
    static NeatEvolutionAlgorithm<NeatGenome> esHyperNeatAlgorithm;
    static MLPBPNavigationExperiment mlpbpExperiment;

    public Dictionary<GameObject, BlackBoxNavigator> population = new Dictionary<GameObject, BlackBoxNavigator>();

    public new void initializePopulation(string movementBehaviorClassName, string networkClassBehavior)
    {
        base.initializePopulation(movementBehaviorClassName, networkClassBehavior);

        XmlDocument xmlConfig = new XmlDocument();
        xmlConfig.Load("NavigationParameters.xml");

        //print("Initializing MLPBP experiment");
        mlpbpExperiment = new MLPBPNavigationExperiment(new BlackBoxEvaluator(this), 100);
        mlpbpExperiment.Initialize("MLP w/ BP Experiment", xmlConfig.DocumentElement);
        _lifespan = mlpbpExperiment.LifeSpan;
        //mlpbpExperiment.StartContinueMainThread();
        //print("MLP w/ BP running");

        //print("Initializing NEAT experiment");
        //NEATNavigationExperiment neatExperiment = new NEATNavigationExperiment(new BlackBoxEvaluator(this));
        //neatExperiment.Initialize("NEAT Experiment", xmlConfig.DocumentElement);
        //neatAlgorithm = neatExperiment.CreateEvolutionAlgorithm(0, () =>
        //{
        //    //neatAlgorithm.UpdateEvent += new EventHandler(ea_UpdateEvent);
        //    neatAlgorithm.StartContinueMainThread();
        //});
        //print("NEAT running");

        print("Initializing ES-HyperNEAT experiment");
        ESHyperNEATNavigationExperiment esHyperNeatExperiment = new ESHyperNEATNavigationExperiment(new BlackBoxEvaluator(this));
        esHyperNeatExperiment.Initialize("ES-HyperNEAT Experiment", xmlConfig.DocumentElement);
        esHyperNeatAlgorithm = esHyperNeatExperiment.CreateEvolutionAlgorithm(1, () =>
        {
            esHyperNeatAlgorithm.UpdateEvent += new EventHandler(ea_UpdateEvent);
            esHyperNeatAlgorithm.StartContinueMainThread();
        });
        print("ES-HyperNEAT running");
    }

    static void ea_UpdateEvent(object sender, EventArgs e)
    {
        //string mlpbpText = string.Format("MLP w/ BP   ,{0:N0},{1:N0},{2:N6},{3:N6},{4:N6}",
        //    mlpbpExperiment.CurrentGeneration, mlpbpExperiment._totalEvaluationCount, mlpbpExperiment._maxFitness,
        //    mlpbpExperiment._meanFitness, 20);
        //string neatText = string.Format("NEAT        ,{0:N0},{1:N0},{2:N6},{3:N6},{4:N6}",
        //    neatAlgorithm.CurrentGeneration, neatAlgorithm.Statistics._totalEvaluationCount, neatAlgorithm.Statistics._maxFitness,
        //    neatAlgorithm.Statistics._meanFitness, neatAlgorithm.Statistics._meanComplexity);
        string esHyperNeatText = string.Format("ES-HyperNEAT,{0:N0},{1:N0},{2:N6},{3:N6},{4:N6}",
            esHyperNeatAlgorithm.CurrentGeneration + 1, esHyperNeatAlgorithm.Statistics._totalEvaluationCount, esHyperNeatAlgorithm.Statistics._maxFitness,
            esHyperNeatAlgorithm.Statistics._meanFitness, esHyperNeatAlgorithm.Statistics._meanComplexity);

        // Save the best genome to file
        //var neatDoc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { neatAlgorithm.CurrentChampGenome }, false);
        //neatDoc.Save("champNeat" + CHAMPION_FILE);

        var esHyperNeatDoc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { esHyperNeatAlgorithm.CurrentChampGenome }, false);
        esHyperNeatDoc.Save("champESHyperNeat" + CHAMPION_FILE);

        //CPPNDotWriterStatic.saveCPPNasDOT(neatAlgorithm.CurrentChampGenome, "neatChamp" + DOT_FILE);
        //CPPNDotWriterStatic.saveCPPNasDOT(esHyperNeatAlgorithm.CurrentChampGenome, "esHyperNEATChamp" + DOT_FILE);

        //Debug.Log(mlpbpText);
        //Debug.Log(neatText);
        Debug.Log(esHyperNeatText);

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resultsFile, true))
        {
            //file.WriteLine(mlpbpText);
            //file.WriteLine(neatText);
            file.WriteLine(esHyperNeatText);
        }
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