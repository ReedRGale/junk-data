using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waitforakey : cfxNotificationIntegratedReceiver {

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("jumpPressed");
	}
	
	public override void OnNotification (string notificationName)
	{
		Debug.Log("Jump was pressed.");
	}
}
