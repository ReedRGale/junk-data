using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waypoint : cfxNotificationIntegratedReceiver {

	public override void Start () {
		base.Start ();
		respondToQueryNamed ("waypoint"); // when navigator calls, we respond
	}
	
 	// our collider tells us that player has come close enough to trigger
	void OnTriggerEnter(Collider other) {
		// since only one thing moves, we know it's the player
		sendNotification("waypointReached", gameObject); // we use the string, GameObject overload
	}


	public override object OnQuery (string queryName, string lookFor, Dictionary<string, object> info) {
		// since we signed up only for one query, return self, no guards, bad example
		return gameObject;
	}
}
