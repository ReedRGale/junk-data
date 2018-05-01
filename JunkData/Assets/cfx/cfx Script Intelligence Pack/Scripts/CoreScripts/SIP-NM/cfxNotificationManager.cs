using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  cf/x Notification Manager, Query Manager
 * 	part of SIP
 * 
 *  (C) 2017, 2018 by Christian Franz and cf/x AG
 *  all rights reserved
 * 
 */ 
public class cfxNotificationManager : MonoBehaviour {
	public bool verbose;
    /*
     * The notification manager implements a fast, simple-to-use
     * notification system to be used in scripts, and with a host
     * of easy frop-in prefabs.
     */

    // this is the delagate pattern that you must use in all receivers, and that you register when 
    // subscribing to a notification when you are not using OnNotification
    public delegate void notificationHandler(Dictionary<string, object> notificationInfo);

	// this is the delegate pattern you must use in all receivers if you are not using OnQuery
	public delegate object queryHandler(Dictionary<string, object> query);

    // to define a handler in your script, it must look like the following, but you can give it 
    // a different name:
    // 
    // public void myHandler(Dictionary<string, object> notificationInfo) {}

    //
    // Subscriptions is a list of all the delegates, accessed by keyword, that the notificationManager
    // manages. They are keyed by notificationName, and have a List of delegate procedures for each 
    // notification name to call whenever a notification is posted for that name. Note that we keep
    // the delegates in a list separate, in order to avoid a null-trapping abort the list 
    //
    private Dictionary<string, List<notificationHandler>> subscriptions = new Dictionary<string, List<notificationHandler>>();

	//
	// a list of all queries and their callbacks
	//
	private Dictionary<string, List<queryHandler>> knownQueries = new Dictionary<string, List<queryHandler>>();

    //
    // subscribers contains a list of all the scripts with all their callbacks
	//
    private Dictionary<string, List<notificationHandler>> subscribers = new Dictionary<string, List<notificationHandler>>();

	private Dictionary<string, List<queryHandler>> queryResponderScripts = new Dictionary<string, List<queryHandler>> ();


	//
	// coreBusy is a semaphore to defer all unsubscribes and subscribes duringa notification cycle
	// this is required because we should not modify the call list during notification, but many
	// objects have the tendency to unsub and sub during a notification because they just woke up
	// DeviceOrientation got destroyed as a result from a notification.
	//
	// So if coreBusy, all subscriptions go into a buffer, and will then be added after the notification
	// has completed
	//
	private static int coreBusy = 0;

	//
	// queryBusy is a similar semaphore used to lock down the query core to guard against 
	// adding responders during query time. Unlike the NM, the QueryManager will not defer
	// sign-ups but discard them. Signing up during a query makes no semantical sense
	// and is thus discouraged. 
	//
	private static int queryBusy = 0;

	//
	// codeLock is a semaphore to protect the critical code path 
	// of deferred notification against a concurrent thread. 
	// this has no effect on the single-taks version of SIP,
	// but comes into play for multi-threaded SIP
	//
	private object codeLock = new object(); // protect deferred adding subscriptions against other threads
                                            // and secure coreBusy semaphore update


    // deferred subscriptions, unsubscriptions and full unsubscriptions are kept in
    // a list of deferred actions, which is executed at the end of the lockdown core cycle
    private List<Dictionary<string, object>> deferredActions = null;



	// uniqueCount is used by the notification/query manager to generate unique
	// references that subscribers use to identify themselves when subscribing.
	// subscribers should draw a uuid when they initially connect to the manager
	// and save it. Every time they subscribe they must provide this (and only this)
	// uuid or they can't successfully unsubscribe
	// all convenience functions handle uuid transparently
	private System.Int32 uniqueCount = 0;


	//
	// notification suspension system
	// ==============================
	// 
	// the notification manager can be put on hold (suspended). Notifications that are sent
	// (including delayed) are queued, not executed. Suspending the NM may be necessary if 
	// you have paused your game (if you pause by setting timescale to 0, this should normally
	// not be an issue).
	//
	// it can still be accessed, and you can retrieve a list of all notification names that
	// are currently queued. 
	//
	// When the notification manager is told to continue, it will then execute all queued 
	// notifications. Methods exist to discard queued notifications upon restarting
	//

