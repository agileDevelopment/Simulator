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

    void setSpawnLocation(GameObject singleNode);
	
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
//  Class Dependicies: Node Controller

//  Description:  Implements C.E. Perkins AODV dynamic network protocol.
//
//--------------------------------------------------------------
public interface INetworkBehavior
{

    void addNeighbor(GameObject node);
    void removeNeighbor(GameObject node);
    void recMessage(MSGPacket packet);
    void sendMessage(MSGPacket packet);
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

//  Description:  Implements Gui Options for C.E. Perkins AODV dynamic network protocol.
//
//--------------------------------------------------------------
public interface INetworkGUIOptions{

    void setGuiValues();
    void showGUI();
	
}

public interface IMovementManager
{
    Vector3 getDestination(ArrayList inputs);
}