using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ANNNavGUI : MonoBehaviour, IFlightGUIOptions {
	LoadOptionsGUI simValues;
    public ANNPopulationManager popManager;

    public int floorSize = 300;
    public string spawnPointXString = "50", spawnPointYString, spawnPointZString = "50";
    public string goalPointXString, goalPointYString = "50", goalPointZString;
	public string nodeMaxSpeedString="150", numCheckpointsString="30";
	public int spawnPointX, spawnPointY, spawnPointZ, goalPointX, goalPointY, goalPointZ, numCheckpoints;
	public int nodeMaxSpeed;

    GameObject goalPrefab;
	GameObject obstaclePrefab;

	// Use this for initialization
	void Start () {
        simValues = gameObject.GetComponent<LoadOptionsGUI>();
        popManager = gameObject.GetComponent<ANNPopulationManager>();
        goalPrefab = (GameObject)Resources.Load("GoalPrefab");
		obstaclePrefab = (GameObject)Resources.Load ("ObstaclePrefab");
        spawnPointYString = goalPointXString = goalPointZString = "" + (floorSize - 50);
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
		GUILayout.BeginHorizontal();
		GUILayout.Label("Number of Breadcrumbs");
		numCheckpointsString = GUILayout.TextField(numCheckpointsString, 4);
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
		numCheckpoints = int.Parse (numCheckpointsString);
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
        int center = floorSize / 2;

        GameObject floor = GameObject.Find("Floor");
        floor.transform.position = (new Vector3(center, -10, center));
        floor.transform.localScale = (new Vector3(floorSize, .1f, floorSize));

        Camera mainCamera = null;
        foreach (Camera c in Camera.allCameras)
        {
            if (c.gameObject.name == "Main Camera")
            {
                mainCamera = c; 
                c.transform.position = (new Vector3(5 * floorSize / 4, 5 * floorSize / 4, -1 * floorSize / 4));
                c.transform.LookAt(new Vector3(center, 3 * center / 4, center));
            }
            else if (c.gameObject.name == "Second Camera")
            {
                c.transform.position = (new Vector3(center, center, -floorSize));
                c.transform.LookAt(new Vector3(center, center, center));
            }
			else if (c.gameObject.name == "Third Camera")
			{
				c.transform.position = (getGoalLocation() + new Vector3(80, 0, 80));
				c.transform.LookAt(getSpawnLocation() - new Vector3(0, 150, 0));
			}
        }
        floor.renderer.material.mainTextureScale = new Vector2(floorSize / 10, floorSize / 10);

		float spawnToGoal = Vector3.Distance(getSpawnLocation(), getGoalLocation());
		float spacing = 4.0f / 5.0f * spawnToGoal / (numCheckpoints);

		GameObject goalNode;
		goalNode = (GameObject)GameObject.Instantiate(goalPrefab);
		goalNode.name = "GoalNode 0";
        goalNode.layer = 2;
		goalNode.transform.position = getGoalLocation();
        goalNode.transform.LookAt(getSpawnLocation());

        GameObject checkpointNode;
        for (int i = 1; i < numCheckpoints; i++)
        {
            checkpointNode = (GameObject)GameObject.Instantiate(goalPrefab);
            checkpointNode.name = "GoalNode " + i;
            checkpointNode.layer = 2;
            checkpointNode.transform.localScale = new Vector3(1, 1, 1);
            checkpointNode.transform.position = goalNode.transform.position + (goalNode.transform.forward * spacing * i);

            SphereCollider collider = checkpointNode.AddComponent<SphereCollider>();
            collider.center = Vector3.zero;
            collider.radius = spacing / checkpointNode.transform.localScale.x;
        }
        goalNode.transform.rotation = Quaternion.LookRotation(Vector3.forward);

        buildBoundary(center);
        layObstacles();
    }

    void layObstacles()
    {
        int cubeScale = 30;
        int spacing = 75;
        for (int i = 0; i <= getSpawnLocation().y + spacing; i += spacing)
        {
            layObstacleLayer(i, cubeScale, spacing);
        }
    }

    void layObstacleLayer(int yOffset, int size, int spacing)
    {
        GameObject obstacleNode;
        int numObstacles = floorSize / spacing;
        for (int i = 0; i <= numObstacles; i++)
        {
            for (int j = 0; j <= numObstacles; j++)
            {
                obstacleNode = (GameObject)GameObject.Instantiate(obstaclePrefab);
                obstacleNode.transform.position = new Vector3(i * spacing, yOffset, j * spacing);
                obstacleNode.transform.localScale = new Vector3(Random.Range(5, size), Random.Range(5, size), Random.Range(5, size));
                Color color = obstacleNode.renderer.material.color;
                color.a = 0.5f;
                obstacleNode.renderer.material.color = color;
            }
        }
    }

    void buildBoundary(int center)
    {
        GameObject boundaryObstacle;

        boundaryObstacle = (GameObject)GameObject.Instantiate(obstaclePrefab);
        boundaryObstacle.name = "Boundary Back";
        boundaryObstacle.transform.localScale = new Vector3(floorSize, floorSize, 1);
        boundaryObstacle.transform.position = new Vector3(center, -10 + center, 0);
        Color invisibleColor = boundaryObstacle.renderer.material.color;
        invisibleColor.a = 0;
        boundaryObstacle.renderer.material.color = invisibleColor;
        boundaryObstacle.tag = "Boundary";

        boundaryObstacle = (GameObject)GameObject.Instantiate(obstaclePrefab);
        boundaryObstacle.name = "Boundary Front";
        boundaryObstacle.transform.localScale = new Vector3(floorSize, floorSize, 1);
        boundaryObstacle.transform.position = new Vector3(center, -10 + center, floorSize);
        boundaryObstacle.renderer.material.color = invisibleColor;
        boundaryObstacle.tag = "Boundary";

        boundaryObstacle = (GameObject)GameObject.Instantiate(obstaclePrefab);
        boundaryObstacle.name = "Boundary Right";
        boundaryObstacle.transform.localScale = new Vector3(1, floorSize, floorSize);
        boundaryObstacle.transform.position = new Vector3(floorSize, -10 + center, center);
        boundaryObstacle.renderer.material.color = invisibleColor;
        boundaryObstacle.tag = "Boundary";

        boundaryObstacle = (GameObject)GameObject.Instantiate(obstaclePrefab);
        boundaryObstacle.name = "Boundary Left";
        boundaryObstacle.transform.localScale = new Vector3(1, floorSize, floorSize);
        boundaryObstacle.transform.position = new Vector3(0, -10 + center, center);
        boundaryObstacle.renderer.material.color = invisibleColor;
        boundaryObstacle.tag = "Boundary";

        boundaryObstacle = (GameObject)GameObject.Instantiate(obstaclePrefab);
        boundaryObstacle.name = "Boundary Top";
        boundaryObstacle.transform.localScale = new Vector3(floorSize, 1, floorSize);
        boundaryObstacle.transform.position = new Vector3(center, floorSize - 10, center);
        boundaryObstacle.renderer.material.color = invisibleColor;
        boundaryObstacle.tag = "Boundary";
    }
}
