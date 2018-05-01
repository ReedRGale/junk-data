using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
/*
 * cf/x SIP for Unity
 * 
 * Copyright (C) 2017, 2018 by cf/x AG and Christian Franz
 *
 * NotificationManager base class, all notification classes descend from here
 * This class implements connecting to the notification manger prefab, 
 * and gets an uuid from the NM so that we can identify our methods
 * On Destroy we tell the NM to forget about us
 * 
 */
public class cfxSIPNetworkManagerBehaviour : NetworkManager{

    [HideInInspector]  public cfxNotificationManager theNotificationManager = null; // auto-connects, pre-config only in exceptional cases
    public bool verbose; // if true, this object trly becomes chatty, telling you everything it does

	[Tooltip("If true, I will not respond to notifications when enclosing OBJECT is disabled.")] 
	public bool sleepOnDeactivate; // 
	[Tooltip("If true, I will not respond to notifications when this SCRIPT is disabled.")]
	public bool sleepOnDisable; // if true, we will not respond to notification when SCRIPT is disabled

	[HideInInspector] //public string uuid; // this is used across all SIP methods that communicate with the NM to identify itself
	private string _uuid; // this property will be only settable from within here

	//
	// uuid getter/setter: ensures that connected to notification manager if used 
	// before actually sending or signing up for a notification
	//
	public string uuid
	{
		get { if (_uuid == null) {
				connectToNotificationManager ();
			} 
			return _uuid; }
		private set { _uuid = value; }
	}
	//
	// implicitInfo is a copy of theInfo as it comes in that is used for
	// the accessor convenience access methods. It's set from within cfxNotificationReceiver
	//
	[HideInInspector] public Dictionary<string, object> theImplicitInfo = null;

	/*
     * Helper Procs for common tasks
     */

	//
	// UniqueNotificationName
	// Simply applend uuid to notification name so we can have 
	// a notification that is bound to the script's incarnation
	//
	public string UniqueNotificationName(string inNotificationName) {
		return inNotificationName + uuid;
	}

	//
	// Query Return Processing
	//
	public List<GameObject> convertToGameObjects(List<object> inList)
	{
		List<GameObject> outList = new List<GameObject>();
		foreach (object anObject in inList)
		{
			outList.Add(anObject as GameObject);
		}
		return outList;
	}

	public List<GameObject> convertToGameObjects(List<object> inList, bool sorted)
	{
		List<GameObject> outList = convertToGameObjects(inList);
		if (sorted) {
			outList.Sort(delegate (GameObject i1, GameObject i2) {
				return i1.name.CompareTo(i2.name);
			});
		}

		return outList;
	}

	// 
	// A simple List<GameObject> sort that sorts ascending, direct insert. 
	// Suitable for one-pass, small to medium size list. Worst Case: O(n*n)
	// Insertion sort modified to not requiring a swap
	//
	public List<GameObject> sipSortGameObjectList(List<GameObject> unsorted) {
		List<GameObject> sorted = new List<GameObject>();
		// direct insert ("card player") sort
		bool inserted = false;
		int maxItem = unsorted.Count; // deref count, it's invariant
		for(int i = 0; i < maxItem; i++)	{
			inserted = false;
			for (int j = 0; j < sorted.Count; j++) {
				// Look for card to insert before. If none found, add at the end
				if (unsorted[i].name.CompareTo(sorted[j].name) < 0)	{
					//Insert before that "card" as its name is greater.
					sorted.Insert(j, unsorted[i]);
					inserted = true; // remember that we inseerted already
					break; // we can exit loop now
				}
			}

			if (!inserted) {
				// items wasn't inseerted, so it's added at the end
				sorted.Add(unsorted[i]);
			}

		}
		return sorted;
	}

	//
	// info dictionary access -- retrieval
	//
	public void dumpSIPDict(Dictionary<string, object> info) {
		if (info == null) return;
		foreach (string aKey in info.Keys) {
			if (info[aKey] is string) {
				Debug.Log("dict[" + aKey + "] = " + info[aKey]);
			} else {
				Debug.Log("dict[" + aKey + "] = <opaque object>" );
			}
		}

	} 

	public object fetchObject(string theKey) {
		return fetchObject (theKey, null);
	}

	public object fetchObject(string theKey, object defaultObject) {
		if (theImplicitInfo == null)
			Debug.Log ("WARNING: implicit info dictionary not set. Are you calling fetchObject from your own notification handler?");
		return fetchObject (theImplicitInfo, theKey, defaultObject);
	}

