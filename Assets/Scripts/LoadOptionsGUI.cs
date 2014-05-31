//------------------------------------------------------------
//  Title: LoadOptionsGui
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None
//
//  Description:  Defines what options for the simulator to use.  
//--------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

public class LoadOptionsGUI : MonoBehaviour {
    public GameObject linePrefab;
	GUIContent[] flightControllerList;
    GUIContent[] networkControllerList;
    private ComboBox flightComboBoxControl;// = new ComboBox();
    private ComboBox networkComboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();
	public FlightGUI flightGUI;
    public NetworkGUI networkGUI;
	public Renderer cubeRender;	
	public int buttonHeight = 50;
	public int buttonWidth = 200;
	public int buttonSpace = 100;	// Use this for initialization
	public int numberButtons;
	public string numNodesString;
	public int numNodes;
	public string simRunTimeString="30";
	public int simRunTime;
    static string defaultMoveString = "Select Behavior";
	public string pauseString = "Pause Simulation";
	public bool paused=false;
	public bool showMainGui;
	public string movementChoice = "0";
    public string networkChoice = "none";
    private string lastNetComponentStr = "";
    private string lastMoveComponentStr = "";
	public int menuLabelWidth = 170;
	public int menuFieldWidth = 100;
	public int nodesSqrt;
	public bool enableUpdate;
	public bool slowMotion = false;
	public string slowMoRateString;
	public int slowMoRate;
	public int counter = 1;

    public Dictionary<int, string> movementBehaviorLoader = new Dictionary<int, string>();
    public Dictionary<int, string> networkBehaviorLoader = new Dictionary<int, string>();

    // Real Time Population Options
    public int maxAge = 0;

//-----------------------Unity Defined Functions-------------------------------

	void Start () {
		numberButtons=5;
		numNodesString="100";
        simRunTimeString = "30";
		slowMoRateString = "2";
        showMainGui = true;
		//List for types of Flight Controllers
		//Must be updated when new FlightBehaviors are implemented

        // Initialize the various attached classes
        movementBehaviorLoader.Add(0, defaultMoveString);
        movementBehaviorLoader.Add(1, "ANNNav");
        movementBehaviorLoader.Add(2, "Grid");
        movementBehaviorLoader.Add(3, "Orbit");
        networkBehaviorLoader.Add(0, "none");
        networkBehaviorLoader.Add(1, "AODV");
        networkBehaviorLoader.Add(2, "MCDSGA");

		flightControllerList = new GUIContent[movementBehaviorLoader.Count];
        
        foreach (KeyValuePair<int, string> key_value in movementBehaviorLoader) {
            flightControllerList[key_value.Key] = new GUIContent(key_value.Value);
       }

		listStyle.normal.textColor = Color.white; 
		listStyle.onHover.background =
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.left =
			listStyle.padding.right =
				listStyle.padding.top =
					listStyle.padding.bottom = 4;
        flightComboBoxControl = new ComboBox(new Rect((Screen.width - buttonWidth) / 2 - 140, Screen.height / 2 - 150, 135, 20),
			 flightControllerList[0], flightControllerList, "button", "box", listStyle);

        //List for Network Controllers
        networkControllerList = new GUIContent[networkBehaviorLoader.Count];
        foreach (KeyValuePair<int, string> key_value in networkBehaviorLoader)
        {
            networkControllerList[key_value.Key] = new GUIContent(key_value.Value);
        }

        networkComboBoxControl = new ComboBox(new Rect((Screen.width - buttonWidth) / 2 - 140, Screen.height / 2 - 120, 135, 20), networkControllerList[0], networkControllerList, "button", "box", listStyle);
	}
	
	void OnGUI () {
		//main interface to be shown when program first runs;
		if(showMainGui){
            showMainMenu();
		}

		//show this menu while simulation is running
		else{
            showRunningMenu();
		}	
	}

	void Update(){
        if (showMainGui)
            updateComponents();
		if(slowMotion){
			slowMoRate = int.Parse(slowMoRateString);
			enableUpdate = false;
			if (counter == slowMoRate){
				counter =0;
				enableUpdate = true;
			}

		}
		if(!slowMotion){
		enableUpdate = true;
		if(counter >= 64/(1f/Time.deltaTime*2)){
				counter =0;
			}
		}

		counter++;
	}

// ------------------------ Custom Functions----------------------
	
	void setVariables(){
		numNodes = int.Parse(numNodesString);
		simRunTime = int.Parse(simRunTimeString);
		showMainGui = false;
		nodesSqrt = (int)Mathf.Sqrt(numNodes);
	}

    void showMainMenu()
    {
        GUI.BeginGroup(new Rect(((Screen.width - buttonWidth) / 2) - 250, Screen.height / 2 - 250, 250, 400));
        GUI.Box(new Rect(0, 0, 250, 400), "UAV Simulator Options");
        GUILayout.BeginArea(new Rect(5, 30, buttonWidth, buttonHeight * numberButtons));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Simulation Runtime \n (0 for no limit)", GUILayout.Width(menuLabelWidth));
        simRunTimeString = GUILayout.TextField(simRunTimeString, GUILayout.Width(menuFieldWidth));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Number of Nodes", GUILayout.Width(menuLabelWidth));
        numNodesString = GUILayout.TextField(numNodesString, GUILayout.Width(menuFieldWidth));
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("Mobility Model");
        GUILayout.Space(5);
        GUILayout.Label("Network Model");
        GUILayout.EndArea();
        GUI.EndGroup();

        //display addition option drop down menus
        movementChoice = movementBehaviorLoader[flightComboBoxControl.Show()];
        networkChoice = networkBehaviorLoader[networkComboBoxControl.Show()];

        GUILayout.BeginArea(new Rect((Screen.width - buttonWidth) / 2, Screen.height / 2 + 250, buttonWidth, buttonHeight * numberButtons));
        if (GUILayout.Button("Load Simulation", GUILayout.Width(buttonWidth), GUILayout.Height(50)))
        {


            if (flightGUI)
            {
                paused = true;
                setVariables();
                flightGUI.setGuiValues();
                flightGUI.setFloor();
                if (networkGUI)
                    networkGUI.setGuiValues();
                gameObject.GetComponent<RTPopulationManager>().initializePopulation(movementChoice, networkChoice);
                flightGUI.setSpawnLocation();
            }
        }

        if (GUILayout.Button("Exit"))
        {
            Application.Quit();
        }
        GUILayout.EndArea();

        //show sub-options menu for specific componenets
        if (flightGUI)
            flightGUI.showGUI();

        if (networkGUI)
            networkGUI.showGUI();
    }