	private bool isSuspended = false;

	//
	// you can force queries to abort with an empty list if the notification
	// manager is suspended. We recommend against doing this, but you may want
	// to do this anyway.
	//
	public bool abortQueryOnSuspend = false;

	void Awake() {
		uniqueCount = System.Convert.ToInt32(10000.0 * Random.value);
		Debug.Log ("cf/x Notification Manager (SIP) is awake!");
		isSuspended = false;
	}

    // Use this for initialization
    void Start() {

		string uuid = uuidSIP ();
		//
        // self-test: subscribe to a notification called 'xyzABC123' and provide the template call
        //
        subscribeTo("xyzABC123", myHandler, uuid);
        postNotification("xyzABC123"); // should result in console message "NotificationManager installed"
        removeAllSubscriptions(uuid);
        postNotification("xyzABC123"); // should not result in any message at all

		// now test the query manager
		signUpToAnswerQuery ("ABC123NULL", myQueryHandler, uuid);
		Dictionary<string, object> myResult = submitQuery ("ABC123NULL");
		if (myResult == null)
			Debug.Log ("Something may be wrong");
		removeAllQueryResponsesFor (uuid);
		myResult = submitQuery ("ABC123NULL");

		// now test the notification suspension system
		subscribeTo("suspendedXXYYZZ", mySuspendHandler, uuid);
		subscribeTo("suspendedXXYYZZPositive", suspendedXXYYZZPositive, uuid);
		suspendNotifications ();

		postNotification ("suspendedXXYYZZ");
		postNotification ("suspendedXXYYZZPositive");
		if (!suspended ())
			Debug.Log ("SIP failed Manager Suspendion Detection Test");
		
		List<string> suspendedNamesToDiscard = pendingNotificationNames ();
		suspendedNamesToDiscard.Remove ("suspendedXXYYZZPositive");
		resumeNotifications (suspendedNamesToDiscard);
		removeAllSubscriptions (uuid);
    }


	#region NotificationManager

	//
	// uuidSIP is required so the notification/query manager can differentiate the scripts that subscribe. This is because
	// Unity does not identify script instances by unique identers but a name string composed of object and script name,
	// which can be a problem with same-same objects.
	// so when you connect to the notification manager, draw a uuid, save it, and use it whenever you subscribe for
	// any callback. You must also provide it when unsubscribing, or the manager will out-sync and crash on calls
	//
	public string uuidSIP () {
		// derive a uid from seconds since 1.1.1970 and a random-initialized value that is incremented with each call
		// the UUIS is not guaranteed to be unique, but enough unique for within this unity instance as used
		// with the notification manager to identify script instances

		System.DateTime  epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
		double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
		string uid = "cfxSIP-" + System.Convert.ToInt32(timestamp) + "-" + uniqueCount;
		uniqueCount = uniqueCount + 1;
		return uid;
	}

