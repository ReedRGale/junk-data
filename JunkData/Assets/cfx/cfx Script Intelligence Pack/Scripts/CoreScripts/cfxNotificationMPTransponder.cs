	using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class cfxNotificationMPTransponder : cfxNotificationIntegratedReceiver {

	/*
	 * Copyright (C) 2018 by Christian Franz and cf/x AG
	 * 
	 * This class adds networked notifications to SIP
	 * 
	 * Notifications sent use the networkedNotificationManager which sits on
	 * top of the notification manager (and is a cfxNotificationIntegratedReceiver class script)
	 * to broadcast the notifications locally and to all CONNECTED clients in the net. If you
	 * are not connected, you also won't send any messages across the net, but the notification is 
	 * still broadcast locally.
	 * 
	 * We get the notification to the networked notification manager with a standard sendNotification call
	 * that masks the original notificationName
	 * 
	 */

	public void netSendNotification(string notificationName) {
		netSendNotification (notificationName, 0.0f);
	}

	public void netSendNotification(string notificationName, float delay) {
		netSendNotification (notificationName, "", delay);
	}

	public void netSendNotification(string notificationName, string theEvent) {
		netSendNotification (notificationName, theEvent, 0.0f);
	}

	public void netSendNotification(string notificationName, string theEvent, float delay) {
		Dictionary<string, object> info = getBasicInfo(theEvent); // save event into dict, together with rest
		netPost (notificationName, info, delay);

	}

	public void netSendNotification(string notificationName, Dictionary<string, object> info) {
		netSendNotification (notificationName, info, 0.0f);
	}
		
	public void netSendNotification(string notificationName, Dictionary<string, object> info, float delay) {
		netPost (notificationName, info, delay);

	}

	public void netSendNotification(string notificationName, string theEvent, Dictionary<string, object> info) {
		netSendNotification (notificationName, theEvent, info, 0.0f);
	}

	public void netSendNotification(string notificationName, string theEvent, Dictionary<string, object> info, float delay) {
		if (theEvent != null) info["Event"] = theEvent;
		netPost (notificationName, info, delay);
	}

	public void netPost(string theNotificationName, Dictionary<string, object> info, float delay) {
		Debug.Log ("Enter netPost with name = " + theNotificationName);

		if (theNotificationName == null) {
			Debug.Log ("SIP: notification name is null in netSendNotification");
			return;
		}
		// mask / switch the notificationName to trigger
		// the networkNotificationManager
		info[cfxSIPConstants.cfxNetMessageRealNotificationName] = theNotificationName; // save this so after net transfer, this can be restored

		postNotification (cfxSIPConstants.cfxNetMessageNotification, info, delay);

	} 
}
