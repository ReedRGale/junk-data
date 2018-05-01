using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cfxNotificationSMBSender : cfxNotificationSMBAgent
{

	// this is basically a verbatim copy of the cfxNotificationSender, curtailed for SMB.
	// there are the following limits:
	//  - No GameObject available
	//  - No local scheduling because coroutines are not allowed

	private void post(string theNotificationName, Dictionary<string, object> theDict, float delay)
	{
		// sanity
		if (this == null) {
			Debug.Log ("Warning: post with NULL script instance. The parenting object was destroyed!");
			return;
		}

		// Resilience:
		// if we are not connected, try once to re-connect. The original cNM may have gone
		// off-line or been switched, so try to re-establish a connection
		if (theNotificationManager == null) connectToNotificationManager ();

		// sanity check
		if (theNotificationManager == null) return;

		// validation of theDict
		if (theDict == null) theDict = new Dictionary<string, object>();

		// validation of notificationName
		if ((theNotificationName == null) || (theNotificationName.Length < 1))  {
			theNotificationName = "cfxGenericNotification";
			if (verbose) Debug.Log("notification name set to" + theNotificationName);
		}

		// ensure that theDict has correct notificationName and minimal data
		theDict ["NotificationName"] = theNotificationName;
//		theDict ["GameObject"] = gameObject;
		theDict["Time"] = Time.time.ToString();

		// reporting time if enabled: dump info dict. Great for debugging
		if (verbose) {
			Debug.Log("Posting Notification with info dict containing:");
			foreach (string theKey in theDict.Keys)  {
				Debug.Log(" --> " + theKey + " : " + theDict[theKey].ToString());
			}
			Debug.Log(" --> NotificationName : " + theNotificationName);
		}


		// handle delay explicitly, make too short delay to execute immediately instead of deferred
		if (delay < 0.01) {
			theNotificationManager.postNotification (theNotificationName, theDict);
		} else {
			theDict["Scheduled"] = "Central"; // Mark this as a sheduled central notification
			theNotificationManager.postDelayedNotification (theNotificationName, theDict, delay);
		}

	}


	public void postNotification(string theNotificationname) {
		Dictionary<string, object> theDict = getBasicInfo(theNotificationname);
		post(theNotificationname, theDict, 0f);
	}

	public void sendNotification(string theNotificationName) {
		postNotification(theNotificationName);
	}

	public void postNotification(string theNotificationName, Dictionary<string, object> theDict)
	{
		addBasicInformation(theDict);
		post(theNotificationName, theDict, 0f);
	}

	public void sendNotification(string theNotificationName, Dictionary<string, object> theDict) {
		postNotification(theNotificationName, theDict);
	}

	public void postNotification(string theNotificationName, string theEvent) {
		Dictionary<string, object> theDict = getBasicInfo(theEvent);
		postNotification(theNotificationName, theDict);
	}

	public void sendNotification(string theNotificationName, string theEvent) {
		Dictionary<string, object> theDict = getBasicInfo(theEvent);
		sendNotification(theNotificationName, theDict);
	}

	public void postNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict)
	{
		if (theEvent != null) theDict["theEvent"] = theEvent;
		postNotification(theNotificationName, theDict);
	}

	public void sendNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict)
	{
		if (theEvent != null) theDict["theEvent"] = theEvent;
		sendNotification(theNotificationName, theDict);
	}

	//
	// The full set with notification delay
	//
	public void postNotification(string theNotificationname, float delay) {
		Dictionary<string, object> theDict = getBasicInfo(theNotificationname);
		post(theNotificationname, theDict, delay);
	}

	public void sendNotification(string theNotificationName, float delay) {
		postNotification(theNotificationName, delay);
	}

	public void postNotification(string theNotificationName, Dictionary<string, object> theDict, float delay)
	{
		addBasicInformation(theDict);
		post(theNotificationName, theDict, delay);
	}

	public void sendNotification(string theNotificationName, Dictionary<string, object> theDict, float delay) {
		postNotification(theNotificationName, theDict, delay);
	}

	public void postNotification(string theNotificationName, string theEvent, float delay) {
		Dictionary<string, object> theDict = getBasicInfo(theEvent);
		postNotification(theNotificationName, theDict, delay);
	}

	public void sendNotification(string theNotificationName, string theEvent, float delay) {
		Dictionary<string, object> theDict = getBasicInfo(theEvent);
		sendNotification(theNotificationName, theDict, delay);
	}

	public void postNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict, float delay)
	{
		if (theEvent != null) theDict["theEvent"] = theEvent;
		postNotification(theNotificationName, theDict, delay);
	}

	public void sendNotification(string theNotificationName, string theEvent, Dictionary<string, object> theDict, float delay)
	{
		if (theEvent != null) theDict["theEvent"] = theEvent;
		sendNotification(theNotificationName, theDict, delay);
	}


}