    //
    // SubscribeTo is the main subscription method through which you connect to the notification manager
    // Give it a notification name and a callback modeled after the delegate defined above, and it will
    // call your script whenever the notification is posted.
    // put in the uuidSIP you requested once for 'script' in your call so the manager can retain a $
	// reference to your script
    //
    public void subscribeTo(string notificationName, notificationHandler handler, string script)
    {

		if (script == null) {
            Debug.Log("Subscription to " + notificationName + "aborted: subscriber is NULL.");
            return;
        }


		if (coreBusy > 0) {
			// we need to defer subscriptions 
			// lock this part of the code against other threads
			lock (codeLock) {
				if (deferredActions == null)
					deferredActions = new List<Dictionary<string, object>> ();
				Dictionary<string, object> entry = new Dictionary<string, object> ();
				entry ["name"] = notificationName;
				entry ["handler"] = handler;
				entry ["script"] = script;
                entry["action"] = "subscribe";
				deferredActions.Add (entry);
			}
			// at the end of a notification run the handler checks and adds
			if (verbose) 
				Debug.Log("Core Busy: Delayed subscription of " + notificationName + " until after Notification run completes");
			return;
		}

		if (verbose) {
			Debug.Log ("Subscription registered for " + notificationName);
		}

        // ok, go through our list of notifications, and see if we already have a notification of
        // that name. If so, add the handler to the list. We could use the built-in += additions, but since
		// the delegate calling aborts at the first null reference, this is safer

		List<notificationHandler> existingNotifications = null;
        if (subscriptions.ContainsKey(notificationName)) existingNotifications = subscriptions[notificationName];
        
        if (existingNotifications != null) {
            existingNotifications.Add(handler); // this is a blind add. We may want to check if it already exists, else it'll be called twice.
                                                // note: alternatively, this could be used with the delegates +=, and no List<>. This should dramatically speed up invocation calling
        } else {
            existingNotifications = new List<notificationHandler>();
            existingNotifications.Add(handler);

            // add this to our global list of notifications
            subscriptions[notificationName] = existingNotifications;
        }

        // now do the same for subscribers. We keep a list of all subscribers so we can quickly remove
        // them with a single call
        List<notificationHandler> existingSubscriber = null;

		if (subscribers.ContainsKey (script)) {
			// fetch existing list of subsciber scripts
			existingSubscriber = subscribers[script];
		}

        if (existingSubscriber != null) {
            existingSubscriber.Add(handler);
			if (verbose) Debug.Log ("used EXISTING subscriber for " + notificationName + "/<"+ script.ToString ()+ ">");
        } else {
            existingSubscriber = new List<notificationHandler>();
            existingSubscriber.Add(handler);
			subscribers[script] = existingSubscriber;
			if (verbose) Debug.Log ("Added new subscriber for " + notificationName  + "/<"+ script.ToString ()+ ">");
        }

		if (verbose) Debug.Log("Success: Subscriptions/Subscribers: " + subscriptions.Count + "/" + subscribers.Count + " for: " + notificationName); 
    }


    public void removeAllSubscriptions(string theScriptObject)
	{
		if (theScriptObject == null) {
			if (verbose)
				Debug.Log ("NULL script objects - you need to supply me with a script object");
			return;
		}

		if (!subscribers.ContainsKey (theScriptObject))
			return;

		if (coreBusy > 0) {
			// lock this part of the code against other threads
			lock (codeLock) {
				if (deferredActions == null)
					deferredActions = new List<Dictionary<string, object>> ();
                Dictionary<string, object> entry = new Dictionary<string, object>();
                entry["script"] = theScriptObject;
				entry["action"] = "unsubscribeAll";
                deferredActions.Add(entry);

			}

			if (verbose)
				Debug.Log ("Deferred full unsubscribe until core released");
			return;
		}


        // iterate over all notifications registered under this object
        List<notificationHandler> allNotifications = subscribers[theScriptObject]; 
        foreach (notificationHandler notification in allNotifications)
        {
            // iterate over all notifications and remove any call to this handler
            List<string> allNames = new List<string>(subscriptions.Keys);
            foreach (string notificationName in allNames) {
                List<notificationHandler> theCallBacks = subscriptions[notificationName];
				if (theCallBacks.Contains (notification))
					theCallBacks.Remove (notification);
				// was this the last?
				if (theCallBacks.Count < 1)
					subscriptions.Remove (notificationName);
            }
        }

        // finally, remove me from the dictionary
        subscribers.Remove(theScriptObject);

		if (verbose) Debug.Log("Remaining Subscriptions/Subscribers: " + subscriptions.Count + "/" + subscribers.Count + " after removing all"); 
    }

