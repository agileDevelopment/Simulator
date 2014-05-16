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
//  Description:  Defines what options for the simulator to use
//--------------------------------------------------------------

using UnityEngine;

public class LoadOptionsGUI : MonoBehaviour {
	
	GUIContent[] flightControllerList;
	private ComboBox flightComboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();
	IFlightGUIOptions flightGUI;
	public Renderer cubeRender;	
	public int buttonHeight = 50;
	public int buttonWidth = 200;
	public int buttonSpace = 100;	// Use this for initialization
	public int numberButtons;
	public string numNodesString;
	public int numNodes;
	public string simRunTimeString="0";
	public int simRunTime;
	public string nodeCommRangeString="100";
	public int nodeCommRange;
	public string drawLinesString = "Hide Lines";
	public bool drawLine=true;
	public string pauseString = "Pause Simulation";
	public bool paused=false;
	public bool showMainGui;
    public bool adaptiveNetworkColor = false;
	public int flightChoice=0;
	public int menuLabelWidth = 170;
	public int menuFieldWidth = 100;
	public int nodesSqrt;
	public bool enableUpdate;
	public bool updateLines  =true;
	public bool slowMotion = false;
	public string slowMoRateString;
	public int slowMoRate;
	int counter = 1;
	
	void Start () {
		numberButtons=5;
		numNodesString="25";
		slowMoRateString = "2";
	showMainGui = true;
		//List for types of Flight Controllers
		//Must be updated when new FlightBehaviors are implemented
		flightControllerList = new GUIContent[4];
		flightControllerList[0] = new GUIContent("Grid");
		flightControllerList[1] = new GUIContent("Orbit");
		flightControllerList[2] = new GUIContent("Random");
		flightControllerList[3] = new GUIContent("Path");
		listStyle.normal.textColor = Color.white; 
		listStyle.onHover.background =
		listStyle.hover.background = new Texture2D(2, 2);
		//set all padding to 4
		listStyle.padding.left =
			listStyle.padding.right =
				listStyle.padding.top =
					listStyle.padding.bottom = 4;
		flightComboBoxControl = new ComboBox(new Rect(5, buttonHeight*numberButtons+10, 240, 20),
			 flightControllerList[0], flightControllerList, "button", "box", listStyle);

	}
	
	void OnGUI () {
		//main interface to be shown when program first runs;
		if(showMainGui){
			GUI.BeginGroup(new Rect(((Screen.width- buttonWidth)/2)-250 , Screen.height/2-250, 250, 400));
			GUI.Box(new Rect(0, 0, 250, 400), "UAV Simulator Options");
			GUILayout.BeginArea(new Rect(5 , 30, buttonWidth,buttonHeight*numberButtons));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Simulation Runtime \n (0 for no limit)", GUILayout.Width(menuLabelWidth));
			simRunTimeString = GUILayout.TextField(simRunTimeString, GUILayout.Width(menuFieldWidth));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Number of Nodes", GUILayout.Width(menuLabelWidth));
			numNodesString = GUILayout.TextField(numNodesString, GUILayout.Width(menuFieldWidth));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Node Comm Range", GUILayout.Width(menuLabelWidth));
			nodeCommRangeString = GUILayout.TextField(nodeCommRangeString, GUILayout.Width(menuFieldWidth));
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			flightChoice = flightComboBoxControl.Show();
			GUI.EndGroup();


			// Make a background box			

			
			//Exit button.
			GUILayout.BeginArea(new Rect((Screen.width- buttonWidth)/2 , Screen.height/2+250, buttonWidth,buttonHeight*numberButtons));
			if(GUILayout.Button("Load Simulation",GUILayout.Width(buttonWidth),GUILayout.Height(50))){
			paused = true;
				setVariables();
				gameObject.GetComponent<Spawner>().StartSimulation(flightChoice);		
			}
			
			if(GUILayout.Button("Exit")){
				Application.Quit();
			}
			GUILayout.EndArea();
			switch(flightChoice){
					case 0:
				gameObject.GetComponent<GridGUI>().showGUI();
						break;
					case 1:
				gameObject.GetComponent<OrbitGUI>().showGUI();
						break;
					default:
					break;
				}
		}
		//show this menu while simulation is running
		if(!showMainGui){
		
			//Left hand column options
            GUI.color = Color.green;
            GUI.backgroundColor = Color.blue;

            GUI.Box(new Rect(5, 5, buttonWidth + 10, buttonHeight * numberButtons+30), "UAV Simulator Options");
			GUILayout.BeginArea(new Rect(10,30,buttonWidth, buttonHeight*numberButtons));
						if(GUILayout.Button("Exit")){
							Application.Quit();
							}
					GUILayout.Space(buttonHeight);
					if(GUILayout.Button("Reset Simulation")){
						Application.LoadLevel("Simulation");
						}
					GUILayout.Space(buttonHeight);
					if(GUILayout.Button(drawLinesString)){
                        if (drawLine)
                        {
                            drawLinesString = "Show Lines";
                        }
                        else drawLinesString = "Hide Lines";
							drawLine = !drawLine;
						}
                          if (drawLine)
                        adaptiveNetworkColor = GUILayout.Toggle(adaptiveNetworkColor, "Adaptive Color");
						if(GUILayout.Button(pauseString)){
						if(!paused)
						pauseString = "Resume";
						if(paused)
						pauseString = "Pause Simulation";
						paused = !paused;
						}

			GUILayout.BeginHorizontal();
			slowMotion = GUILayout.Toggle(slowMotion, "Slow Sim", GUILayout.Width(80));
			if(GUILayout.Button("-")){
			slowMoRateString = Mathf.Max(2,(--slowMoRate)).ToString();
			}
			slowMoRateString = GUILayout.TextField(slowMoRateString,GUILayout.Width(50));
			if(GUILayout.Button("+")){
				slowMoRateString = (++slowMoRate).ToString();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			
			//Top Speed Controls...
			GUILayout.BeginArea(new Rect((Screen.width- buttonWidth)/2 - 200,10, 400,30));

			GUILayout.EndArea();
			
		}
		

	}
	
	void Update(){
		if(slowMotion){
			slowMoRate = int.Parse(slowMoRateString);
			enableUpdate = false;
			if (counter == slowMoRate){
				counter =0;
				enableUpdate = true;
				updateLines = true;
			}

		}
		if(!slowMotion){
		enableUpdate = true;
			updateLines = false;
		if(counter >= 64/(1f/Time.deltaTime*2)){
				counter =0;
				updateLines = true;
			}
		}
		

		counter++;
	}
	
	void setVariables(){
		numNodes = int.Parse(numNodesString);
		nodeCommRange = int.Parse(nodeCommRangeString);
		simRunTime = int.Parse(simRunTimeString);
		showMainGui = false;
		nodesSqrt = (int)Mathf.Sqrt(numNodes);
		
		switch(flightChoice){
		case 0:
			gameObject.GetComponent<GridGUI>().setGuiValues();
			break;
		case 1:
			gameObject.GetComponent<OrbitGUI>().setGuiValues();
			break;
		default:
			break;
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
			Rect listRect = new Rect( rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1.0f),
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
