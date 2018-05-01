using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cfxNotificationSMBAgent : StateMachineBehaviour 
{
    public string theNotificationName = null;
    public bool verbose = false;
	[HideInInspector] public string uuid;
	[HideInInspector] public cfxNotificationManager theNotificationManager = null;

    public void connectToNotificationManager()
    {
        if (theNotificationManager == null)
        {
            GameObject notificationMaster = GameObject.Find("cfxNotificationManager");
            theNotificationManager = notificationMaster.GetComponent<cfxNotificationManager>();
            if (theNotificationManager == null)
            {
                Debug.Log("Could not connect to notification Manager. Please ensure that the cfxNotificationManger is placed somewhere in your scene.");
            }
            else
            {
                if (verbose) Debug.Log("cfxNotificationManager: connected.");
				uuid = theNotificationManager.uuidSIP ();
            }
        }
        else
        {
            if (verbose) Debug.Log("cfxNotificationManager: using pre-established connection");
			if (uuid.Length < 1)
				uuid = theNotificationManager.uuidSIP ();
        }
    }

    public Dictionary<string, object> getBasicInfo(string theEvent)
    {
        Dictionary<string, object> theDict = new Dictionary<string, object>();
        addBasicInformation(theDict);
        if (theEvent != null) theDict["Event"] = theEvent;
        return theDict;
    }

    public void addBasicInformation(Dictionary<string, object> theDict)
    {
        // note that by default, there is no gameObject attached. It's accessible if we have an Animator
        // in some calls, though
        theDict["Time"] = Time.time.ToString();
        if (!theDict.ContainsKey("Event")) theDict["Event"] = "cfxGenericEvent";

    }
    public void post(string theNotificationName, Dictionary<string, object> theDict)
    {
		// Resilience:
		// if we are not connected, try once to re-connect. The original cNM may have gone
		// off-line or been switched, so try to re-establish a connection
		if (theNotificationManager == null) connectToNotificationManager ();


        if (theNotificationManager == null) return;

        if (theDict == null) theDict = new Dictionary<string, object>();

        if ((theNotificationName == null) || (theNotificationName.Length < 1))
        {
            theNotificationName = "cfxGenericNotification";
            if (verbose) Debug.Log("notification name set to" + theNotificationName);
        }

        if (verbose)
        {
            Debug.Log("Posting Notification with info dict containing:");
            foreach (string theKey in theDict.Keys)
            {
                Debug.Log(" --> " + theKey + " : " + theDict[theKey].ToString());
            }
            Debug.Log(" --> NotificationName : " + theNotificationName);
        }


        theNotificationManager.postNotification(theNotificationName, theDict);

    }

    public virtual void Awake()
    {
        connectToNotificationManager();
    }
}
