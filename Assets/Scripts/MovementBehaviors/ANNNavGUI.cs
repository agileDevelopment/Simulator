using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ANNNavGUI : MonoBehaviour, IFlightGUIOptions {
	LoadOptionsGUI simValues;
    public NEATPopulationManager popManager;

    public string spawnPointXString = "10", spawnPointYString = "100", spawnPointZString = "10";
    public string goalPointXString = "390", goalPointYString = "10", goalPointZString = "390";
	public string nodeMaxSpeedString="150";
	public int spawnPointX, spawnPointY, spawnPointZ, goalPointX, goalPointY, goalPointZ;
	public int nodeMaxSpeed;

	// Use this for initialization
	void Start () {
        simValues = gameObject.GetComponent<LoadOptionsGUI>();
        popManager = gameObject.GetComponent<NEATPopulationManager>();
	}
	
	// Update is called once per frame
    void Update()
    {

    }

    public void showGUI()
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

    public void setGuiValues()
    {
        spawnPointX = int.Parse(spawnPointXString);
        spawnPointY = int.Parse(spawnPointYString);
        spawnPointZ = int.Parse(spawnPointZString);
        goalPointX = int.Parse(goalPointXString);
        goalPointY = int.Parse(goalPointYString);
        goalPointZ = int.Parse(goalPointZString);
        nodeMaxSpeed = int.Parse(nodeMaxSpeedString);
    }

    public Vector3 getGoalLocation()
    {
        return new Vector3(goalPointX, goalPointY, goalPointZ);
    }

    public Vector3 getSpawnLocation()
    {
        return new Vector3(spawnPointX, spawnPointY, spawnPointZ);
    }

    public void setFloor()
    {
        int floorSize = 400;
        int center = floorSize / 2;
        GameObject floor = GameObject.Find("Floor");
        floor.transform.position = (new Vector3(center, -10, center));
        floor.transform.localScale = (new Vector3(floorSize, .1f, floorSize));
        foreach (Camera c in Camera.allCameras)
        {
            print(c.gameObject.name);
            if (c.gameObject.name == "Main Camera")
            {
                c.transform.position = (new Vector3(5 * floorSize / 4, 200, -2 * floorSize / 4));
                c.transform.LookAt(new Vector3(center, -10, center));
            }
            else if (c.gameObject.name == "Second Camera")
            {
                c.transform.position = (new Vector3(-10, 110, -center));
                c.transform.LookAt(new Vector3(center, -10, center));
            }
        }
        floor.renderer.material.mainTextureScale = new Vector2(floorSize / 10, floorSize / 10);
    }
}
