//------------------------------------------------------------
//  Title: NodeLine
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: NodeController
//
//  Description: Generates visualizations between showing node network interaction.
//	This script is part of the linePrefab and will Start() will be called as soon
// 	as the prefab is instantiated.
//
//--------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class NodeLine : MonoBehaviour {
public GameObject linePrefab;
Hashtable lines;
LoadOptionsGUI simValues;
NodeController data;	
Color lineColor = Color.red;
float colorStep;
int count;
int midPoint;


void Start(){
		lines = new Hashtable();
		data = gameObject.GetComponent<NodeController>();
		simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
		midPoint = simValues.nodeCommRange/2;
		lineColor.r = 255;
		lineColor.g = 0;
		lineColor.b = 0;
		colorStep = (float)255/(simValues.nodeCommRange/2);
}

//public function to be called by nodeController if we need to add a connection
public void addLine(GameObject otherNode){
		int idNum = otherNode.GetComponent<NodeController>().idNum;
		if(otherNode.GetComponent<NodeController>().idNum < gameObject.GetComponent<NodeController>().idNum ){
			if(!lines.ContainsKey(idNum)){
				GameObject line = (GameObject)GameObject.Instantiate(linePrefab);
				line.tag = "Line";
				line.name = "line_" + gameObject.GetComponent<NodeController>().idNum.ToString() + otherNode.GetComponent<NodeController>().idNum.ToString();
				line.transform.parent = gameObject.transform;
				lines.Add(idNum, line);									
				}
		}
}
	//public function to be called by nodeController if we need to remove a connection
public void removeLine(GameObject otherNode){
		int idNum = otherNode.GetComponent<NodeController>().idNum;
		if(lines.ContainsKey(idNum)){
			lines.Remove(idNum);
			Destroy(GameObject.Find ("line_" + data.idNum + idNum ));
		}
}


//called every frame.
void Update(){
		//if we're not paused
		if(!simValues.paused){
			//if lines are disabled in the "in-simulation" menu
			if(!simValues.drawLine){
				foreach(DictionaryEntry entry in lines){
					GameObject line = (GameObject)entry.Value;
					line.GetComponent<LineRenderer>().enabled = false;
				}
			}
			}
		//if lines are enabled in the "in-simulation" menu		
		if(simValues.drawLine){
			if(simValues.updateLines){
				GameObject source = gameObject;
			
				//loop through all the lines in our container and update accordingly
                foreach (DictionaryEntry entry in lines)
                {
                    GameObject line = (GameObject)entry.Value;
                    if (line)//check to see if its been destroyed already
                        line.GetComponent<LineRenderer>().enabled = true;
                    GameObject dest = GameObject.Find("Node " + entry.Key);

                    line.GetComponent<LineRenderer>().SetPosition(0, source.transform.position);
                    line.GetComponent<LineRenderer>().SetPosition(1, dest.transform.position);
                    lineColor = Color.white;
                    if (simValues.adaptiveNetworkColor)
                    {
                        float distance = Vector3.Distance(source.transform.position, dest.transform.position);
                        if (line != null)
                        {
                            lineColor = Color.black;
                            //Lots of RGB math here... basically spilts the line into two categories, halfway and less
                            // than half way and adjusts the color accordly.  

                            //greater than midway show Red - Yellow
                            if (distance > midPoint)
                            {
                                float delta = ((simValues.nodeCommRange - distance) / midPoint);
                                lineColor.r = 255;
                                lineColor.g = (delta * colorStep) * 3;
                            }
                            //less than midway, show Yellow - Green				
                            if (distance <= midPoint)
                            {
                                float delta = (distance / midPoint);
                                lineColor.g = 255;
                                lineColor.r = (delta * colorStep) - 10;
                            }

                            //extra bonus for being close
                            if (distance < 20)
                            {
                                lineColor.g = 255;
                                lineColor.r = 0;
                            }
                        }
                    }
                    line.GetComponent<LineRenderer>().SetColors(lineColor, lineColor);
                }
		}
		}
}
}