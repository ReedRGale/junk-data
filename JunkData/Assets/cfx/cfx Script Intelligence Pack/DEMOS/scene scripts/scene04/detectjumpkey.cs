using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detectjumpkey : cfxNotificationIntegratedReceiver {

	// Use this for initialization
	public override void Start() {
		base.Start ();

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton ("Jump")) sendNotification("jumpPressed");
			
	}
}
