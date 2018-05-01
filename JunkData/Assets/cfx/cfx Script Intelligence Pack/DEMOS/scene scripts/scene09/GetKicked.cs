using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetKicked : cfxNotificationIntegratedReceiver {

	private Rigidbody rb;

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("Kick");
		rb = GetComponent<Rigidbody> ();
	}
	
	public override void OnNotification (string notificationName)
	{
		rb.AddForce (Vector3.up * 600f);
	}

}
