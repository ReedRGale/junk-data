using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class raceconditioner : cfxNotificationIntegratedReceiver {

	public string subTo;
	public string notifyOf;
	private bool updated = false;

	// Use this for initialization
	public override void Start () { // NOTE OVERRIDE!
		base.Start (); // NOTE BASE.START()!
		subscribeTo (subTo);	
	}

	public override void DelayedStart () {
		sendNotification (notifyOf);
	}

	public override void OnNotification (string notificationName)	{
		Debug.Log ("Received notification: " + notificationName);
	}

	void Update(){
		if (!updated) {
			Debug.Log ("Update Invoked");
			updated = true;
		}
	}
}