    public void unsubscribeFrom(string notificationName, notificationHandler handler, string script)
    {

		if (coreBusy > 0) {
			// we need to defer subscriptions 
			// lock this part of the code against other threads
			lock (codeLock) {
				if (deferredActions == null)
					deferredActions = new List<Dictionary<string, object>> ();
				Dictionary<string, object> entry = new Dictionary<string, object> ();
				entry ["name"] = notificationName;
				entry ["handler"] = handler;
				entry ["script"] = script;
                entry["action"] = "unsubscribe";
				deferredActions.Add (entry);
				// at the end of a notification run the handler checks and adds
			}
			if (verbose)
				Debug.Log ("Core Busy: Delayed cancellation of " + notificationName + " until after Notification run completes");
			
			return;
		}

        List<notificationHandler> existingNotifications = null;
        if (subscriptions.ContainsKey(notificationName)) existingNotifications = subscriptions[notificationName];

        if(existingNotifications != null) {
            // find and remove the entry - or the whole entry if last
			if (existingNotifications.Count <= 1) {
				// only one left, remove the whole entry
				subscriptions.Remove (notificationName);
			} else {
				existingNotifications.Remove(handler);
			}
			if (verbose) Debug.Log ("Removed " + notificationName + " from List of existingNotifications.");

        }

        List<notificationHandler> existingSubscriber = null;

		if (subscribers.ContainsKey(script))
        {
			existingSubscriber = subscribers[script];
			if (existingSubscriber.Count <= 1) {
				subscribers.Remove (script);
			} else {
				existingSubscriber.Remove (handler);
			}
			if (verbose) Debug.Log ("Removed " + notificationName + " from List of subscribers.");
        }

		if (verbose) Debug.Log("Remaining Subscriptions/Subscribers: " + subscriptions.Count + "/" + subscribers.Count + " after removing " + notificationName); 
    }

	//
    // this is the workhorse notifier. To distribute a notification to all subscribed objects
    // simply call postNotification with the name, and info.
	// codeLock prevents against deadlock/race conditions,
	// coreBusy adds re-entrancy
	//
    public void postNotification(string name) {
        Dictionary<string, object> theDict = new Dictionary<string, object>();
        postNotification(name, theDict);
    }

    public void postNotification(string name, Dictionary<string, object> info)
    {
		//
		// if we are suspended, queue the request, and exit
		//
		if (isSuspended) {
			Dictionary<string, object> theNotification = encodeNotification (name, info, 0f);
			suspendedNotifications.Add (theNotification);
			return;
		}

        //
		// close down the core: semaphore up by one. This counts re-entries
		// 
		lock (codeLock) {
			// the code lock is to prevent a competing thread to srew up the coreBusy count if they arrive
			// at the same time
			coreBusy = coreBusy + 1;
		}

		if (info == null) info = new Dictionary<string, object>();

        List<notificationHandler> existingNotifications = null;
        if (subscriptions.ContainsKey(name)) existingNotifications = subscriptions[name]; 
		if (existingNotifications != null) {
			// add the "notificationName" KVP to the dict, so it's always accessible
			info ["NotificationName"] = name;

			if (verbose) {
				Debug.Log ("Broadcasting Notification: " + name + "with dict:");
				foreach (string aKey in info.Keys) {
					Debug.Log ("<" + aKey + "> : <" + info [aKey].ToString () + ">"); 
				}
			}
				
			foreach (notificationHandler handler in existingNotifications) {
				// we call them separately to avaid the list break when we hit
				// a null object

				if (handler != null) {
					if (verbose) {
						Debug.Log ("Calling" + handler);
					} else {
						if (verbose) {
							Debug.Log ("Skipped " + handler + ": NULL");
						}
					}
					handler (info);
				}
			}


		} else {
			if (verbose) {
				Debug.Log ("No subscriber found for <" + name + ">. We have:"); 
				foreach (string subs in subscriptions.Keys) {
					Debug.Log (subs);
				}
			}
		}
		// 
		// we are done. what remains is to process all subs and de-subs that happened during 
		// core cycle. However, since we are re-entrant, do this only on the very last exit
		//
		// make it atomic so another thread can't barge in
		lock(codeLock) {
			coreBusy = coreBusy - 1; // semaphore down one.
		}

		// the semaphore counts re-entries. We only execute the latter part
		// if we really have exited for the last time
		if (coreBusy > 0) {
			if (verbose) Debug.Log ("Decreasing semaphore to " + coreBusy); 
			return;
		}

		// work through all deferred notifications. 
		// must not be interrupted by other threads
		// this should be next to impossible since coreBusy MUST be >= 1
		// but you never know
		lock (codeLock) {
            if (deferredActions != null) {
                if (verbose) Debug.Log("executing deferred actions:");

                foreach (Dictionary<string, object> entry in deferredActions) {
                    string action = entry["action"] as string;
                    if (action == "subscribe") {
                        string notificationName = entry["name"] as string;
                        notificationHandler theHandler = entry["handler"] as notificationHandler;
                        string theScript = entry["script"] as string;
                        subscribeTo(notificationName, theHandler, theScript);
                        if (verbose) Debug.Log("Subscribe to " + notificationName);
                        continue;
                    }

                    if (action == "unsubscribe") {
                        string notificationName = entry["name"] as string;
                        notificationHandler theHandler = entry["handler"] as notificationHandler;
                        string theScript = entry["script"] as string;
                        unsubscribeFrom(notificationName, theHandler, theScript);
                        if (verbose) Debug.Log("Cancelled: " + notificationName);
                        continue;
                    }

                    if (action == "unsubscribeAll") {
                        string theScript = entry["script"] as string;
                        removeAllSubscriptions(theScript);
                        if (verbose) Debug.Log("CancelledAll for script " + theScript);
                        continue;
                    }

                    Debug.Log("WARNING: unknown recorded action: " + action + "in deferred subscription handler!");
                }

                deferredActions.Clear();
                deferredActions = null;
            }


		} // end of lock
		if (coreBusy < 0) {
				Debug.Log ("WARNING: Semaphore CoreBusy out of Sync!");
		}
    }

