using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class delayedKick : cfxNotificationIntegratedReceiver {
	public float delay = 5f;
	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("Jump Pressed");
	}
	
	public override void OnNotification (string notificationName)
	{
		sendNotification ("Kick", delay);
	}
}
