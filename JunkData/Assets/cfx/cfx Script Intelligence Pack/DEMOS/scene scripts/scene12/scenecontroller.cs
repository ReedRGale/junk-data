using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scenecontroller : cfxNotificationIntegratedReceiver {

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("Input");		
	}
	
	public override void OnNotification (string notificationName)
	{
		// 1. Start fx fade in
		Dictionary<string, object> info = new Dictionary<string, object> ();
		info ["FadeIn"] = "fadein";
		info ["Duration"] = "0.25";
		sendNotification ("startFX", info);

		// 2. Start fx fade out, same duration. 
		info.Remove ("FadeIn");
		sendNotification ("startFX", info, 1.5f);

		// 3. Stop fx
		sendNotification ("stopFX", 2.0f);

	}
		

}
