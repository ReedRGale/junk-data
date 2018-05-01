using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * Copyright (C) 2017, 2018 by cf/x AG and Christian Franz
 * 
 * This is the Notifification Sender root class, an intermediade class
 * that you can use to gain immediate access to connecting to the 
 * notification manager and convenience functions to post notifications.
 * If your script should also be able to receive notifications, use
 * the standard "IntegratedReceiver" class, that inherits from this class.
 * 
 * Important Methods
 *  - getBasicInfo creates a dictionary with an "Event"
 *  - addBasicInformation adds standard GameObject, Time entries, and validates "Event"
 *  
 *  Convenience Pack:
 *  sendNotification: fully overloaded notification send
 *  postNotification: fully overloaded notification send (alternative name)
 * 
 * Scheduling:
 * This class implements local scheduling in addition to central scheduling on NM
 *
 */

public class cfxNetworkedNotificationSender : cfxNetworkedNotificationBehaviour {

    public Dictionary<string, object> getBasicInfo(string theEvent)
    {
		if (this == null) {
			Debug.Log ("Warning: post with NULL script instance. The parenting object was destroyed!");
			return null;
		}

        Dictionary<string, object> theDict = new Dictionary<string, object>();
        addBasicInformation(theDict);
        if (theEvent != null) theDict["Event"] = theEvent;
        return theDict;
    }

	//
	// addBasicInformation ensures minimum information is added to the Notification
	//
    public void addBasicInformation(Dictionary<string, object> theDict) {
		if (this == null) {
			Debug.Log ("Warning: post with NULL script instance. The parenting object was destroyed!");
			return;
		}

        theDict["GameObject"] = gameObject;
        theDict["Time"] = Time.time.ToString();
        if (!theDict.ContainsKey("Event")) 
			theDict["Event"] = "cfxGenericEvent";

    }

	//
	// post actually sends out the notification, and is the endpoint for all
	// sendXXX and postXXX convenience methods
	//
	private void post(string theNotificationName, Dictionary<string, object> theDict, float delay)
    {
		// sanity: are we a zombie instance?
		if (this == null) {
			Debug.Log ("Warning: post with NULL script instance. The parenting object was destroyed!");
			return;
		}

		// Resilience:
		// if we are not connected, try once to re-connect. The original cNM may have gone
		// off-line or been switched, so try to re-establish a connection
		if (theNotificationManager == null) connectToNotificationManager ();

		// sanity check
        if (theNotificationManager == null) return;

		// validation of theDict
        if (theDict == null) theDict = new Dictionary<string, object>();

		// validation of notificationName
        if ((theNotificationName == null) || (theNotificationName.Length < 1))  {
            theNotificationName = "cfxGenericNotification";
            if (verbose) Debug.Log("notification name set to" + theNotificationName);
        }

		// ensure that theDict has correct notificationName and minimal data
		theDict ["NotificationName"] = theNotificationName;
		theDict ["GameObject"] = gameObject;
		theDict["Time"] = Time.time.ToString();

		// reporting time if enabled: dump info dict. Great for debugging
        if (verbose) {
            Debug.Log("Posting Notification with info dict containing:");
            foreach (string theKey in theDict.Keys)  {
                Debug.Log(" --> " + theKey + " : " + theDict[theKey].ToString());
            }
            Debug.Log(" --> NotificationName : " + theNotificationName);
        }
			

		// handle delay explicitly, make too short delay to execute immediately instead of deferred
		if (delay < 0.01) {
			theNotificationManager.postNotification (theNotificationName, theDict);
		} else {
			theDict["Scheduled"] = "Central"; // Mark this as a sheduled central notification
			theNotificationManager.postDelayedNotification (theNotificationName, theDict, delay);
		}

    }


