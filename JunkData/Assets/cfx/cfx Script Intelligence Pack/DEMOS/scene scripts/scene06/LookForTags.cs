using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookForTags : cfxNotificationIntegratedReceiver {

	public List<GameObject> foundObjects = new List<GameObject>();
	public string lookForTag = "Destroyable";

	private bool hasRun = false;

	
	// Update is called once per frame
	void Update () {
		if (!hasRun) {
			List<object> queryResponse = runQuery ("HasTag", lookForTag);
			foreach (object anObject in queryResponse) {
				foundObjects.Add (anObject as GameObject);
			}

			Debug.Log ("Found and connected to " + queryResponse.Count + " Objects from Query looking for Tag <"+ lookForTag + ">");
			hasRun = true;
		}	
	}
}
