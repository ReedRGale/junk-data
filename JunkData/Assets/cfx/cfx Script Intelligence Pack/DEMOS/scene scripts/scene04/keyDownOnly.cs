using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyDownOnly : cfxNotificationIntegratedReceiver {

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("Input");
		filterUsing = cfxFilterMethod.WhiteList; // use whitelist
		filterEventForNotification ("KeyDown", "Input"); // only allow "KeyDown" 
	}
	
	public override void OnNotification (string notificationName, string theEvent)
	{
		// should only receive "KeyDown" as theEvent
		Debug.Log ("Received event: " + theEvent);
	}
}
