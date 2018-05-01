using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inquisitor : cfxNotificationIntegratedReceiver {

	public List<GameObject> foundObjects; // will be filled by script
	private bool hasRun = false;
	// Use this for initialization
	public override void Start() {
		base.Start ();

	}
	
	// Update is called once per frame
	void Update () {
		if (!hasRun) {
			// the rolecall query is implemented in all cubes to respond with their 
			// game object root
			List<object> queryResponse = runQuery ("RoleCall");
			foreach (object anObject in queryResponse) {
				foundObjects.Add (anObject as GameObject);
			}

			Debug.Log ("Found and connected to " + queryResponse.Count + " Objects from Query Result");
			hasRun = true;
		}
	}
}
