using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uniqueExample : cfxNotificationIntegratedReceiver {

	private Rigidbody rb;
	private float delay;

	// Use this for initialization
	public override void Start () {
		base.Start ();
		rb = this.gameObject.GetComponent<Rigidbody> ();
		subscribeTo (UniqueNotificationName ("jump"));	
		delay = Random.Range (1.5f, 4.0f);
		postNotification (UniqueNotificationName ("jump"), delay);
	}

	public override void OnNotification (string notificationName)
	{
		if (notificationName == UniqueNotificationName ("jump")) {
			// jump
			rb.AddForce (Vector3.up * 400f);

			// jump again in a random amount of time
			delay = Random.Range (1.5f, 8.0f);
			postNotification (UniqueNotificationName ("jump"), delay);
			return;
		}
	}
	
}