    void showRunningMenu()
    {
        //Left hand column options
        GUI.color = Color.green;
        GUI.backgroundColor = Color.blue;

        GUI.Box(new Rect(5, 5, buttonWidth + 10, buttonHeight * numberButtons + 30), "UAV Simulator Options");
        GUILayout.BeginArea(new Rect(10, 30, buttonWidth, buttonHeight * numberButtons));
        if (GUILayout.Button("Exit"))
        {
            Application.Quit();
        }
        GUILayout.Space(buttonHeight);
        if (GUILayout.Button("Reset Simulation"))
        {
            Application.LoadLevel("Simulation");
        }
        GUILayout.Space(buttonHeight);
        if (GUILayout.Button(pauseString))
        {
            if (!paused)
                pauseString = "Resume";
            if (paused)
                pauseString = "Pause Simulation";
            paused = !paused;
        }

        GUILayout.BeginHorizontal();
        slowMotion = GUILayout.Toggle(slowMotion, "Slow Sim", GUILayout.Width(80));
        if (GUILayout.Button("-"))
        {
            slowMoRateString = Mathf.Max(2, (--slowMoRate)).ToString();
        }
        slowMoRateString = GUILayout.TextField(slowMoRateString, GUILayout.Width(50));
        if (GUILayout.Button("+"))
        {
            slowMoRateString = (++slowMoRate).ToString();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void updateComponents()
    {
        // Attach/Remove the NetworkGUI Component to the Spawner
        if (networkChoice != "none" && networkChoice != "")
        {
            if (networkGUI == null)
            {
                lastNetComponentStr = networkChoice + "GUI";
                gameObject.AddComponent(lastNetComponentStr);
                networkGUI = (NetworkGUI)gameObject.GetComponent(lastNetComponentStr);
            }
        }
        else
        {
            if (networkGUI != null)
                Destroy(networkGUI);
        }
        // Attach/Remove the FlightkGUI Component to the Spawner
        if (movementChoice != defaultMoveString && movementChoice != "")
        {
            if (flightGUI == null)
            {
                lastMoveComponentStr = movementChoice + "GUI";
                gameObject.AddComponent(lastMoveComponentStr);
                flightGUI = (FlightGUI)gameObject.GetComponent(lastMoveComponentStr);
            }
        }
        else
        {
            if (flightGUI != null)
                Destroy(flightGUI);
        }
    }
}
//code to generate Combo box.  Taking off Unity Forums 
//Todo: add credit to creator.
public class ComboBox
{
	private static bool forceToUnShow = false; 
	private static int useControlID = -1;
	private bool isClickedComboButton = false;
	private int selectedItemIndex = 0;
	
	private Rect rect;
	private GUIContent buttonContent;
	private GUIContent[] listContent;
	private string buttonStyle;
	private string boxStyle;
	private GUIStyle listStyle;
	
	public ComboBox( Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle ){
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.buttonStyle = "button";
		this.boxStyle = "box";
		this.listStyle = listStyle;
	}
	
	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, GUIStyle listStyle){
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.buttonStyle = buttonStyle;
		this.boxStyle = boxStyle;
		this.listStyle = listStyle;
	}
	
	public int Show()
	{
		if( forceToUnShow )
		{
			forceToUnShow = false;
			isClickedComboButton = false;
		}
		
		bool done = false;
		int controlID = GUIUtility.GetControlID( FocusType.Passive );       
		
		switch( Event.current.GetTypeForControl(controlID) )
		{
		case EventType.mouseUp:
		{
			if( isClickedComboButton )
			{
				done = true;
			}
		}
			break;
		}       
		
		if( GUI.Button( rect, buttonContent, buttonStyle ) )
		{
			if( useControlID == -1 )
			{
				useControlID = controlID;
				isClickedComboButton = false;
			}
			
			if( useControlID != controlID )
			{
				forceToUnShow = true;
				useControlID = controlID;
			}
			isClickedComboButton = true;
		}
		
		if( isClickedComboButton )
		{
			Rect listRect = new Rect( rect.x+140, rect.y + listStyle.CalcHeight(listContent[0], 1.0f),
			                         rect.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length );
			
			GUI.Box( listRect, "", boxStyle );
			int newSelectedItemIndex = GUI.SelectionGrid( listRect, selectedItemIndex, listContent, 1, listStyle );
			if( newSelectedItemIndex != selectedItemIndex )
			{
				selectedItemIndex = newSelectedItemIndex;
				buttonContent = listContent[selectedItemIndex];
			}
		}
		
		if( done )
			isClickedComboButton = false;
		
		return selectedItemIndex;
	}
	
	public int SelectedItemIndex{
		get{
			return selectedItemIndex;
		}
		set{
			selectedItemIndex = value;
		}
	}
}
