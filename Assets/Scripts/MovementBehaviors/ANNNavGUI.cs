using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ANNNavGUI : FlightGUI {
	LoadOptionsGUI simValues;
    public RTPopulationManager popManager;
    public NEATManager movementManager;

    public string spawnPointXString, spawnPointYString, spawnPointZString;
    public string goalPointXString, goalPointYString, goalPointZString;
	public string nodeOrbitString="50";
	public string nodeMaxSpeedString="20";
	public int spawnPointX, spawnPointY, spawnPointZ, goalPointX, goalPointY, goalPointZ;
	public int nodeMaxSpeed;

	// Use this for initialization
	void Start () {
        simValues = gameObject.GetComponent<LoadOptionsGUI>();
        popManager = gameObject.GetComponent<RTPopulationManager>();
        movementManager = new NEATManager(simValues.numNodes, 15);
	    
        nodeMaxSpeedString="15";
        spawnPointXString = spawnPointYString = spawnPointZString = "10";
        goalPointXString = goalPointZString = "390";
        goalPointYString = "10";
	}
	
	// Update is called once per frame
    void Update()
    {

    }

    public override void showGUI()
    {
        GUI.BeginGroup(new Rect(((Screen.width - simValues.buttonWidth) / 2) + 250, Screen.height / 2 - 250, 250, 400));
        GUI.Box(new Rect(0, 0, 250, 400), "ANN Nav Options");
        GUILayout.BeginArea(new Rect(5, 30, simValues.buttonWidth, simValues.buttonHeight * simValues.numberButtons));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spawn Point");
        spawnPointXString = GUILayout.TextField(spawnPointXString, 4);
        spawnPointYString = GUILayout.TextField(spawnPointYString, 4);
        spawnPointZString = GUILayout.TextField(spawnPointZString, 4);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Goal Point");
        goalPointXString = GUILayout.TextField(goalPointXString, 4);
        goalPointYString = GUILayout.TextField(goalPointYString, 4);
        goalPointZString = GUILayout.TextField(goalPointZString, 4);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Node Speed");
        nodeMaxSpeedString = GUILayout.TextField(nodeMaxSpeedString, 4);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        GUI.EndGroup();
    }

    public override void setGuiValues()
    {
        spawnPointX = int.Parse(spawnPointXString);
        spawnPointY = int.Parse(spawnPointYString);
        spawnPointZ = int.Parse(spawnPointZString);
        goalPointX = int.Parse(goalPointXString);
        goalPointY = int.Parse(goalPointYString);
        goalPointZ = int.Parse(goalPointZString);
        nodeMaxSpeed = int.Parse(nodeMaxSpeedString);
        movementManager.maxSpeed = nodeMaxSpeed;
    }

    public override void setSpawnLocation()
    {
        foreach (KeyValuePair<GameObject, MemberInfo> key_value in popManager.populationInfo)
        {
            key_value.Key.transform.position = new Vector3(spawnPointX, spawnPointY, spawnPointZ);
        }
    }

    public override void setSpawnLocation(GameObject gameObject)
    {
        gameObject.transform.position = new Vector3(spawnPointX, spawnPointY, spawnPointZ);
    }

    public override void setFloor()
    {
        int floorSize = int.Parse(goalPointXString) + 10;
        int center = floorSize / 2;
        GameObject floor = GameObject.Find("Floor");
        floor.transform.position = (new Vector3(center, -10, center));
        floor.transform.localScale = (new Vector3(floorSize, .1f, floorSize));
        Camera.main.transform.position = (new Vector3(center, floorSize / 2, center));
        Camera.main.isOrthoGraphic = true;
        Camera.main.orthographicSize = floor.transform.localScale.x / 2 + 50;
        floor.renderer.material.mainTextureScale = new Vector2(floorSize / 10, floorSize / 10);
    }

}
