using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Copyright (C) 2017, 2018 by cf/x AG and Christian Franz
 * 
 * This is the Notifification Agent root class
 * that all cfx Notification Manager and Query Manager utility scripts 
 * derive from. 
 *
 * It automatically connects to the cfxNotificationManager during Awake, so
 * if you override Awake, make sure you call the inherited (base) Awake, or call
 * connectToNotificationManger yourself.
 *
 */

public class cfxNotificationAgent : cfxIntegratedQuery {

    public string theNotificationName; // if you leave this empty, it defaults to "cfxGenericNotification"
    public float delayBeforePosting = 0f;

    public Dictionary<string, object> getBasicInfo(string reportingModule, string theEvent) {
        Dictionary<string, object> theDict = new Dictionary<string, object>();
        theDict.Add("GameObject", gameObject);
        theDict.Add("Time", Time.time.ToString());
        theDict.Add("Module", reportingModule);
        theDict.Add("Event", theEvent);

        return theDict;
    }

	//
	// provide a convenient overload to post a single Dict for more code 
	// cleanlyness in all the agent utilities.
	// This is but a pale shadow to the IntegratedReceiver's amenities,
	// so you are usually better of inheriting from there.
	// We use this because we know we don't need filtering
	//
    public void post(Dictionary<string, object> theDict) {
		// if we are not connected, try once to re-connect. The cNM may have gone
		// off-line or been switched, so try to re-establish a connection
		if (theNotificationManager == null) connectToNotificationManager ();

        if (theNotificationManager == null) return;

        if (theDict == null) theDict = new Dictionary<string, object>();

        if ((theNotificationName == null) || (theNotificationName.Length < 1)) {
            theNotificationName = "cfxGenericNotification";
            if (verbose) Debug.Log("notification name set to" + theNotificationName);
        }

        if (verbose) {
            Debug.Log("Posting Notification with info dict containing:");
            foreach (string theKey in theDict.Keys) {
                Debug.Log(" --> " + theKey + " : " + theDict[theKey].ToString());
            }
            Debug.Log(" --> NotificationName : " + theNotificationName);
        }

        if (delayBeforePosting <= 0.0001)
        {
            theNotificationManager.postNotification(theNotificationName, theDict);
        }
        else {
            theNotificationManager.postDelayedNotification(theNotificationName, theDict, delayBeforePosting);
        }
    }

	public override void Awake()
    {
        connectToNotificationManager();
    }
}
