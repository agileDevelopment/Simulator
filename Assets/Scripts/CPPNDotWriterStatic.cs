using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;

public class CPPNDotWriterStatic
{
    //saves a CPPN in dot file format. 
    //Assumes that inputs are X1, Y1, X2, Y2, Z
    public static void saveCPPNasDOT(NeatGenome genome, string filename)
    {
        StreamWriter SW = File.CreateText(filename);
        SW.WriteLine("digraph g { ");

        String activationType = "";
        IActivationFunctionLibrary activationFuncLib = DefaultActivationFunctionLibrary.CreateLibraryCppn();

        foreach (NeuronGene neuron in genome.NeuronGeneList)
        {


            switch (neuron.NodeType)
            {
                case NodeType.Bias: SW.WriteLine("N0 [shape=box, label=Bias]"); break;
                case NodeType.Input:

                    string str = "?";
                    switch (neuron.InnovationId)
                    {
                        case 1: str = "X1"; break;
                        case 2: str = "Y1"; break;
                        case 3: str = "X2"; break;
                        case 4: str = "Y2"; break;
                        case 5: str = "Z"; break;

                    }
                    SW.WriteLine("N" + neuron.InnovationId + "[shape=box label=" + str + "]");
                    break;
                case NodeType.Output: SW.WriteLine("N" + neuron.InnovationId + "[shape=triangle]"); break;
                case NodeType.Hidden:
                    if (activationFuncLib.GetFunction(neuron.ActivationFnId) is BipolarSigmoid) activationType = "S";
                    if (activationFuncLib.GetFunction(neuron.ActivationFnId) is Gaussian) activationType = "G";
                    if (activationFuncLib.GetFunction(neuron.ActivationFnId) is Linear) activationType = "L";
                    if (activationFuncLib.GetFunction(neuron.ActivationFnId) is Sine) activationType = "Si";

                    SW.WriteLine("N" + neuron.InnovationId + "[shape=circle, label=N" + neuron.InnovationId + "_" + activationType + ", fillcolor=gray]");
                    break;
            }

        }

        foreach (ConnectionGene gene in genome.ConnectionGeneList)
        {
            SW.Write("N" + gene.SourceNodeId + " -> N" + gene.TargetNodeId + " ");

            if (gene.Weight > 0)
                SW.WriteLine("[color=black] ");
            else if (gene.Weight < -0)
                SW.WriteLine("[color=red] [arrowType=inv]");
        }

        //foreach (ModuleGene mg in genome.)
        //{
        //    foreach (uint sourceID in mg.InputIds)
        //    {
        //        foreach (uint targetID in mg.OutputIds)
        //        {
        //            SW.Write("N" + sourceID + " -> N" + targetID + " ");

        //            SW.WriteLine("[color=gray]");
        //        }
        //    }
        //}

        SW.WriteLine(" { rank=same; ");
        foreach (NeuronGene neuron in genome.NeuronGeneList)
        {
            if (neuron.NodeType == NodeType.Output)
            {
                SW.WriteLine("N" + neuron.InnovationId);
            }
        }
        SW.WriteLine(" } ");


        SW.WriteLine(" { rank=same; ");
        foreach (NeuronGene neuron in genome.NeuronGeneList)
        {
            if (neuron.NodeType == NodeType.Input)
            {
                SW.Write("N" + neuron.InnovationId + " ->");
            }
        }
        //Also the bias neuron on the same level
        SW.WriteLine("N0 [style=invis]");
        SW.WriteLine(" } ");

        SW.WriteLine("}");

        SW.Close();
    }

}
