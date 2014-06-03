using UnityEngine;
using System.Collections;

public class AODVGUI : NetworkGUI {

	// Use this for initialization
    protected override void Start()
    {
        base.Start();
        myUIElements.Add("messageQueue","");
 	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}

}

