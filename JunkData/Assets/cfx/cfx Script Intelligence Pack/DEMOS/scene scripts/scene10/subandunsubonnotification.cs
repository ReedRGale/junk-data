using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class subandunsubonnotification : cfxNotificationIntegratedReceiver {

	public GameObject caption;

	private GUIText theText;
	private bool subbedToOther = false;

	// Use this for initialization
	public override void Start() {
		base.Start ();
		theText = caption.GetComponent<GUIText> ();

		subscribeTo ("Subscribe");
		subscribeTo ("Level2");
		theText.text = "Staring up. Waiting for 'Subscribe'";
	}

	public override void OnNotification (string notificationName)
	{
		if (notificationName == "Subscribe") {
			// we'll re-enter the notifiation manager now
			sendNotification ("Level2");
		}

		if (notificationName == "Level2") {
			// we are a notification inside a notification - the ideal 
			// moment to ask for a subscription or to unsubscribe
			// a lesser notification manager will die now. Painfully.
			if (subbedToOther) {
				Debug.Log ("UNSUBcribing to Other");
				unsubscribeFrom ("Other");
				theText.text = "Unsubscribed from Other";
				subbedToOther = false;
			} else {
				Debug.Log ("Subscribing to Other");
				subscribeTo ("Other");
				theText.text = "Subscribed to Other";
				subbedToOther = true;
			}
		
		}

		if (notificationName == "Other") {
			theText.text = "Received 'Other' within 'Subcribe'";
			Debug.Log ("Other at " + Time.time.ToString ());
		}
	}
	
}
