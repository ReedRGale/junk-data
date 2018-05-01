using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class complainAI : cfxNotificationIntegratedReceiver {

	// Use this for initialization
	public override void Start() {
		base.Start ();
		// the other AI will send 'thrownPunch' when it starts the
		// punch animation
		subscribeTo ("thrownPunch");
	}
	

	public override void OnNotification (string notificationName)
	{
		// OK, the one is throwing a punch. Let
		// our Animator know so we initiated the punched animation
		Animator anim = gameObject.GetComponent<Animator> ();
		anim.SetTrigger ("punched");
	}
}
