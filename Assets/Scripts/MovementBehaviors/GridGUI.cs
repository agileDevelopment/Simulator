using UnityEngine;
using System.Collections;

public class GridGUI : FlightGUI
{
    LoadOptionsGUI simValues;
    public string nodeSpacingString = "45";
    public string nodeOrbitString = "50";
    public string nodeMaxSpeedString = "20";
    public int nodeSpacing;
    public int nodeMaxSpeed;
    public float radius;
    public float rotationSpeed;
    // Use this for initialization
    void Start()
    {
        simValues = gameObject.GetComponent<LoadOptionsGUI>();
        nodeSpacingString = "45";
        nodeOrbitString = "30";
        nodeMaxSpeedString = "0";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void showGUI()
    {
        GUI.BeginGroup(new Rect(((Screen.width - simValues.buttonWidth) / 2) + 250, Screen.height / 2 - 250, 250, 200));
        GUI.Box(new Rect(0, 0, 250, 400), "Grid Options");
        GUILayout.BeginArea(new Rect(5, 30, simValues.buttonWidth, simValues.buttonHeight * simValues.numberButtons));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Node Spacing", GUILayout.Width(simValues.menuLabelWidth));
        nodeSpacingString = GUILayout.TextField(nodeSpacingString, 4);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Node Orbit Radius", GUILayout.Width(simValues.menuLabelWidth));
        nodeOrbitString = GUILayout.TextField(nodeOrbitString, 4);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Node Speed", GUILayout.Width(simValues.menuLabelWidth));
        nodeMaxSpeedString = GUILayout.TextField(nodeMaxSpeedString, 4);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        GUI.EndGroup();
    }

    public override void setGuiValues()
    {
        radius = float.Parse(nodeOrbitString);
        rotationSpeed = Random.Range(5, float.Parse(nodeMaxSpeedString));
        nodeSpacing = int.Parse(nodeSpacingString);
        nodeMaxSpeed = int.Parse(nodeMaxSpeedString);
    }
    public override void setSpawnLocation()
    {
        int count = 0;
        float range = simValues.nodesSqrt + 1;
        for (int i = 0; i <= range; i++)
        {
            for (int j = 0; j < range; j++)
            {
                if (count < simValues.numNodes)
                {
                    GameObject gameNode = GameObject.Find("Node " + count);
                    if (gameNode.GetComponent<NodeController>().idNum < simValues.numNodes)
                    {
                        if (count == gameNode.GetComponent<NodeController>().idNum)
                        {
                            gameNode.transform.position = new Vector3(i * nodeSpacing + radius * 2, 10, j * nodeSpacing + radius);
                        }
                    }
                    ++count;
                }
            }
        }

    }

    public override void setSpawnLocation(GameObject node)
    { }

    public override void setFloor()
    {
        int floorSize = (int)(simValues.nodesSqrt * nodeSpacing + radius * 3);
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
