using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ReportObjectCollision:
 * A cfxNotificationManager UtilityScript
 * 
 * Copyright (C) 2017 by cf/x AG and Christian Franz
 * 
 * This script will broadcast a notification whenever the GameObject reports a collision with it's collider:
 * 
 * When subscribing to this notification, the following fields are defined in the information dictionary 
 * - GameObject: the GameObject that is reporting the change (eitehr root or parent of the prefab)
 * - Time: Time (as a string) when this happened
 * - Module: "ReportObjectCollision"
 * - Event: "Enter" / "Exit" / "Stay"
 * - Collision: the collision information  
 * - NotificationName: content of <theNotificationName> or "cfxGenericNotification" when empty
 * 
 */
public class ReportObjectCollision : cfxNotificationAgent {

    public bool reportEnter = true;
    public bool reportStay = true;
    public bool reportExit = true;

    private string theModule = "ReportObjectCollision";

    // NOTE: ifyou override AWAKE make sure to call inherited as well!

    // Use this for initialization
	public override void Start() {
		// call SIP's Start()
		base.Start ();

	}

    private void OnCollisionEnter(Collision collision)
    {
        if (reportEnter) {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Enter");
            theDict.Add("Collision", collision);
            // add any more information here
            this.post(theDict);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (reportStay)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Stay");
            theDict.Add("Collision", collision);
            // add any more information here
            this.post(theDict);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (reportExit)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Exit");
            theDict.Add("Collision", collision);
            // add any more information here
            this.post(theDict);
        }
    }
}
