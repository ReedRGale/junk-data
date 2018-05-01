using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scenehandler : cfxNotificationIntegratedReceiver {

	public List<string> LookFor;
	public GameObject theCaption;

	private GUIText theGuiText;

	private int index;

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("Ping");
		theGuiText = theCaption.GetComponent<GUIText> ();
	}
	
	//
	// every time we receive a ping, we take the current LookFor
	// word, query for the objects, and give them a kick, then advance
	// the index (and wrap if necessary
	//
	public override void OnNotification (string notificationName)
	{
		if (notificationName != "Ping")
			return;

		string theTagWeAreLookingFor = LookFor [index];
		List<object> objectsReceived = runQuery ("HasTag", theTagWeAreLookingFor);

		theGuiText.text = "Query Tag <" + theTagWeAreLookingFor + ">: " + objectsReceived.Count + " objects.";

		foreach (GameObject anObject in objectsReceived) {
			Rigidbody rb = anObject.GetComponent<Rigidbody> ();
			rb.AddForce (Vector3.up * 400f);
		}

		index = index + 1;
		if (index >= LookFor.Count)
			index = 0;
	}
}
