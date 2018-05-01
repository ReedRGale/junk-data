using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * cf/x SIP - Query Manager
 *
 * Copyright (C) 2017 by cf/x Ag and Christian Franz
 *
 * Query Manager Response Section:
 * OnQuery and RunQuery amenities
 * 
 */
public class cfxNetworkedIntegratedQuery : cfxNetworkedNotificationSender {

	/*
     * MAKE SURE YOU OVERRIDE ONE AND ONLY ONE OF THE FOLLOWING IN YOUR CODE BY DEFINING YOUR OWN.
     * If not, the OnQuery method directly below will be called and produce a warning in the log
     */

	public virtual object OnQuery(string queryName) {
		// if we enter here, you have forgotten to override one of the three OnQuery
		Debug.Log("Please override OnQuery in your script - Query" + queryName + " ignored.");
		return null;
	}

	public virtual object OnQuery(string queryName, string lookFor) {
		return OnQuery (queryName);
	}

	public virtual object OnQuery(string queryName, Dictionary<string, object> info){
		string lookFor = null;
		if (info.ContainsKey ("LookFor"))
			lookFor = info ["LookFor"] as string;

		if (lookFor != null) {
			// we detour to 'OnQuery with lookfor"
			return OnQuery (queryName, lookFor);
		} else {
			return OnQuery (queryName);

		}
	}

	public virtual object OnQuery (string queryName, string lookFor, Dictionary <string, object> info) {
		return OnQuery (queryName, info);
	}

	// here is the entry point for the OnInfo chain
	public virtual object OnQuery(Dictionary<string, object> info) {
		string queryName = "cfxGenericQuery";
		string lookFor = null;

		if (info.ContainsKey ("Query"))
			queryName = info ["Query"] as string;

		if (info.ContainsKey ("LookFor"))
			lookFor = info ["LookFor"] as string;

		return OnQuery (queryName, lookFor, info);


	}

	//
	// Main OnQuery Entry point when signed up for 
	// queries follows below
	//

	private object QueryEntryPoint(Dictionary<string, object> info) {
		// this is the entry point for all OnXXXX queries, filtering 
		// can be implemented here

		// now call convenience chain
		return OnQuery (info);
	}

	//
	// Signing up to a query
	//

	public void respondToAllQueries() {
		respondToQueryNamed ("*");
	}

	public void respondToQueryNamed(string queryName) {
		respondToQueryNamed (queryName, QueryEntryPoint);
	}

	public void respondToQueryNamed(string queryName, cfxNotificationManager.queryHandler handler) {
		if (theNotificationManager == null)
			connectToNotificationManager ();
		if (theNotificationManager == null) {
			if (verbose)
				Debug.Log ("Cannot sign up for queries: Notification Manager not found.");
			return;
		}
		theNotificationManager.signUpToAnswerQuery (queryName, handler, uuid);
	}

	//
	// resigning from answering
	//
	public void withdrawAllQueryResponses() {
		if (theNotificationManager == null)
			connectToNotificationManager ();
		if (theNotificationManager == null) {
			if (verbose)
				Debug.Log ("Cannot withdraw from queries: Notification Manager not found.");
			return;
		}
		theNotificationManager.removeAllQueryResponsesFor (uuid);
	}

	public void withdrawQueryResponseFor (string queryName) {
		withdrawQueryResponseFor (queryName, QueryEntryPoint);
	}
		
	public void withdrawQueryResponseFor(string queryName, cfxNotificationManager.queryHandler handler) {
		if (theNotificationManager == null)
			connectToNotificationManager ();
		if (theNotificationManager == null) {
			if (verbose)
				Debug.Log ("Cannot withdraw from query " + queryName + ": Notification Manager not found.");
			return;
		}
		theNotificationManager.removeAnswerForQuery (queryName, handler, uuid);
	}

	//
	// running a query
	// Convenience method chain.
	//
	public List<object> runQuery(string queryName) {
		if (queryName.Length < 1)
			return new List<object>();
		Dictionary<string, object> info = new Dictionary<string, object> ();
		info ["Query"] = queryName;
		return runQuery (queryName, info);
	}

	public List<object> runQuery(string queryName, string lookFor) {
		if (queryName.Length < 1)
			return new List<object>();
		Dictionary<string, object> info = new Dictionary<string, object> ();
		info ["Query"] = queryName;
		info ["LookFor"] = lookFor;
		return runQuery (queryName, info);

	}
	public List<object> runQuery(string queryName, Dictionary<string, object> info) {
		if (queryName.Length < 1)
			return new List<object>();

		List<string> discarded = null;
		return runQuery (queryName, info, out discarded);
	}