   /*
    * Amenities Section
    * various versions of postNotification and sendNotification (for those who 
    * can't remember if it's post or send) with different parameter sets that
    * auto-complete the missing information.
    * 
    * SIP provides a full orthogonal set of overloaded methods for
    *  - normal
    *  - scheduled
    *  - locally/centrally scheduling
    * notifications
    * 
    * Make sure not to break the chain if you extend.
    * 
    */

    public void postNotification(string theNotificationname) {
        Dictionary<string, object> theDict = getBasicInfo(theNotificationname);
        post(theNotificationname, theDict, 0f);
    }

    public void sendNotification(string theNotificationName) {
        postNotification(theNotificationName);
    }

    public void postNotification(string theNotificationName, Dictionary<string, object> theDict)
    {
        addBasicInformation(theDict);
        post(theNotificationName, theDict, 0f);
    }

    public void sendNotification(string theNotificationName, Dictionary<string, object> theDict) {
        postNotification(theNotificationName, theDict);
    }

    public void postNotification(string theNotificationName, string theEvent) {
        Dictionary<string, object> theDict = getBasicInfo(theEvent);
        postNotification(theNotificationName, theDict);
    }

    public void sendNotification(string theNotificationName, string theEvent) {
        Dictionary<string, object> theDict = getBasicInfo(theEvent);
        sendNotification(theNotificationName, theDict);
    }

