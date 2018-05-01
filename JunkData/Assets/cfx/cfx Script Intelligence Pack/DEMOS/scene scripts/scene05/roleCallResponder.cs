using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class roleCallResponder : cfxNotificationIntegratedReceiver {

	// Use this for initialization
	public override void Start() {
		base.Start ();
		// sign up for this query
		respondToQueryNamed ("RoleCall");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override object OnQuery (string queryName)
	{
		return gameObject;
	}
}
