using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cfxTagSystem : cfxNotificationAgent {

	public string queryName = "Tags";
	public string tags = "Player, Destroyable, Moving";
	public bool caseInsensitive;
	
	// Use this for initialization
	public override void Start() {
		// call SIP's Start()
		base.Start ();
		respondToQueryNamed (queryName);
	}
	
	public override object OnQuery (string queryName, string lookFor)
	{
		string comparator = tags;
		if (caseInsensitive) {
			comparator = tags.ToUpper ();
			lookFor = lookFor.ToUpper ();
		}
		// this will only be called if queryName matches "Tags" AND there is a "LookFor" string in info
		if (comparator.Contains (lookFor)) {
			// yup, we have this tag, return self
			if (verbose)
				Debug.Log ("Will return GAMEOBJECT for Query " + queryName + " with LookFor = <" + lookFor + ">.");
			return gameObject;
		} else {
			if (verbose)
				Debug.Log ("Will return NULL for Query " + queryName + " with LookFor = <" + lookFor + ">.");
			return null;
		}
	}
}
