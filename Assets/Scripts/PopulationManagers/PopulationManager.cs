//------------------------------------------------------------
//  Title: LoadOptionsGui
//  Date: 5-20-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Joshua Christman
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: MonoBehaviour
//
//  Description:  Defines a way to manage a real-time population
//--------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PopulationManager : MonoBehaviour
{
    public GameObject nodePrefab;
    public LoadOptionsGUI loadData;
    public string movementBehaviorClassName = "";
    public string networkClassBehavior = "";
    public int globalCount = 0;
    public object buildMemberLock = new object(); // We need this to prevent many members dying and the count being messed up by threading
    public bool replaceMembers = true; // Can add a toggle later for programs that want to just kill members without replacement

    public void initializePopulation(string movementBehaviorClassName, string networkClassBehavior)
    {
        this.movementBehaviorClassName = movementBehaviorClassName;
        this.networkClassBehavior = networkClassBehavior;
        nodePrefab = (GameObject)Resources.Load("NodePrefab");
    }

    public GameObject buildMemberNode(Color nodeColor)
    {
        GameObject node;
        // These next lines will instantiate an game object with the appropriate data
        node = (GameObject)GameObject.Instantiate(nodePrefab);
        lock (buildMemberLock) // We need to lock on the global node count
        {
            NodeController data = node.GetComponent<NodeController>();

            node.AddComponent(movementBehaviorClassName);
            if (networkClassBehavior != "")
                node.AddComponent(networkClassBehavior);

            node.name = "Node " + globalCount;
            node.renderer.material.color = nodeColor;
            node.layer = 2;

            data.idNum = globalCount;
            data.idString = "Node " + globalCount;
            data.flightBehavior = (NodeMove)node.GetComponent(movementBehaviorClassName);
            if (networkClassBehavior != "")
                data.networkBehavior = (INetworkBehavior)node.GetComponent(networkClassBehavior);
            else
                data.networkBehavior = null;

            node.GetComponent<SphereCollider>().radius = 0.5f;

            globalCount++;
        }

        return node;
    }

    public void removeMemberNode(GameObject node)
    {
        DestroyImmediate(node);
    }
}
