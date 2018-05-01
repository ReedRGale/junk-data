using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * Copyright (C) 2017, 2018 by cf/x AG and Christian Franz
 * 
 * This is the Notofification Integrated Receiver root class
 * that you can use to gain immediate access to connecting to the 
 * notification manager 
 * 
 * It has the ability to both send and receive notifications using
 * convenience functions for sending as well as receiving.
 * 
 * Use this class if you want a central OnNotification routine for all your
 * Notifications that can pre-filter events.
 * 
 * To use this class, do two things:
 * 
 * 1. implement a method 
 *     private override void OnNotification(string notificationName, Dictionary<string, object> info)
 *    
 *    if you do not provide such a method yourself, the console will alert you to this
 *    but the script won't crash
 *    
 * 2. Subscribe to the notifications you are interested in with subscribeTo and the
 *    name of the notification/event
 *
 * 3. (Optional)
 *    If you are only interested in certain subtypes of a notification, you can
 *    elect to ignore them.
 *    NOTE THAT THIS WORKS ONLY WITH NOTIFOCATIONS THAT HAVE A 'Event' KEY 
 *    All cfxNotification senders usually have that keyword, but third-party senders
 *    might omit it. In those cases, they will not be filtered but passed on
 * 
 * 4. (Optional) post/send notifications to other scripts
 *    
 * To implement notification hadling separately yourself for each notification, use
 * the cfxNotificationReceiver class on which this class is built (i.e. you can also
 * access that class's functionality from with this class
 * 
 * This script automatically connects to the cfxNotificationManager during Awake, so
 * if you override Awake, make sure you call the inherited Awake, or call
 * connectToNotificationManger yourself.
 *
 */

public class cfxNetworkedNotificationIntegratedReceiver : cfxNetworkedIntegratedQuery {

    public enum cfxFilterMethod
    {
        BlackList,
        WhiteList
    }

    private List<string> subscribedNames = null;
    private Dictionary<string, List<string>> blackList = null; // also doubles as whitelist if filtering is set to whiteList

    public cfxFilterMethod filterUsing = cfxFilterMethod.BlackList;

	/*
	 * Resetting the SIP object - we need to forget all the notifications we subscribe to
	 * else we won't subscribe due to optimizations
	 * 
	 */

	public override void resetSIPObject() {
		subscribedNames = null; // forget all previous subscripion. Seting to null will re-init with next subscription
		base.resetSIPObject ();
	}


    /*
     * HERE COMES THE ONXXX BLOCK. IT'S CURRENTLY THREADED BOTTOM-UP. 
     * YOUR OWN IMPEMENTATION WILL *CORRECTLY* BREAK THE CHAIN OF CALLING THE 
     * NEXT HIGHER. YOU MUST NEVER CALL BASE.ONXXXX
     * 
     */

    /*
     * MAKE SURE YOU OVERRIDE ONE AND ONLY ONE OF THE FOLLOWING IN YOUR CODE BY DEFINING YOUR OWN.
     * If not, the first method directly below will be called and produce a warning in the log
     */
    private Dictionary<string, object> thePassedInfo = null; // used to pass grantaccess to theInfo between calls

    public virtual void OnNotification(string notificationName) {
        Debug.Log("Notification <" + notificationName + "> received but ignored:");
        Debug.Log("Your script needs a method 'private override void OnNotification(string notificationName, Dictionary<string, object> info)'");
        Debug.Log("Make sure not to forget the OVERRIDE keyword as shown above");

    }

    public virtual void OnNotification(string notificationName, GameObject theObject) {
        OnNotification(notificationName);
    }

    public virtual void OnNotification(string notificationName, string theEvent) {
        GameObject theGameObject = null;
        if (thePassedInfo.ContainsKey("TheObject")) theGameObject = thePassedInfo["TheObject"] as GameObject;
        OnNotification(notificationName, theGameObject);
    }

    public virtual void OnNotification(string notificationName, Dictionary<string, object> info)
    {
        string theEvent = null;
        if (info.ContainsKey("Event")) theEvent = info["Event"] as string;
        OnNotification(notificationName, theEvent);
    }


    /*
     * Below is the first entry point in chain of OnXXX
     */

    public virtual void OnNotification(string notificationName, string theEvent, Dictionary<string, object> info) {
        // save info for access while walking up the chain
        thePassedInfo = info;
        OnNotification(notificationName, info);
    }

    /*
     * This class works by simply routing all notifications to the integratedReceiver, 
     * and then calls OnNotification
     * 
     * You extend this class by filtering the integratedReceiver events, possibly enriching
     * the information.
     *
     */

    //
    // the subscribeTo message adds the notificationName to the list of filterable
    // subscriptions, and directs it to the integratedReceiver handler that eventually
    // call the OnNotification chain
    //
    public void subscribeTo(string notificationName) {
        if (subscribedNames == null) subscribedNames = new List<string>(); // lazy init;

        // avoid subscribing twice for the same name
        if (subscribedNames.Contains(notificationName)) {
            if (verbose) 
                Debug.Log("You are already subscribed to " + notificationName + ". No problem.");
            
            return;
        }
        subscribedNames.Add(notificationName);
        subscribeTo(notificationName, integratedReceiver);
    }

    //
    // calling subscribeTo with your handler will pass it to the notificationhandler
    // but not add it to the filterable events. Instead, your own handler will be called.
    //
    public void subscribeTo(string notificationName, cfxNotificationManager.notificationHandler theHandler)
    {
		// resilience: try to re-connect if no connection.
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null) {
			if (verbose) {
				Debug.Log("Not connected to Notification Manager. Subscription not registered.");
			}
			return;
		}

		theNotificationManager.subscribeTo(notificationName, theHandler, uuid);
    }

	//
	// unsubscribeFrom removes a single notification from the subscriptions 
	// and leaves your other subscriptions intact.
	// you must know the subscription you subscribed to, though.
	//
	public void unsubscribeFrom (string notificationName) {
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null) {
			if (verbose) {
				Debug.Log("Not connected to Notification Manager. Can't unsubscribe.");
			}
			return;
		}

		subscribedNames.Remove (notificationName);

		theNotificationManager.unsubscribeFrom (notificationName, integratedReceiver, uuid);

	}

	//
	// remove all subscriptions for this script instance.
	//
	public void unsubscribeAll(){
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null) {
			if (verbose) {
				Debug.Log("Not connected to Notification Manager. Can't unsubscribe.");
			}
			return;
		}

		theNotificationManager.removeAllSubscriptions (uuid);
		if (subscribedNames != null) 
			subscribedNames.Clear ();
	}

    //
    // filterEventForNotification requires two arguments: First, the notification for this
    // you want sub-event filtering. Second, the Event (as carried in the "Event" entry of the
    // info dictionary) that should be entered into the list. Although the list is called 
    // blacklist, it doubles as whitelist if the filtering selector 'filterUsing' is set
    // to cfxFilterMethod.WhiteList
    //
    public void filterEventForNotification(string theEvent, string theNotificationName) {
        if (blackList == null) blackList = new Dictionary<string, List<string>>();

        List<string> filtered = null;
        if (blackList.ContainsKey(theNotificationName)) {
            filtered = blackList[theNotificationName];
        } else {
            filtered = new List<string>();
            blackList.Add(theNotificationName, filtered);
        }

        if (!filtered.Contains(theEvent)) filtered.Add(theEvent);
    }

    public void filterEventForNotification(List<string> theEvents, string theNotificationName) {
        foreach (string item in theEvents) {
            filterEventForNotification(item, theNotificationName);
        }
    }

	public void removeEventFilteringForNotification(string theEvent, string theNotificationName) {
		if (blackList == null) {
			blackList = new Dictionary<string, List<string>>(); 
			return;
		}

		List<string> filtered = null;
		if (blackList.ContainsKey(theNotificationName)) {
			filtered = blackList[theNotificationName];
			if (filtered.Contains (theEvent)) {
				filtered.Remove (theEvent);
			}
		}
	}

	public void removeEventFilteringForNotification(List<string> theEvents, string theNotificationName) {
		foreach (string item in theEvents) {
			removeEventFilteringForNotification(item, theNotificationName);
		}
	}

	public virtual void preProcessInfo(Dictionary<string, object> theInfo){
		// do nothing, this is for advanced users to hook in here
	}

    //
    // this is the central receiver that finally  calls OnNotification
    // put filtering in here in your extensions
    //
    public void integratedReceiver(Dictionary<string, object> info) {
		// although virtually impossible, let's guard against a null info
		if (info == null) info = new Dictionary<string,object>();

		// save info for the implicit accessors
		theImplicitInfo = info;

		// we might be disabled, and told to remain quiet
		if (sleepOnDeactivate && !this.gameObject.activeSelf) {
			if (verbose)
				Debug.Log ("Master Object inactive and we are told to sleep. Notification ignored.");
			theImplicitInfo = null;
			return; // ok, sleep through this notification
		}

		if (sleepOnDisable && !this.enabled) {
			if (verbose)
				Debug.Log ("Script disabled and we are told to sleep. Notification ignored.");
			theImplicitInfo = null;
			return; // ok, sleep through this notification
		}
			
		// pre-process the info to get the notificationName
		preProcessInfo (info);

        //string theNotificationName = "cfxGenericNotification";
        //if (info.ContainsKey("NotificationName")) theNotificationName = info["NotificationName"] as string;
		string theNotificationName = fetchString (info, "NotificationName", "cfxGenericNotification"); // use the accessor

        if (blackList == null) blackList = new Dictionary<string, List<string>>();
//        string theEvent = null;
//        if (info.ContainsKey("Event")) theEvent = info["Event"] as string;
		string theEvent = fetchString(info, "Event", "cfxGenericNotification");

		//
		// call our infoFilter method that can be overridden
		//


		//
		// blacklist filtering
		//
        if (filterUsing == cfxFilterMethod.BlackList)
        {
            if ((theEvent != null) && (blackList.ContainsKey(theNotificationName)))    {
                // see if this event subtype is blacklisted
                List<string> filteredSubtypes = blackList[theNotificationName];
                if (filteredSubtypes.Contains(theEvent))  {
                    // this event is being filtered
                    if (verbose) Debug.Log("Filtered blacklisted Event <" + theEvent + "> from Notification <" + theNotificationName + ">.");
					theImplicitInfo = null;
					return;
                }
            }
        }

		//
		// whitelist filtering
		//
        if (filterUsing == cfxFilterMethod.WhiteList) {
            // see if the event in in whitelist
            if (theEvent == null) return; // theEvent not defined, can't be. exit
            if (!blackList.ContainsKey(theNotificationName)) return; // notificationName is not on list of allowed notifications
            List<string> allowedSubtypes = blackList[theNotificationName];
			if (!allowedSubtypes.Contains (theEvent)) { // is theEvnt part of this notificationNames allowed types?
				// nope. exit
				if (verbose) Debug.Log("Event <" + theEvent + "> failed Whitelist for <" + theNotificationName + "> and was filtered");
				theImplicitInfo = null;
				return;
			} 
        }

		// re-access the Event key in case it got dynamically changed above
        if (info.ContainsKey("Event")) theEvent = info["Event"] as string;

		// do more filtering here if you like

		// now we call the method that you should overrie in your code
		if (verbose) {
			if (theEvent == null) theEvent = "null";
			// tiny, tiny risk: if verbose is on, a null event is changed to a "null" event. Should never be a factor
			Debug.Log("Received notification <" + theNotificationName + "> , with event = <" + theEvent + ">.");
		}

		// call the OnNotification Handler Chain
        OnNotification(theNotificationName, theEvent, info);

		// forget theImplicitInfo
		theImplicitInfo = null;
    }

   	
}