	public object fetchObject(Dictionary<string, object> info, string theKey) {
		return fetchObject (info, theKey, null);
	}

	public object fetchObject(Dictionary<string, object> info, string theKey, object defaultValue) {
		if (info == null) {
			if (verbose)
				Debug.Log ("fetchObjet invoked with NULL info dict");
			return defaultValue;
		}

		if (theKey == null) {
			if (verbose)
				Debug.Log ("fetchObjet invoked with NULL key");
			return defaultValue;
		}

		if (info.ContainsKey (theKey))
			return info [theKey];
		
		if (verbose)
			Debug.Log ("fetchObject returns default object for key <" + theKey + ">");
		return defaultValue;
	}

	public string fetchString(string theKey) {
		return fetchString (theKey, null);
	}

	public string fetchString(string theKey, string defaultObject) {
		if (theImplicitInfo == null)
			Debug.Log ("WARNING: implicit info dictionary not set. Are you calling fetchString from your own notification handler?");
		return fetchString (theImplicitInfo, theKey, defaultObject);
	}


	public string fetchString(Dictionary<string, object> info, string theKey) {
		return fetchString (info, theKey, null);

	}


	public string fetchString(Dictionary<string, object> info, string theKey, string defaultValue) {
		string theString = fetchObject (info, theKey) as string;
		if (theString == null) {
			if (verbose)
				Debug.Log ("fetchString returns default string for key <" + theKey + ">");
			theString = defaultValue; 
		}
		return theString;
	}


	public GameObject fetchGameObject(string theKey) {
		return fetchGameObject (theKey, null);
	}

	public GameObject fetchGameObject(string theKey, GameObject defaultObject) {
		if (theImplicitInfo == null)
			Debug.Log ("WARNING: implicit info dictionary not set. Are you calling fetchGameObject from your own notification handler?");
		return fetchGameObject (theImplicitInfo, theKey, defaultObject);
	}

	public GameObject fetchGameObject(Dictionary<string, object> info, string theKey){
		return fetchGameObject (info, theKey, null);
	}

	public GameObject fetchGameObject(Dictionary<string, object> info, string theKey, GameObject defaultValue){
		return fetchObject (info, theKey, defaultValue) as GameObject;
	}



	public int fetchInt(string theKey) {
		return fetchInt (theKey, 0);
	}

	public int fetchInt(string theKey, int defaultValue) {
		if (theImplicitInfo == null)
			Debug.Log ("WARNING: implicit info dictionary not set. Are you calling fetchInt from your own notification handler?");
		return fetchInt (theImplicitInfo, theKey, defaultValue);
	}

	public int fetchInt(Dictionary<string, object> info, string theKey) {
		return fetchInt (info, theKey, 0);
	}

	public int fetchInt(Dictionary<string, object> info, string theKey, int defaultValue) {
		string theIntString = fetchString (info, theKey);
		if (theIntString == null) {
			if (verbose)
				Debug.Log ("fetchInt returns default value because no key <" + theKey + "> found");
			return defaultValue;
		}

		int theInt = defaultValue;

		if (int.TryParse (theIntString, out theInt)) {
			// note: we might as well just drop through, and return theFloat at the end;
			// this way, however, we could add some more debug code
			return theInt;
		};
		if (verbose)
			Debug.Log ("fetchInt returns default value because key <" + theKey + "> cound not be converted to int");

		return defaultValue;
	}

	public float fetchFloat(string theKey) {
		return fetchFloat (theKey, 0f);
	}

	public float fetchFloat(string theKey, float defaultValue) {
		if (theImplicitInfo == null)
			Debug.Log ("WARNING: implicit info dictionary not set. Are you calling fetchFloat from your own notification handler?");
		return fetchFloat (theImplicitInfo, theKey, defaultValue);
	}

	public float fetchFloat(Dictionary<string, object> info, string theKey) {
		return fetchFloat (info, theKey, 0f);
	}

	public float fetchFloat(Dictionary<string, object> info, string theKey, float defaultValue) {
		string theFloatString = fetchString (info, theKey);
		if (theFloatString == null)
			return defaultValue;

		float theFloat = defaultValue;

		if (float.TryParse (theFloatString, out theFloat)) {
			// note: we might as well just drop through, and return theFloat at the end;
			// this way, however, we could add some more debug code
			return theFloat;
		};
		if (verbose)
			Debug.Log ("fetchFloat returns default value because key <" + theKey + "> cound not be converted to int");
		
		return defaultValue;
	}