	/*
	 * Tiny debug helper if you are wondering what subscriptions are currently active
	 */

	public void dumpSubscriptions() {
		foreach (string subs in subscriptions.Keys) {
			Debug.Log (subs);
		}
	}

	/*
	 * The postDelayedNotification method uses a coroutine to avoid
	 * blocking. Unfortunately, it requires exceedingly inelegant local 
	 * variable passing because of the way coroutines are invoked
	 */

	private Dictionary<string, object> passForDelay; // special var only used to pass info to coroutine
	private float passDelay = 0f;

    public void postDelayedNotification(string name, Dictionary<string, object> info, float delay)
    {
		if (delay < 0.01) {
			// anything less than 1/100s delay is executed immedately 
			postNotification (name, info);
			return;
		}

		//
		// if we are suspended, queue the request, and exit
		//
		if (isSuspended) {
			Dictionary<string, object> theNotification = encodeNotification (name, info, delay);
			suspendedNotifications.Add (theNotification);
			return;
		}

		// invoke coroutine
        // yield for number of seconds.
        // done. Coroutines are reentrant, as
        // they create separate instances for each invocation
		passDelay = delay;
		info["NotificationName"] = name;
		passForDelay = info;
		StartCoroutine ("DelayAndPostViaCoroutine");
    }


	IEnumerator DelayAndPostViaCoroutine() {
		// immediately fetch the info bit, and create a copy
		Dictionary<string, object> info = new Dictionary<string, object> (passForDelay); // create a copy for this instance of coroutine
		yield return new WaitForSeconds(passDelay);
		// we'll continue here after delay, in our own instance, without blocking

		// recover notificationName
		string notificationName = info["NotificationName"] as string;
		postNotification (notificationName, info);
	}


	//
	// Suspending and continuing the notification manager
	//
	//
	// suspendedNotifications is the queue we put notifications in
	// when the NM is suspended. We encode Info, Delay and NotificationName
	// into a dictionary. Dict is private
	//
	private List<Dictionary<string, object>> suspendedNotifications;

	Dictionary<string, object> encodeNotification(string notificationName, Dictionary<string, object> info, float delay) {
		Dictionary<string, object> theDict = new Dictionary<string, object> ();
		if (notificationName == null)
			notificationName = "cfxGenericNotification";
		if (info == null) info = new Dictionary<string, object> ();
		theDict ["NotificationName"] = notificationName;
		theDict ["Info"] = info;
		theDict ["Delay"] = delay.ToString ();
		return theDict;
	}

