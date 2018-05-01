using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpKyle : cfxNotificationIntegratedReceiver {

	public string listenTo = "Jump!";

	public Animator theAnimator;

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo (listenTo);
	}
	

	public override void OnNotification (string notificationName)
	{
		theAnimator.SetTrigger ("jump");
	}

}