	public Color fetchColor(string theKey) {
		return fetchColor (theKey, Color.black);
	}

	public Color fetchColor(string theKey, Color defaultValue) {
		if (theImplicitInfo == null)
			Debug.Log ("WARNING: implicit info dictionary not set. Are you calling fetchColor from your own notification handler?");
		return fetchColor (theImplicitInfo, theKey, defaultValue);
	}

	public Color fetchColor(Dictionary<string, object> info, string theKey) {
		return fetchColor (info, theKey, Color.black);
	}

	public Color fetchColor(Dictionary<string, object> info, string theKey, Color defaultValue) {
		Color theColor = defaultValue;
		string theString = fetchString (info, theKey);
		if (theString == null) {
			if (verbose)
				Debug.Log ("fetchColor returns default value because no key <" + theKey + "> found");
			return defaultValue;
		}

		if (ColorUtility.TryParseHtmlString (theString, out theColor)) {
			return theColor;
		}
		if (verbose)
			Debug.Log ("fetchColor returns default value because value for key <" + theKey + "> cannot be converted to a color. Value is <"+ theString +">");

		return defaultValue;
	}

	public bool fetchBool(string theKey) {
		return fetchBool(theKey, false);
	}

	public bool fetchBool(string theKey, bool defaultValue) {
		if (theImplicitInfo == null)
			Debug.Log ("WARNING: implicit info dictionary not set. Are you calling fetchBool from your own notification handler?");
		return fetchBool(theImplicitInfo, theKey, defaultValue);
	}

	public bool fetchBool(Dictionary<string, object> info, string theKey) {
		return fetchBool (info, theKey, false);
	}

	public bool fetchBool(Dictionary<string, object> info, string theKey, bool defaultValue) {
//		bool theBool = defaultValue;
		string theString = fetchString (info, theKey);
		if (theString == null) {
			if (verbose)
				Debug.Log ("fetchBool returns default value because no key <" + theKey + "> found");
			return defaultValue;
		}

		if (theString.ToUpper () == "TRUE" || theString.ToUpper () == "YES")
			return true;
		return false;

	}

	public Vector3 fetchVector3(string theKey)
	{
		return fetchVector3(theKey, Vector3.zero);
	}

	public Vector3 fetchVector3(string theKey, Vector3 defaultValue)
	{
		if (theImplicitInfo == null)
			Debug.Log("WARNING: implicit info dictionary not set. Are you calling fetchVector3 from your own notification handler?");
		return fetchVector3(theImplicitInfo, theKey, defaultValue);
	}

	public Vector3 fetchVector3(Dictionary<string, object> info, string theKey)
	{
		return fetchVector3(info, theKey, Vector3.zero);
	}

	public Vector3 fetchVector3(Dictionary<string, object> info, string theKey, Vector3 defaultValue)
	{
		Vector3 theVector = new Vector3();

		theVector.x = fetchFloat(info, theKey + ".VectorX", defaultValue.x);
		theVector.y = fetchFloat(info, theKey + ".VectorY", defaultValue.y);
		theVector.z = fetchFloat(info, theKey + ".VectorZ", defaultValue.z);

		//  Debug.Log("About to exit FetchVector with v.x = " + theVector.x + ".y = " + theVector.y + ".z = " + theVector.z);
		return theVector;
	}



	//
	// Info access: storing 
	//
	public void addColorToInfo(Dictionary<string, object> info, string theKey, Color theColor) {
		if (info == null) {
			if (verbose)
				Debug.Log ("addColorToInfo invoked with null info dictionary");
			return;
		}
		if (theKey == null) {
			if (verbose)
				Debug.Log ("addColorToInfo invoked with null key");
			return;
		}

		// a glaring omission in ColorUtility is that it does not precede the 
		// converted html string with a hash. We add it here.
		info [theKey] = "#" + ColorUtility.ToHtmlStringRGBA (theColor);
	}

	public void addIntToInfo(Dictionary<string, object> info, string theKey, int theInt) {
		if (info == null) {
			if (verbose)
				Debug.Log ("addIntToInfo invoked with null info dictionary");
			return;
		}
		if (theKey == null) {
			if (verbose)
				Debug.Log ("addIntToInfo invoked with null key");
			return;
		}
		
		info [theKey] = theInt.ToString ();
	}

