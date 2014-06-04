using UnityEngine;
using System.Collections;

public class OrbitGUI : FlightGUI {

	LoadOptionsGUI simValues;
	public string nodeOrbitString;
	public string nodeMaxSpeedString;
	public int nodeMaxSpeed;
	public float radius;
	public float rotationSpeed;
	// Use this for initialization
	void Start () {
		simValues = gameObject.GetComponent<LoadOptionsGUI>();
		nodeOrbitString="200";
		nodeMaxSpeedString="30";
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}


    public override void showGUI()
    {
		    GUI.BeginGroup(new Rect(((Screen.width- simValues.buttonWidth)/2)+250 , Screen.height/2-250, 250, 400));
		    GUI.Box(new Rect(0, 0, 250, 400), "Orbit Options");
		    GUILayout.BeginArea(new Rect(5 , 30,simValues.buttonWidth,simValues.buttonHeight*simValues.numberButtons));
	    GUILayout.BeginHorizontal();
		    GUILayout.Label("Node Orbit Radius",GUILayout.Width(simValues.menuLabelWidth));
	    nodeOrbitString = GUILayout.TextField(nodeOrbitString,4);
	    GUILayout.EndHorizontal();
	    GUILayout.BeginHorizontal();
		    GUILayout.Label("Max Node Speed",GUILayout.Width(simValues.menuLabelWidth));
	    nodeMaxSpeedString = GUILayout.TextField(nodeMaxSpeedString,4);
	    GUILayout.EndHorizontal();
	    GUILayout.EndArea();
	    GUI.EndGroup();
    }

    public override void setGuiValues()
    {
        base.setGuiValues();
	    radius = float.Parse(nodeOrbitString);
	    rotationSpeed = Random.Range(5,float.Parse(nodeMaxSpeedString));
	    nodeMaxSpeed  = int.Parse(nodeMaxSpeedString);
    }

    public override void setSpawnLocation()
    {
        int count = 1;
		float range = simValues.nodesSqrt;
	    for(int i=0; i <= range; i++){
		    for(int j=0; j < range;j++){
			    if(count < simValues.numNodes){
			        GameObject gameNode = GameObject.Find("Node " + count);
			        if(gameNode.GetComponent<NodeController>().idNum < simValues.numNodes){
				        if(count == gameNode.GetComponent<NodeController>().idNum+1){
							gameNode.transform.position = new Vector3(center+count*radius,10,center+count*radius);
				        }
			        }
			        ++count;
			    }
			}
		}
	}

    public override void setSpawnLocation(GameObject node)
    {}
		
    public override void setFloor(){
		    floorSize = (int)(radius*2.5);
    center = floorSize/2;
    GameObject floor = GameObject.Find("Floor");
    floor.transform.position = (new Vector3(center, -center, center));
    floor.transform.localScale = (new Vector3(floorSize,.1f,floorSize));	
    camera2.isOrthoGraphic = true;
    camera2.orthographicSize = floor.transform.localScale.x / 2 + 50;
	camera2.transform.position = (new Vector3(center,100,center));
    camera2.rect = new Rect(.7f, .7f, .3f, .3f);
    Camera.main.transform.position = (new Vector3(floorSize, floorSize / 2, floorSize));
    Camera.main.transform.LookAt(new Vector3(center, 0, center));
    floor.renderer.material.mainTextureScale = new Vector2(floorSize/10, floorSize/10);
    }


}