	// 
	// to suspend notifications, simply invoke suspendNotifications.
	// all incoming notifications are queued
	//
	public void suspendNotifications() {
		if (isSuspended) return;
		suspendedNotifications = new List<Dictionary<string, object>>();
		isSuspended = true;
	}


	//
	// resuming notifications allows multiple choices:
	//  - simply resume, executing all queued notifications
	//  - resume, but discard all queued notifications
	//  - resume, but discard specifically named notifications
	//
	public void resumeNotifications(bool discardAll) {
		if (discardAll) {
			suspendedNotifications.Clear();
		}

		doResumeNotifications(suspendedNotifications);

		// implementation note:
		// above is much faster than getting all notification names as a list and then
		// call resume with that list, e.g.
		//
		// List<string> allNotifications = pendingNotificationNames (true);
		// resumeNotifications (allNotifications);
		//
	}

	public void resumeNotifications() {
		resumeNotifications(false);
	}


	public void resumeNotifications(string discardNotificationNamed) {
		List<string> nameList = new List<string> ();
		nameList.Add (discardNotificationNamed);
		resumeNotifications (nameList);
	}


	// main resume filter
	public void resumeNotifications(List<string> discardNotificationsNamed) {
		if (!isSuspended) return;
		// 
		// ensure that there are Lists to iterate.
		//
		if (discardNotificationsNamed == null)
			discardNotificationsNamed = new List<string>();
		//
		// we only iterate once, checking name and script
		//
		List<Dictionary<string, object>> filteredNotifications = new List<Dictionary<string, object>>();
		foreach (Dictionary<string, object> aNotification in suspendedNotifications){
			string aName = aNotification["NotificationName"] as string;
			if (discardNotificationsNamed.Contains(aName)) continue; // if in there, skip copy phase
			filteredNotifications.Add(aNotification);
		}
		doResumeNotifications(filteredNotifications);
	}


	public void doResumeNotifications(List<Dictionary<string, object>> theQueue) {
		// switch off suspension
		isSuspended = false;

		// now execute all remaining notifications
		foreach (Dictionary<string, object> aNotification in theQueue) {
			string notificationName = aNotification["NotificationName"] as string;
			Dictionary<string, object> info = aNotification["Info"] as Dictionary<string, object>;
			float delay = 0;
			float.TryParse(aNotification["Delay"] as string, out delay);
			postDelayedNotification(notificationName, info, delay);
		}
	}

	//
	// get a list of all pending notification names
	// if you pass true for unique, each notification name is 
	// only returned once
	//
	public List<string> pendingNotificationNames (){
		return pendingNotificationNames (false);
	}

	public List<string> pendingNotificationNames(bool unique) {
		List<string> thePending = new List<string> ();
		if (!isSuspended)
			return thePending;

		foreach (Dictionary<string, object> aNotification in suspendedNotifications) {
			string notificationName = aNotification ["NotificationName"] as string;
			if (unique) {
				if (!thePending.Contains (notificationName))
					thePending.Add (notificationName);
			} else {
				thePending.Add (notificationName);
			}
		}

		return thePending;
	}

	//
	// external read-only accessor to suspended attribute
	//
	public bool suspended(){
		return isSuspended;
	}

	#endregion

	#region query manager
	/*
	 * Query Manager:
	 * ==============
	 * Similar to notification Manager, but it compiles answers and passes back the result
	 * 
	 * you sign up for queries by providing a callback, and the query you are able to respond to
	 * or "*" as query if you want to be called every time there is a query
	 * 
	 */