	public void addFloatToInfo(Dictionary<string, object> info, string theKey, float theFloat) {
		if (info == null) {
			if (verbose)
				Debug.Log ("addFloatToInfo invoked with null info dictionary");
			return;
		}
		if (theKey == null) {
			if (verbose)
				Debug.Log ("addFloatToInfo invoked with null key");
			return;
		}

		info [theKey] = theFloat.ToString ();
	}

	public void addStringToInfo(Dictionary<string, object> info, string theKey, string theString) {
		if (info == null) {
			if (verbose)
				Debug.Log ("addStringToInfo invoked with null info dictionary");
			return;
		}
		if (theKey == null) {
			if (verbose)
				Debug.Log ("addStringToInfo invoked with null key");
			return;
		}
		if (theString == null) {
			if (verbose)
				Debug.Log ("addStringToInfo invoked with null theString");
			return;
		}

		info [theKey] = theString;
		
	}

	public void addGameObjectToInfo(Dictionary<string, object> info, string theKey, GameObject theGameObject) {
		if (info == null) {
			if (verbose)
				Debug.Log ("addGameObjectToInfo invoked with null info dictionary");
			return;
		}
		if (theKey == null) {
			if (verbose)
				Debug.Log ("addGameObjectToInfo invoked with null key");
			return;
		}
		if (theGameObject == null) {
			if (verbose)
				Debug.Log ("addGameObjectToInfo invoked with null theGameObject");
			return;
		}

		info [theKey] = theGameObject;

	}

	public void addBoolToInfo(Dictionary<string, object> info, string theKey, bool theBool) {
		if (info == null) {
			if (verbose)
				Debug.Log ("addBoolToInfo invoked with null info dictionary");
			return;
		}
		if (theKey == null) {
			if (verbose)
				Debug.Log ("addBoolToInfo invoked with null key");
			return;
		}

		if (theBool) {
			info[theKey] = "true";
		} else {
			info[theKey] = "false";
		}

	}

	public void addVector3ToInfo(Dictionary<string, object> info, string theKey, Vector3 theVector) {
		if (info == null)
		{
			if (verbose)
				Debug.Log("addVector3ToInfo invoked with null info dictionary");
			return;
		}
		if (theKey == null)
		{
			if (verbose)
				Debug.Log("addVector3ToInfo invoked with null key");
			return;
		}

		addFloatToInfo(info, theKey + ".VectorX", theVector.x);
		addFloatToInfo(info, theKey + ".VectorY", theVector.y);
		addFloatToInfo(info, theKey + ".VectorZ", theVector.z);
	}

	/*
	 * 
	 * 
	 * SIP base behaviour methods follow below
	 * 
	 * 
	 */

	// special accessor to null uuid
	public void resetUUID(){
		_uuid = null;
	}

	// resetting the script. Should only be required when a new scene is loaded
	// and this object is persistent. This is the virtual stub, but is expanded upon
	// when we reach higher in the hierarchy, e.g. the integrated receiver (which
	// stores which notifications we already subscribed to

	public virtual void resetSIPObject() {
		theNotificationManager = null; // forget my old connection
		resetUUID (); // will cause a re-issue of uuid when we connect to nm
		connectToNotificationManager (); // re-connect
	}

	//
	// connectToNM 
	// try to find the NM prefab and if found, request a uuid token
	// that we use to identify ourselves with.
	//
    public void connectToNotificationManager()
    {
        if (theNotificationManager == null)         {
            GameObject notificationMaster = GameObject.Find("cfxNotificationManager");
			if (notificationMaster == null) {
				Debug.Log ("*** WARNING! No cfxNotificationManager found in scene!");
				return;
			}

            theNotificationManager = notificationMaster.GetComponent<cfxNotificationManager>();
            if (theNotificationManager == null)             {
                Debug.Log("Could not connect to notification Manager. Please ensure that the cfxNotificationManger is placed somewhere in your scene.");
				if (_uuid != null)
					_uuid = null; // forget my last uuid, we need a new one
            } else  {
                if (verbose) Debug.Log("cfxNotificationManager: connected.");
				_uuid = theNotificationManager.uuidSIP ();
            }
        }  else  {
            if (verbose) 
				Debug.Log("cfxNotificationManager: using pre-established connection");
			if (_uuid == null)
				_uuid = theNotificationManager.uuidSIP ();
        }
    }

