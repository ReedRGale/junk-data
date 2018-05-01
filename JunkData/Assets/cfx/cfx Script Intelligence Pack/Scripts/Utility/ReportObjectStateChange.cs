using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ReportObjectStateChange:
 * A cfxNotificationManager UtilityScript
 * 
 * Copyright (C) 2017 by cf/x AG and Christian Franz
 * 
 * This script will broadcast a notification whenever the GameObject's state changes as follows:
 * - Destroy: is about to be destroyed
 * - Enable: has just become enabled
 * - Disable: has just become disabled
 * 
 * When subscribing to this notification, the following fields are defined in the information dictionary 
 * - GameObject: the GameObject that is reporting the change (eitehr root or parent of the prefab)
 * - Time: Time (as a string) when this happened
 * - Module: "ReportObjectStateChange"
 * - Event: "Destroy" / "Enable" / "Disable"
 * - NotificationName: content of <theNotificationName> or "cfxGenericNotification" when empty
 * 
 */

public class ReportObjectStateChange : cfxNotificationAgent {

    public bool reportDestroy = true;
    public bool reportEnable = true;
    public bool reportDisable = true;


    private string theModule = "ReportObjectStateChange";


    public override void OnDestroy()
    {
        if (reportDestroy) {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Destroy" );
			// we must send this out immediately, override any delay
			delayBeforePosting = 0;
			// terminate all pending notifications
			StopAllCoroutines ();

            this.post(theDict);
        }
		base.OnDestroy ();
    }

    private void OnDisable()
    {
        if (reportDisable)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Disable");
            this.post(theDict);
        }
    }

    private void OnEnable()
    {
        if (reportEnable)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Enable");
            this.post(theDict);
        }
    }

}
