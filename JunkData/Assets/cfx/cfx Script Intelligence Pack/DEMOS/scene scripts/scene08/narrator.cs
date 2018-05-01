using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class narrator : cfxNotificationIntegratedReceiver {


	public AudioClip intro;
	public AudioClip jump;

	public AudioSource theSource;


	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("DoneSpeaking");
		subscribeTo ("Ping!");
		theSource.PlayOneShot (intro);



	}
	

	public override void OnNotification (string notificationName)
	{
		if (notificationName == "DoneSpeaking") {
			unsubscribeFrom ("DoneSpeaking"); // will no longer be called when audio finishes
			//theNotificationManager.removeAllSubscriptions (this);

			sendNotification ("startPing");
			Debug.Log ("return sendNotification");
		}

		if (notificationName == "Ping!") {
			sendNotification ("Jump!");
			theSource.PlayOneShot (jump);
		}
	}
}