	//
	// suspending and resuming the entire notification manager
	//
	// WARNING!!!!
	// this will suspend and resume the notifcation manager in its entirety. 
	// to suspend this object/script, choose setActivate or enable/disable
	//
	public void suspendAllNotifications(){
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null)
			return; // two strikes, you are out!

		theNotificationManager.suspendNotifications ();
	}

	//
	// find out if NM is suspended
	//
	public bool notificationsAreSuspended() {
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null) {
			Debug.Log ("WARNING: notificationsAreSuspended() returns TRUE because Notification Manager was not found!");
			return true; // we return true because the notification manager is not in the scene
		}
		
		return theNotificationManager.suspended ();
	}

	//
	// resume notification, and filter queue
	//
	public void resumeNotifications(bool discardAll) {
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null)
			return; // two strikes, you are out!
		
		theNotificationManager.resumeNotifications (discardAll);
	}

	public void resumeNotifications() {
		resumeNotifications(false);
	}


	public void resumeNotifications(string discardNotificationNamed) {
		List<string> nameList = new List<string> ();
		nameList.Add (discardNotificationNamed);
		resumeNotifications (nameList);
	}


	public void resumeNotifications(List<string> discardNotificationsNamed) {
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null)
			return; // two strikes, you are out!

		theNotificationManager.resumeNotifications (discardNotificationsNamed);
	}

	//
	// get a list of all pending notifications
	//
	public List<string> pendingNotificationNames (){
		return pendingNotificationNames (true); // per default we return a unique list
	}

	public List<string> pendingNotificationNames(bool unique) {
		if (theNotificationManager == null)
			connectToNotificationManager ();

		if (theNotificationManager == null)
			return new List<string>(); 

		return theNotificationManager.pendingNotificationNames (unique);

	}
		

	//
	// we set OnDestroy behavior to disco from NM as to
	// no longer be called 
	//
	public virtual void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded; // to prevent SceneManager to goof
		if (theNotificationManager != null) {
			// tell NM that we are going away
			theNotificationManager.removeAllSubscriptions(uuid);
			theNotificationManager.removeAllQueryResponsesFor(uuid);
			if (verbose) 
				Debug.Log ("removed all subs before destroying " + uuid);
			uuid = null;
		}
	}

	public virtual void Awake() {
		// virtual only, so we can override across the whole 
		// tree
	}

	public virtual void NewSceneStart() {
		// you should subscribe all your stuff again

	}

	public virtual void NewSceneDelayedStart() {
		// all the notifications you need to send out again

	}

	private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
	{
		if (this.gameObject == null) // make sure we aren't a ghost
			return;
		
		// called in a scene after all awake are called 
		if (verbose)
			Debug.Log("undestroyed object detects new scene load and initiates NewSceneStart() and NewSceneDelaedStart()");
		resetSIPObject (); // forget all we knew about notifications and connect anew
		NewSceneStart(); // do this now
		StartCoroutine ("invokeNewSceneDelayedStart"); // do this after all start are finished
	}

	public virtual void Start() {
		// install hooks to be called when a new scene is loaded
		// and we are on a do not destroy on load object
		SceneManager.sceneLoaded += OnSceneLoaded; // this will be executed whenever a scene is loaded

		// provide functionality for DelayedStart.
		// we do this by invoking a Co-routine once, that will
		// execute after FixedUpdate
		StartCoroutine ("invokeDelayedStart");
	
	}

	public virtual void DelayedStart() {
		// virtual only so you can override me to 
		// send your notifications and run queries safely just a tad after
		// all start messages have been invoked

		// NOTE: EXECUTION TIME OF DELAYED START
		// =====================================
		//
		// DelayedStart will be invoked after FixedUpdate but before Update
		// it will be vinvoked exactly once
		// it will only be invoked if you call base.Start() in your script.
	}

	IEnumerator invokeDelayedStart() {
		if (verbose) Debug.Log ("DelayedStart() scheduled for this script");
		yield return new WaitForFixedUpdate();
		if (verbose) Debug.Log("Invoking DelayedStart()");
		DelayedStart ();
	}

	IEnumerator invokeNewSceneDelayedStart() {
		if (verbose) Debug.Log("NewSceneDelayedStart() scheduled for this script");
		yield return new WaitForFixedUpdate();
		if (verbose) Debug.Log("Invoking NewSceneDelayedStart()");
		NewSceneDelayedStart();
	}
}