	public void signUpToAnswerQuery(string queryName, queryHandler theQueryHandler, string queryResponderScript) {
	
		if (queryResponderScript == null) {
			Debug.Log("Sign up for " + queryName + " query aborted: subscriber is NULL.");
			return;
		}

		if (queryBusy > 0) {
			Debug.Log ("WARNING: You are trying to sign up for query " + queryName + " during query processing. Sign-up discarded.");
			return;
		}

		if (verbose) {
			Debug.Log ("Signed up for query " + queryName);
		}
		// ok, go through our list of notifications, and see if we already have a notification of
		// that name. If so, add the handler to the list. We could use the built-in += additions, but they
		// terminate with the first null handler. This we do not want

		List<queryHandler> existingQueries = null;
		if (knownQueries.ContainsKey(queryName)) existingQueries = knownQueries[queryName];

		if (existingQueries != null) {
			existingQueries.Add(theQueryHandler); // this is a blind add. We may want to check if it already exists, else it'll be called twice.
			// note: alternatively, this could be used with the delegates +=, and no List<>. This should dramatically speed up invocation calling
		} else {
			existingQueries = new List<queryHandler>();
			existingQueries.Add(theQueryHandler);

			// add this to our global list of known queries
			knownQueries[queryName] = existingQueries;
		}

		// now do the same for subscribers. We keep a list of all subscribers so we can quickly remove
		// them with a single call
		List<queryHandler> existingResponders = null;

		if (queryResponderScripts.ContainsKey(queryResponderScript)) existingResponders = queryResponderScripts[queryResponderScript];
		if(existingResponders != null) {
			existingResponders.Add(theQueryHandler);
		} else {
			existingResponders = new List<queryHandler> ();
			existingResponders.Add(theQueryHandler);
			queryResponderScripts[queryResponderScript] = existingResponders;
		}

	}

	public void signUpToAnswerQuery(queryHandler theQueryHandler, string queryResponderScript) {
		string theQueryName = "*"; // wildcard: all queries
		signUpToAnswerQuery (theQueryName, theQueryHandler, queryResponderScript);
	}


	// 
	// removing query responders
	//
	public void removeAllQueryResponsesFor(string theScriptObject)
	{

		if (theScriptObject == null) return;
		if (!queryResponderScripts.ContainsKey(theScriptObject)) return;

		if (queryBusy > 0) {
			Debug.Log ("WARNING: You are trying to rescind all during query processing. Rescind discarded.");
			return;
		}


		// iterate over all notifications registered under this object
		List<queryHandler> allQueryHandlers = queryResponderScripts[theScriptObject]; 
		foreach (queryHandler aHandler in allQueryHandlers)
		{
			// iterate over all notifications and remove any call to this handler
			List<string> allQueryNames = new List<string>(knownQueries.Keys);
			foreach (string queryName in allQueryNames) {
				List<queryHandler> theCallBacks = knownQueries [queryName];
				if (theCallBacks.Contains(aHandler)) theCallBacks.Remove(aHandler);
			}
		}

		// finally, remove me from the dictionary
		queryResponderScripts.Remove(theScriptObject);
	}

	public void removeAnswerForQuery(string queryName, queryHandler handler, string answerScript)
	{
		if (queryBusy > 0) {
			Debug.Log ("WARNING: You are trying to sign up for query " + queryName + " during query processing. Sign-up discarded.");
			return;
		}

		List<queryHandler> existingQueries = null;
		if (knownQueries.ContainsKey(queryName)) existingQueries = knownQueries[queryName];

		if(existingQueries != null) {
			// find and remove the entry
			existingQueries.Remove(handler);
		}

		// do the same for the scripts
		existingQueries = null;

		if (queryResponderScripts.ContainsKey(answerScript))
		{
			existingQueries = queryResponderScripts[answerScript];
			existingQueries.Remove(handler);
		}
	}

	//
	// submitting a query. Multiple Overloads ending in scoped query.
	// if you submit a query without scope, all are queried,
	// the scope acts as a whitelist only if present
	//
	public Dictionary<string, object> submitQuery(string name) {
		Dictionary<string, object> theDict = new Dictionary<string, object>();
		return submitQuery(name, theDict);
	}

	public Dictionary<string, object> submitQuery(string name, Dictionary<string, object> info)
	{
		return submitQuery (name, info, null);

	}

	//
	// A scoped query is a query with a whitelist called 'scope' on the scripts that
	// may be called. before we commit a query to a script, we first check if it is
	// on the whitelist
	// We allow one wildcard: a null whitelist means no whitelisting.
	//