    public void postNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict)
    {
        if (theEvent != null) theDict["Event"] = theEvent;
        postNotification(theNotificationName, theDict);
    }

    public void sendNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict)
    {
        if (theEvent != null) theDict["Event"] = theEvent;
        sendNotification(theNotificationName, theDict);
    }

	//
	// The full set with notification delay
	//
	public void postNotification(string theNotificationname, float delay) {
		Dictionary<string, object> theDict = getBasicInfo(theNotificationname);
		post(theNotificationname, theDict, delay);
	}

	public void sendNotification(string theNotificationName, float delay) {
		postNotification(theNotificationName, delay);
	}

	public void postNotification(string theNotificationName, Dictionary<string, object> theDict, float delay)
	{
		addBasicInformation(theDict);
		post(theNotificationName, theDict, delay);
	}

	public void sendNotification(string theNotificationName, Dictionary<string, object> theDict, float delay) {
		postNotification(theNotificationName, theDict, delay);
	}

	public void postNotification(string theNotificationName, string theEvent, float delay) {
		Dictionary<string, object> theDict = getBasicInfo(theEvent);
		postNotification(theNotificationName, theDict, delay);
	}

	public void sendNotification(string theNotificationName, string theEvent, float delay) {
		Dictionary<string, object> theDict = getBasicInfo(theEvent);
		sendNotification(theNotificationName, theDict, delay);
	}

	public void postNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict, float delay)
	{
		if (theEvent != null) theDict["Event"] = theEvent;
		postNotification(theNotificationName, theDict, delay);
	}

	public void sendNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict, float delay)
	{
		if (theEvent != null) theDict["Event"] = theEvent;
		sendNotification(theNotificationName, theDict, delay);
	}

    //
    // above delayed full shebang, but localy sheduled
    //

    public void postNotification(string theNotificationName, float delay, bool local)
    {
        Dictionary<string, object> theDict = getBasicInfo(theNotificationName);
        if (local) {
            postLocallyDelayedNotification(theNotificationName, theDict, delay);
        } else {
            post(theNotificationName, theDict, delay);
        }
       
    }

    public void sendNotification(string theNotificationName, float delay, bool local)
    {
        postNotification(theNotificationName, delay, local);
    }

    public void postNotification(string theNotificationName, Dictionary<string, object> theDict, float delay, bool local)
    {
        addBasicInformation(theDict);
        if (local)       {
            postLocallyDelayedNotification(theNotificationName, theDict, delay);
        }         else         {
            post(theNotificationName, theDict, delay);
        }
    }

    public void sendNotification(string theNotificationName, Dictionary<string, object> theDict, float delay, bool local)
    {
        postNotification(theNotificationName, theDict, delay, local);
    }

    public void postNotification(string theNotificationName, string theEvent, float delay, bool local)
    {
        Dictionary<string, object> theDict = getBasicInfo(theEvent);
        postNotification(theNotificationName, theDict, delay, local);
    }

    public void sendNotification(string theNotificationName, string theEvent, float delay, bool local)
    {
        Dictionary<string, object> theDict = getBasicInfo(theEvent);
        sendNotification(theNotificationName, theDict, delay, local);
    }

    public void postNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict, float delay, bool local)
    {
        if (theEvent != null) theDict["Event"] = theEvent;
        postNotification(theNotificationName, theDict, delay, local);
    }

    public void sendNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict, float delay, bool local)
    {
        if (theEvent != null) theDict["Event"] = theEvent;
        sendNotification(theNotificationName, theDict, delay, local);
    }

    //
    // GameObject case
    // This will alway have TheEvent filled with "cfxGenericNotification"
    //
    public void sendNotification(string theNotificationName, GameObject theObject) {
        sendNotification(theNotificationName, theObject, 0f);
    }

    public void postNotification(string theNotificationName, GameObject theObject) {
        sendNotification(theNotificationName, theObject);
    }

    public void sendNotification(string theNotificationName, GameObject theObject, float delay) {
        sendNotification(theNotificationName, theObject, delay, false);
    }

    public void postNotification(string theNotificationName, GameObject theObject, float delay) {
        sendNotification(theNotificationName, theObject, delay);
    }

    public void sendNotification(string theNotificationName, GameObject theObject, float delay, bool local)
    {
        Dictionary<string, object> theDict = new Dictionary<string, object>();
		if (theObject != null) theDict["TheObject"] = theObject;
        sendNotification(theNotificationName, theDict, delay, local);

    }

    public void postNotification(string theNotificationName, GameObject theObject, float delay, bool local) {
        sendNotification(theNotificationName, theObject, delay, local);
    }


    /*
	 * Local scheduling. Can be stopped with 
	 * 
	 *   StopAllCoroutines()
	 * 
	 * Notice that scheduling via co-routines requires that we create a local
	 * instance for each notification, which is why we need to resort to the
	 * set of private variables to pass the details
	 * 
	 */

    private float passDelay;
    private Dictionary<string, object> passForDelay;

    public void postLocallyDelayedNotification(string name, Dictionary<string, object> info, float delay)
    {
        if (delay < 0.01)
        {
            // anything less than 1/100s delay is executed immedately 
            postNotification(name, info);
            return;
        }

        // invoke coroutine
        // yield for number of seconds.
        // done. Coroutines are reentrant, as
        // they create separate instances for each invocation
        passDelay = delay;
        info["NotificationName"] = name;
        passForDelay = info;
        StartCoroutine("DelayAndPostViaLocalCoroutine");
    }


    IEnumerator DelayAndPostViaLocalCoroutine()
    {
        // immediately fetch the info bit, and create a copy
        Dictionary<string, object> info = new Dictionary<string, object>(passForDelay); // create a copy for this instance of coroutine
        yield return new WaitForSeconds(passDelay);
        // we'll continue here after delay, in our own instance, without blocking

        // recover notificationName
        string notificationName = info["NotificationName"] as string;
		info ["Scheduled"] = "Local"; // mark this as a scheduled local notification
        postNotification(notificationName, info);
    }

    //
    // on awake we connect to the NM. Note that the NM may itself
    // not be awake at this time, so we will also try to connect the
    // first time we post a notification
    //
	// AMENDED:
	// experience has showsn that it is generally not a good idea to try to 
	// connect to the notification manager during awake.
	// 
	// Re-enable below code only if really absolutely MUST try and connect to 
	// to NM during awake, remember that we always have a fallback during sendNotification.
	//

	/*
    public override void Awake()
    {
        connectToNotificationManager();
    }
    */
}
