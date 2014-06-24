using UnityEngine;
using System.Collections;

public class FlightGUI : MonoBehaviour, IFlightGUIOptions {
    public float floorSize = 0;
    public float center = 0;
    public Camera mainCamera;
    public Camera camera2;
 //   bool mainActive = true;

    protected virtual void Update()
    {
    }



    public virtual void setSpawnLocation()
    {

    }

    public virtual void setSpawnLocation(GameObject singleNode)
    {

    }

   public virtual void showGUI()
    {

    }

   public virtual void setGuiValues()
   {
       mainCamera = Camera.main;
       camera2 = (Camera)GameObject.Find("Second Camera").camera;
   }

   public virtual void setFloor()
   {

   }
	

}
