//------------------------------------------------------------
//  Title: Interfaces.cs
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: None
//
//  Description:  Container File for the various interfaces that each node attribute must implement.
//
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;
//------------------------------------------------------------
//  Title: IFlightBehavior
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: NodeController
//
//  Description:  Interface for FlightBehaviors.  Defines must implement functions...
//
//--------------------------------------------------------------
public interface IFlightBehavior{

	void updateLocation();
	
	//void showGUI();
	
	//void setGuiValues();
	

	}
//------------------------------------------------------------
//  Title: IFlightGUIOptions
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: Spawner
//
//  Description:  Defines what methods must be implemented to interface with the main program GUI.
//
//--------------------------------------------------------------
public interface IFlightGUIOptions{
	
	void setSpawnLocation();
	
	void showGUI();
	
	void setGuiValues();
	
	void setFloor();
	
	
}

//------------------------------------------------------------
//  Title: INetworkBehavior
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: Spawner

//  Description:  Defines what methods must be implemented to interface with the main program GUI.
//
//--------------------------------------------------------------
public interface INetworkBehavior{
	
//todo
	
}
//------------------------------------------------------------
//  Title: INetworkGUIOptions
//  Date: 5-12-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Corey Willinger
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: Spawner

//  Description:  Defines what methods must be implemented to interface with the main program GUI.
//
//--------------------------------------------------------------
public interface INetworkGUIOptions{
	
//todo

//adding a test of git commits and changes....
//just modifying some comments.
	
}