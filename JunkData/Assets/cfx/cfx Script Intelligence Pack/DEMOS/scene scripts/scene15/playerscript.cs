using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class playerscript : cfxNetworkedNotificationIntegratedReceiver {

	// Use this for initialization
	public override void  Start () {
		base.Start();
		subscribeTo ("localNotification");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnNotification (string notificationName) {
		Debug.Log ("Received Notification: " + notificationName);
	}
}
