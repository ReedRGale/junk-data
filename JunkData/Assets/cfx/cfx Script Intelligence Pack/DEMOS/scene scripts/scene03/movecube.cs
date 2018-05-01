using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movecube : cfxNotificationIntegratedReceiver {

	public float speed;

	private Vector3 lastPos;

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("collisionDetected");	
		lastPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		lastPos = transform.position;
		transform.position += transform.forward * Time.deltaTime * speed;

	}

	public override void OnNotification (string notificationName)
	{
		// turn by 180 degrees
//		speed = -speed;
		transform.position = lastPos; // last before impact
		transform.Rotate(0f, 90f, 0f);
	}
}
