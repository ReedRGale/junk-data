using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleTags : cfxNotificationIntegratedReceiver {

	public string tags = "red, land, heavy, veteran, mobile, standard";

	public override void Start() {
		// call SIP's Start()
		base.Start ();
		// allow inquiries to my tags
		respondToQueryNamed ("HasTag");
	}

	public override object OnQuery (string queryName, string lookFor)
	{
		// this will only be called if queryName matches "Tags" AND there is a "LookFor" string in info
		if (tags.Contains (lookFor)) {
			// yup, we have this tag, return self
			return gameObject;
		} else {
			// null means nothing to see here, ignore my reply
			return null;
		}
	}


}
