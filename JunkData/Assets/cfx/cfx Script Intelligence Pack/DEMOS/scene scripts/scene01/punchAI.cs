using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// this script controls the unfriendly Kyle. When he receives
// the 'complaining done' notification, he throws a punch by
// setting the trigger in his own animator, and then sending the "thrwonPunch" notification
//

public class punchAI : cfxNotificationIntegratedReceiver {

	// Use this for initialization
	public override void Start() {
		base.Start ();
		// the notifier script in the animator will send this event
		// when the model completes the 'Angry' animation
		subscribeTo ("finishedComplaining");		
	}

	public override void OnNotification (string notificationName, Dictionary<string, object> info)
	{
		// tell my animator to start the 'punch' animation
		Animator anim = gameObject.GetComponent<Animator> ();
		anim.SetTrigger ("punch");

		// send out notification that I just have thrown a punch
		sendNotification ("thrownPunch");

	}

}
