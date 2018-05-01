using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * Copyright (C) 2017, 2018 by cf/x AG and Christian Franz
 * 
 * This is the Notifification Receiver root class
 * that you can use to gain immediate access to connecting to the 
 * notification manager 
 * 
 * Use this class if you want to implement notification hadling separately yourself
 * for each notification, or use the convenience class 'cfxNotificationIntegratedReceiver' 
 * for a more convenient OnNotification event that you can use centrally
 * 
 * It automatically connects to the cfxNotificationManager during Awake, so
 * if you override Awake, make sure you call the inherited Awake, or call
 * connectToNotificationManger yourself.
 *
 */

public class cfxNetworkedNotificationReceiver : cfxNetworkedNotificationBehaviour {

    //
    // calling subscribeTo with your handler will pass it to the notificationhandler
    // but not add it to the filterable events. Instead, your own handler will be called.
    //
    public void subscribeTo(string notificationName, cfxNotificationManager.notificationHandler theHandler)
    {
		// make sure there is a notification manager
		if (theNotificationManager == null) connectToNotificationManager ();
		if (theNotificationManager == null) {
			if (verbose)
				Debug.Log ("Unable to subscribe to " + notificationName + ": no Manager");
			return;
		}

		theNotificationManager.subscribeTo(notificationName, theHandler, uuid);
		if (verbose) Debug.Log ("Subscribed to " + notificationName);
    }

}