	public List<object> runQuery(string queryName, Dictionary<string, object> info, out List<string> responders) {
		responders = null; // set <out> parameter now just in case we need to exit early
		if (queryName.Length < 1)
			return new List<object>();
		if (theNotificationManager == null)
			connectToNotificationManager ();
		if (theNotificationManager == null) {
			if (verbose)
				Debug.Log ("Cannot connect to the notification manager. Query " + queryName + " Aborted");
			return new List<object>();
		}

		if (info == null) info = new Dictionary<string, object>();
		info ["Query"] = queryName;

		Dictionary<string,object> rawResults = theNotificationManager.submitQuery (queryName, info);
		responders = new List<string> (rawResults.Keys); 

		return new List<object>(rawResults.Values);
	}

	//
	// Now follow the scoped variants
	//
	public List<object> runQuery(string queryName, List<string> scope) {
		Dictionary<string, object> info = new Dictionary<string, object> ();
		info ["Query"] = queryName;
		return runQuery (queryName, info, scope);
	}

	public List<object> runQuery (string queryName, out List<string> responders) {
		List<string> scope = null;
		return runQuery (queryName, scope, out responders);
	}

	public List<object> runQuery(string queryName, List<string> scope, out List<string> responders) {
		Dictionary<string, object> info = new Dictionary<string, object> ();
		info ["Query"] = queryName;

		List<string> whiteList = null;
		if (scope != null) {
			whiteList = new List<string> (scope); // create a copy	
		}

		responders = null; // this would blow the scope to smitherenes if scope === responders (due to the OUT keyword)
		// do the qerry
		Dictionary<string,object> rawResults = theNotificationManager.submitQuery (queryName, info, whiteList);
		// split out responders
		responders = new List<string> (rawResults.Keys); 
		// return the values
		return new List<object>(rawResults.Values);
	}
		
	public List<object> runQuery(string queryName, string lookFor, List<string> scope) {
		Dictionary<string, object> info = new Dictionary<string, object> ();
		info ["Query"] = queryName;
		info ["LookFor"] = lookFor;
		return runQuery (queryName, info, scope);
	}

	public List<object> runQuery(string queryName, string lookFor, out List<string> responders) {
		return runQuery (queryName, lookFor, null, out responders); 
	}

	public List <object> runQuery(string queryName, string lookFor, List<string> scope, out List<string> responders) {
		Dictionary<string, object> info = new Dictionary<string, object> ();
		info ["Query"] = queryName;
		info ["LookFor"] = lookFor;

		List<string> whiteList = null;
		if (scope != null) {
			whiteList = new List<string> (scope); // create a copy	
		}

		responders = null; // this would blow the scope to smitherenes if scope === responders (due to the OUT keyword)
		// do the qerry
		Dictionary<string,object> rawResults = theNotificationManager.submitQuery (queryName, info, whiteList);
		// split out responders
		responders = new List<string> (rawResults.Keys); 
		// return the values
		return new List<object>(rawResults.Values);

	}

	public List<object> runQuery(string queryName, Dictionary<string, object> info, List<string> scope) {
		List<string> discarded = null;
		return runQuery (queryName, info, scope, out discarded);
	}

	public List<object> runQuery(string queryName, Dictionary<string, object> info, List<string> scope, out List<string> responders) {
		// it's entirely possible, even likely, that smart coders pass the same variable for both scope
		// and responders, to iteratively narrow the scope. To guard against this, we copy 'scope' into 
		// a nicely named 'whitelist' local variable
		List<string> whiteList = null;
		if (scope != null) {
			whiteList = new List<string> (scope); // create a copy	
		}

		responders = null; // this would blow the scope to smitherenes if scope === responders (due to the OUT keyword)

		// sanity checks
		if (queryName.Length < 1)
			return new List<object>();
		if (theNotificationManager == null)
			connectToNotificationManager ();
		if (theNotificationManager == null) {
			if (verbose)
				Debug.Log ("Cannot connect to the notification manager. Query " + queryName + " Aborted");
			return new List<object>();
		}

		// sanitize info, add query no matter what
		if (info == null) info = new Dictionary<string, object>();
		info ["Query"] = queryName;

		// do the qerry
		Dictionary<string,object> rawResults = theNotificationManager.submitQuery (queryName, info, whiteList);
		// split out responders
		responders = new List<string> (rawResults.Keys); 
		// return the values
		return new List<object>(rawResults.Values);
	}


	//
	// unofficial runQueries that return the very first item in the arrayy as various objects
	//

