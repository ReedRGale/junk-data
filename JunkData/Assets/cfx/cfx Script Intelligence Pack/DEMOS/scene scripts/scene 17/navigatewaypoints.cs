using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // navmeshagent is in here

public class navigatewaypoints : cfxNotificationIntegratedReceiver {

	public List<GameObject> waypoints = null;
	public bool randomize = false;
	public  NavMeshAgent agent; // remember to add UnityEngine.AI to using clause!

	// Use this for initialization
	public override void Start () {
		base.Start ();// if we don't call this, no delayed start!
		agent = GetComponent<NavMeshAgent>();
		subscribeTo ("waypointReached");
	}

	public override void DelayedStart() {
		// Delayed start so all wp have time to sign up for query
		List<object> queryResults = runQuery ("waypoint"); // ask all waypoints to call in
		waypoints = convertToGameObjects (queryResults, true);

		// access the list of waypoints, and start towards the first
		GameObject theWaypoint = waypoints[0];
		agent.SetDestination (theWaypoint.transform.position);
	}

	public override void OnNotification (string notificationName, GameObject theObject) {
		int newIndex = 0, 
			maxNum = waypoints.Count, 
		    currentIndex = waypoints.IndexOf (theObject);
		if (randomize) {
			if (maxNum > 1) {
				do {
					// we simply pick a new waypoint
					newIndex = Random.Range (0, maxNum);
				} while (newIndex == currentIndex);
			}
		} else {
			newIndex = currentIndex + 1;
			if (newIndex >= waypoints.Count) {
				newIndex = 0;
			}
		}
		GameObject theWaypoint = waypoints [newIndex];
		agent.SetDestination (theWaypoint.transform.position);
	}



}
