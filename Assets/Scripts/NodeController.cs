//------------------------------------------------------------
//  Title: NodeController
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None
//
//  Description:  Main script for controlling node.  This script will call other scripts for different
//  functionality such as move and network.
//
//--------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class NodeController : MonoBehaviour {
	public NodeMove flightBehavior;
    public Network networkBehavior;
    public GameObject nodeText;
	LoadOptionsGUI simValues;
	public int idNum;
	public string idString;
    public bool selected;
    public Color oldColor;
 
	
	// Use this for initialization
	void Awake(){
		simValues = GameObject.Find("Spawner").GetComponent<LoadOptionsGUI>();
	//	flightBehavior = gameObject.AddComponent<Orbit>();
	}
	void Start () {
        nodeText = (GameObject)gameObject.transform.FindChild("Node Text").gameObject;
        selected = false;
        oldColor = Color.blue;

		//change this to implement a different movement controller
	
	}

    public void OnMouseDown()
    {
        unselectNodes();
        selected = true;
        oldColor = gameObject.renderer.material.color;
        gameObject.renderer.material.color = Color.green;

    }

    private void unselectNodes()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

        foreach (GameObject node in nodes)
        {
            if(node)
            node.GetComponent<NodeController>().selected = false;
           node.renderer.material.color = oldColor;
        }

    }
	

	//----------------Unity Functions------------------------------
	void OnTriggerEnter (Collider col)
	{

        if (col.gameObject.tag == "Node")
		{
            GameObject otherNode = col.gameObject;
            if (networkBehavior != null)
            {
                networkBehavior.addNeighbor(otherNode);
            }
        }
		
	}
	
	void OnTriggerExit(Collider col){
		if(col.gameObject.tag == "Node")
		{
			GameObject otherNode = col.gameObject;
            if (networkBehavior != null)
            {
                networkBehavior.removeNeighbor(otherNode);
            }
		}
	}
	
	
	// Update is called once per frame
	void Update () {

        if (selected)
            gameObject.renderer.material.color = Color.green;

		}
		
	void LateUpdate(){
		if(!simValues.paused && simValues.enableUpdate){
			updateLocation();
		}
	}
	//-------------------Custome Functions---------------------------------	
	void updateLocation(){
		flightBehavior.updateLocation();
	}
	

}
