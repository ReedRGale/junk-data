using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatingPing : cfxNotificationIntegratedReceiver {

	public float intervalInSeconds = 5.0f;
	public int repeats =-1; // forever
	public string thePingName = "Ping";
	public string startNotificationCommand = "startPingXYZ";
	public string stopNotificationCommand = "stopPingXYZ";
	public bool startImmediately = true;
	public bool delayBeforeFirstPing = true;

	private int runRepeats = 0; // so we can repeatedly start the ping counter without need for re-set


	// Use this for initialization
	public override void Start() {
		// call SIP's Start()
		base.Start ();
		if (startNotificationCommand.Length > 0) subscribeTo(startNotificationCommand);
		if (stopNotificationCommand.Length > 0) subscribeTo(stopNotificationCommand);
		runRepeats = repeats;
		if (startImmediately) {
			if (delayBeforeFirstPing) {
				Invoke ("initiator", intervalInSeconds);
			} else {
				StartCoroutine ("Ping");
			}
		}
	}
	

	 
	public override void OnNotification(string aNotificationName, string theEvent) {
		if (aNotificationName == startNotificationCommand) {
			runRepeats = repeats;
			if (delayBeforeFirstPing) {
				Invoke ("initiator", intervalInSeconds);
			} else {
				StartCoroutine ("Ping");
			}
		}
		 
		if (aNotificationName == stopNotificationCommand) {
			runRepeats = 0; // will cause the next resume to time out
			if (theEvent == "FullStop") {
				StopAllCoroutines ();
			}
		}
	}

	public void initiator(){
		// we need this silly step to ensure that we have a delay before the very 
		// first 'Ping'
		StartCoroutine("Ping");
	}

	IEnumerator Ping() {
		while (runRepeats != 0) { // little dirty trick: values <0 will repeat endlessly
			sendNotification(thePingName);
			if (verbose)
				Debug.Log ("Ping Fired");
			
			yield return new WaitForSeconds(intervalInSeconds);
			if (runRepeats >= 0) {
				runRepeats = runRepeats - 1;
			}
		}
	}
	 

}
