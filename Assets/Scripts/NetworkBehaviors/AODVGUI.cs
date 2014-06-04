using UnityEngine;
using System.Collections;

public class AODVGUI : NetworkGUI {

    int floorsize;


	// Use this for initialization
    protected override void Start()
    {
        base.Start();
        myUIElements.Add("messageQueue","");
        baseStation = GameObject.Find("Sat");
 	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (upLink)
        {
            upLink.GetComponent<LineRenderer>().SetWidth(.5f, 1);
            upLink.GetComponent<LineRenderer>().SetPosition(0, baseStation.transform.position);
            upLink.GetComponent<LineRenderer>().SetPosition(1, supervisor.transform.position);
            upLink.GetComponent<LineRenderer>().SetColors(Color.blue, Color.magenta);
        }

	}
    public override void setGuiValues()
    {
        base.setGuiValues();
        
    }

}

