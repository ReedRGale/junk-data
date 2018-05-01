using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// Improved cfx Tag System
// 
// Copyright (C) 2017 by cf/x AG and Christian Franz
//
// This Tag system uses a List of strings instead of a single string
// Note that this tag Prefab *CAN* be mixed with the old tag cfx system in the
// same scene and poduce correct answers
//

public class cfxTagSystem2 : cfxNotificationAgent {

	public string queryName = "Tags";
	public List<string>  tags;
	public bool caseInsensitive;

	// Use this for initialization
	public override void Start() {
		// call SIP's Start()
		base.Start ();
		respondToQueryNamed (queryName);
	}

	public override object OnQuery (string queryName, string lookFor)
	{
		List<string> currentTags = new List<string> (tags);
		if (caseInsensitive) {
			currentTags = new List<string> ();
			foreach (string aTag in tags) {
				currentTags.Add (aTag.ToUpper ());
			}	
			lookFor = lookFor.ToUpper ();
		}

		foreach (string aTag in currentTags) {
			if (aTag == lookFor) {
				if (verbose)
					Debug.Log ("Will return GAMEOBJECT for Query " + queryName + " with LookFor = <" + lookFor + ">.");
				return gameObject;
			}
		}
		if (verbose)
			Debug.Log ("Will return NULL for Query " + queryName + " with LookFor = <" + lookFor + ">.");

		return null;
	}
}
