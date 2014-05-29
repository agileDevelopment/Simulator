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

public class RTPopulationManager : PopulationManager
{
    public Dictionary<GameObject, MemberInfo> populationInfo = new Dictionary<GameObject, MemberInfo>();
    public void initializePopulation(string movementBehaviorClassName, string networkClassBehavior)
    {
        base.initializePopulation(movementBehaviorClassName, networkClassBehavior);
        loadData = gameObject.GetComponent<LoadOptionsGUI>();

        for (int i = 0; i < loadData.numNodes; i++)
        {
            GameObject node = buildMemberNode();
            populationInfo.Add(node, new MemberInfo());
        }
        gameObject.GetComponent<LoadOptionsGUI>().paused = false;
    }

    public void maintainPopulation()
    {
        if (replaceMembers)
        {
            GameObject newNode = buildMemberNode();
            populationInfo.Add(newNode, new MemberInfo());
            //((IFlightGUIOptions)newNode.GetComponent(movementBehaviorClassName + "GUI")).setSpawnLocation(newNode);
        }
    }

    public bool checkMember(GameObject member)
    {
        MemberInfo info = populationInfo[member];
        if (loadData.maxAge != 0 && loadData.maxAge < info.age)
        {
            populationInfo.Remove(member);
            maintainPopulation();
            return false;
        }
        return true;
    }
}