	public object runQueryAndFetchFirst(string queryName, string lookFor) {
		List<object> theObjects = runQuery (queryName, lookFor);
		if (theObjects.Count > 0) 
			return theObjects [0];
		return null;
	}
		

	public object runQueryAndFetchRandom(string queryName, string lookFor) {
		List<object> theObjects = runQuery (queryName, lookFor);
		if (theObjects.Count > 0) {
			int index = Random.Range (0, theObjects.Count);
			return theObjects [index];
		}
		return null;
	}


	public object runQueryAndFetchFirst(string queryName, string lookFor, Dictionary<string, object> info) {
		if (info == null)
			info = new Dictionary<string, object> ();
		info ["LookFor"] = lookFor;
		List<object> theObjects = runQuery (queryName, info);
		if (theObjects.Count > 0) 
			return theObjects [0];
		return null;
	}

	public object runQueryAndFetchRandom(string queryName, string lookFor, Dictionary<string, object> info){
		if (info == null)
			info = new Dictionary<string, object> ();
		info ["LookFor"] = lookFor;
		List<object> theObjects = runQuery (queryName, info);
		if (theObjects.Count > 0) {
			int index = Random.Range (0, theObjects.Count);
			return theObjects [index];
		}
		return null;
	}

	//
	// as GameObject

	public GameObject runQueryAndFetchFirstGameObject(string queryName, string lookFor) {
		return runQueryAndFetchFirst (queryName, lookFor) as GameObject;	
	}
		
	public GameObject runQueryAndFetchRandomGameObject(string queryName, string lookFor) {
		return runQueryAndFetchRandom (queryName, lookFor) as GameObject;
	}

	public GameObject runQueryAndFetchFirstGameObject(string queryName, string lookFor, Dictionary<string, object> info) {
		return runQueryAndFetchFirst (queryName, lookFor, info) as GameObject;
	}

	public GameObject runQueryAndFetchRandomGameObject(string queryName, string lookFor, Dictionary<string, object> info){
		return runQueryAndFetchRandom (queryName, lookFor, info) as GameObject;
	}

	//
	// as String
	//

	public string runQueryAndFetchFirstString(string queryName, string lookFor) {
		return runQueryAndFetchFirst (queryName, lookFor) as string;	
	}

	public string runQueryAndFetchRandomString(string queryName, string lookFor) {
		return runQueryAndFetchRandom (queryName, lookFor) as string;
	}

	public string runQueryAndFetchFirstString(string queryName, string lookFor, Dictionary<string, object> info) {
		return runQueryAndFetchFirst (queryName, lookFor, info) as string;
	}

	public string runQueryAndFetchRandomString(string queryName, string lookFor, Dictionary<string, object> info){
		return runQueryAndFetchRandom (queryName, lookFor, info) as string;
	}

	//
	// WhoIs handling. This is installed in Start, meaning that if you do not use override in your
	// Start(), you will not have access to this.
	//
	// install the whoIs responder
	public override void Start () {
		base.Start ();
		respondToQueryNamed ("cfxWhoIsUUIDQuery", myWhoIsHandler);
	}


	public GameObject whoIs(string who) {
		return runQueryAndFetchFirstGameObject ("cfxWhoIsUUIDQuery", who);
	}

	public string whoAmI() {
		//
		// note: 
		// Due to the way we have an accessor to uuid, we don't really need below
		// code. But the way we can log errors makes the code worth it.
		//
		if (theNotificationManager == null)
			connectToNotificationManager ();
		if (theNotificationManager == null) {
			if (verbose)
				Debug.Log ("Cannot connect to the notification manager to retrieve WhoAmI");
			return null;
		}
		return uuid;
	}

	public object myWhoIsHandler(Dictionary<string, object> queryInfo) {
		// we know that we can only get here if the query name is cfxWhoIsUUIDQuery. Anyway, place a guard
		if (!queryInfo.ContainsKey ("Query")) return null;
		string queryName = queryInfo ["Query"] as string;
		if (queryName == "cfxWhoIsUUIDQuery") {
			if (queryInfo.ContainsKey ("LookFor"))
				return null;
			string theUUID = queryInfo ["LookFor"] as string;
			if (theUUID == uuid)
				return gameObject;
		}
		return null;
	}

	//
	// OnDestroy: clean-up before leaving to make sure
	// the notification mananger will noty and invoke 
	// us 
	//
	public override void OnDestroy() {
		if (theNotificationManager != null) {
			withdrawAllQueryResponses ();
		}
		base.OnDestroy ();
	}

}
