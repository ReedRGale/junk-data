using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class localsender : cfxNotificationMPTransponder {


	public override void Start () {
		base.Start ();
		doSubscriptions();	
	}

	public override void DelayedStart(){
		doNotifications ();
	}

	public override void NewSceneStart() {
		// we are persistent, and a new scene was loaded
		doSubscriptions();
	}

	public override void NewSceneDelayedStart(){
		sendNotification ("Ready");
	}

	void doSubscriptions() {
		subscribeTo ("NetTest");	
	}

	void doNotifications() {
		sendNotification ("Ready");
	}


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space")) {
			Dictionary<string, object> info = getBasicInfo ("lets see if this works");
			info ["somevalue"] = "Can you read this?";
			info ["look at me"] = "this is another string sent over the network";
			netSendNotification ("NetTest", "Eventname", 1.4f);
		}
	}

	void dumpDict(Dictionary<string, object> info){
		foreach (string key in info.Keys) {
			if (info [key] is string) {
				Debug.Log ("Info[" + key + "] = " + info [key]);
			} else {
				Debug.Log ("Info[" + key + "] = (not a string)");
			}
		}
	}


	public override void OnNotification (string notificationName, Dictionary<string, object> info)
	{
		Debug.Log ("received notification: " + notificationName + " with the following info");
		dumpDict (info);

	}
}