	string getScriptForHandler(queryHandler handler) {
		string targetScript = null;

		// find the script that this handler belongs to 
		foreach (string aScript in queryResponderScripts.Keys) {
			// run through all scripts
			List<queryHandler> allCallBacksForThisScript = queryResponderScripts [aScript];
			foreach (queryHandler thisCallBack in allCallBacksForThisScript) {
				if (thisCallBack == handler) {
					targetScript = aScript;
					return targetScript;
				}
			}
		}
		return null;
	}

	//
	// Main Query Method
	//

	public Dictionary<string, object> submitQuery(string name, Dictionary<string, object> info, List<string> scopeWhiteList)
	{
		if (name == "*") {
			if (verbose)
				Debug.Log ("Illegal query name '*'. Aborting Query");
			return null;
		}

		//
		// if NM is suspended and our abortQueryOnSuspend property is set, return immediately with an empty list
		//
		if (abortQueryOnSuspend && isSuspended) {
			if (verbose)
				Debug.Log ("Aborting Query because NM is suspended and abortQuery switch is set");
			return new Dictionary<string, object> ();
		}

		// close down the core
		queryBusy = queryBusy + 1;

		//List<object> results = new List<object> ();
		Dictionary<string, object> results = new Dictionary<string, object>();

		if (info == null) info = new Dictionary<string, object>();
		info ["Query"] = name;

		List<queryHandler> existingQueries = new List<queryHandler>();
		if (knownQueries.ContainsKey(name)) existingQueries.AddRange (knownQueries[name]); 
		if (knownQueries.ContainsKey ("*"))
			existingQueries.AddRange (knownQueries ["*"]);
		
		if(existingQueries.Count > 0) {

			if (verbose) {
				Debug.Log ("Broadcasting query: " + name + "with dict:");
				foreach (string aKey in info.Keys) {
					Debug.Log ("<" + aKey + "> : <" + info [aKey].ToString () + ">"); 
				}
			}

			foreach (queryHandler handler in existingQueries)
			{
				// we call them separately to avoid the list break when we hit
				// a null object

				if (handler != null) {

					string targetScript = getScriptForHandler(handler);

					// sanity check: the MUST be a script for this handler
					if (targetScript == null) {
						if (verbose)
							Debug.Log ("Aborted: owning script for handler not found");
					}

					// whitelist checking: only if whitelist exists with one or more entries
					// this is so we can supply an initially empty whitelist when we write
					// generic code.
					if ((scopeWhiteList != null)) { // NOTE: ONLY ON (NOT NULL) !! -- MUST BREAK ON EMPTY LIST, THAT IS CORRECT
						// we need to white-list check this query.
						if (!scopeWhiteList.Contains (targetScript)){
							// cycle
							break;
						}
					}

					if (verbose) {
						Debug.Log ("Calling" + handler);
					} 

					// do the actual call
					object result = handler(info);

					// put the result into a dictionary, keyed to the script that was called
					if (result != null) {
						// note: if someone was insane enough to have two handlers for this query
						// in the same script, this will overwrite the first result on the second pass
						// but nobody would be THAT stupid, right? RIGHT???
						results [targetScript] = result;
					}
				}
			}
		}

		// release one level of semaphore
		queryBusy = queryBusy - 1;

		return results;
	}

    //
    // template call to test the notificaton manager
    //
    public void myHandler(Dictionary<string, object> notificationInfo) {
        Debug.Log("cfxNotificationManager installed and tested successfully");
    }

	public object myQueryHandler(Dictionary<string, object> queryInfo) {
		Debug.Log("cfxQueryManager installed and tested successfully");
		return "Test successful";
	}

	public void mySuspendHandler(Dictionary<string, object> notificationInfo) {
		Debug.Log("cfxNotificationManager FAILED suspension removal test");
	}

	public void suspendedXXYYZZPositive (Dictionary<string, object> notificationInfo) {
		Debug.Log("cfxNotificationManager passed resume test");
	}
	#endregion
}
