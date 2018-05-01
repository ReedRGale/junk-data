using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speakerofthehouse : cfxNotificationIntegratedReceiver {

	public AudioClip monkey1;
	public AudioClip monkey2;

	public AudioSource theSource;

	private AudioClip playing;

	public override void Start() {
		base.Start ();
		subscribeTo ("AudioSourceChanged");	
		// now start the first clip
		theSource.PlayOneShot (monkey1);
		playing = monkey1;
	}
	
	public override void OnNotification (string notificationName) {
		Debug.Log ("End play detected");
		// Ping-Pong between clips
		AudioClip nextClip;
		if (playing == monkey1) {
			nextClip = monkey2;
		} else {
			nextClip = monkey1;
		}
		theSource.PlayOneShot (nextClip);
		playing = nextClip;
	}
}
