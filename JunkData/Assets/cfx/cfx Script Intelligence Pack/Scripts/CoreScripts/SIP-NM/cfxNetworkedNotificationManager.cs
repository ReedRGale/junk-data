using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

	/*
	 * Part of SIP for Unity, (C) 2018 by Christian Franz and cf/x AG
	 * 
	 * This part of the notification manager is used to broadcast messages across the network
	 * to all clients. 
	 * The cool part is that it itself is is a SIP receiver, that simply sets up an interface to 
	 * connect and send/receive messages to everyone on the network.
	 * 
	 * Notifications received are sent as local boardcasts to anyone who has subscribed to the 
	 * relevant notification
	 * 
	 * The networked notification manager uses standard notification manager methods and merely uses
	 * a specific named notification name to receive, and then translate, notifications destined
	 * for the network.
	 * 
	 * The only thing special about this class is that it sets itself up as a client to receive
	 * messages from the network
	 * 
	 * 
	 * The message ID we use internally (if you use your own messages) is 2000
	 * 
	 * Networking layer is Unity's UNet. To use SIP in a network environment, your NetworkManager
	 * (or NetworkLobbyManager must be SIP-enabled, and the connection to the other clients must
	 * have been made through that Manager
	 */

public static class cfxSIPConstants{
	public const string cfxNetMessageNotification = "cfxNetMessageNotificationName";
	public const string cfxNetMessageRealNotificationName = "cfxNetMessageRealNotificationName";
	public const string cfxNetMessageTag = "cfxNetMessageTag";
	public const string cfxSIPNetStatusControl = "cfxSIPNetStatusControl";
	public const string cfxSIPEventStartServer = "cfxSIPEventStartServer";
	public const string cfxSIPEventStopServer = "cfxSIPEventStopServer";
	public const string cfxSIPEventStartClient = "cfxSIPEventStartClient";
	public const string cfxSIPEventStopClient = "cfxSIPEventStopClient";

	public const short lowLevelMessageID = 2000; // do not mess with this unless you know what you are doing.
	public const short messageNotificationID = 2001; // don't mess with this either
}


public class cfxSIPNetworkNotification : MessageBase {
	// we transfer the dictionary as two separate arrays of string
	public string[] keys;
	public string[] values;
}

public class cfxNetworkedNotificationManager : cfxNotificationIntegratedReceiver { // WARNING: WE ARE BASING OFF NON-NETWORKED VERSION
	private string netStatus = "none"; // this manager's state: 'Client', 'Server' or 'none'.
	private NetworkClient theClient = null; // this is the network client that receives the messages


	public override void Start(){
		base.Start (); // this MUST execute, or the test for NM conncetion fails below

		// subscribe to the notifications that are sent out by the SIP network manager 
		// so we can manage out state
		subscribeTo (cfxSIPConstants.cfxSIPNetStatusControl, OnNetworkchange); // this controls my state (server/none/client)
		subscribeTo (cfxSIPConstants.cfxNetMessageNotification, OnSendNetNotification); // when SIP wants to broadcast a notification to the net


		// make sure that there is a notificationManager in the scene
		if (theNotificationManager != null) {
			Debug.Log ("Networked SIP found local notification mananger instsance"); 
			Debug.Log ("Networked SIP initiated");
		} else { 
			Debug.Log ("WARNING: No local notification mananger present!");
			Debug.Log ("WARNING: Networked SIP will not function correctly");
		}
			
	}

	//
	// We receive a net notification. Depending on our own network status (Server or Client) 
	// we need to respond differently:
	// if we receive a notification as server, we need to broadcast that notification to all clients and locally
	// if we are a client, we need to broadcast it locally only
	//

	void OnSIPNetworkNotificationReceived(NetworkMessage netNotification) {
		Debug.Log ("Enter OnSIPNetworkNotificationReceived");
		// we received a message of type SIP network notification
		cfxSIPNetworkNotification theNotification = netNotification.ReadMessage<cfxSIPNetworkNotification> ();
		if (theNotification != null) {
			Debug.Log ("BOOM BABY - NETWORK MESSAGE RECEIVED as " + netStatus);

			if (netStatus == "Server") {
				// we received from a single client, and now need to broadcast
				// to all clients and myself
				NetworkServer.SendToAll (cfxSIPConstants.messageNotificationID, theNotification);

				// now also send this locally
				Dictionary<string, object> info = convertToDict (theNotification);
				string notificationName = fetchString (info, "NotificationName", "SIP Net error: No Notification Name");
				sendNotification (notificationName, info);
				return;

			}

			if (netStatus == "Client") {
				// we received from server, simply broadcast on my machine
				Dictionary<string, object> info = convertToDict (theNotification);
				string notificationName = fetchString (info, "NotificationName", "SIP Net error: No Notification Name");
				sendNotification (notificationName, info);
				return;
			}

			Debug.Log("Received a network notification on an unknown status of '" + netStatus + "'");
			return;

		} else {
			Debug.Log ("received a null SIP network Notification. Dropped");
			return;
		}
	}

	//
	// when the Network Manager changes state, we are being notified, and update
	// our state accordingly. The most important information we get here is the
	// client information that gives us the connection to the server. By using this
	// we do not need to set up our own client / server structure but can entirely 
	// rely on Unity's network protocol.
	//
	public void OnNetworkchange(Dictionary<string, object> info) {
		// this notification is triggered whenever we change state from none to client, server, both, or none
		string theEvent = fetchString (info, "Event", "***Error: no event passed from SIPNetworkManager");

		Debug.Log ("SIP networkManager notifies us about a change in network status: " + theEvent);

		if (theEvent == cfxSIPConstants.cfxSIPEventStartClient) {
			if (netStatus == "Server") {
				Debug.Log ("Overriding Client status because we are already server");
				// we still save the client for reference, though
				theClient = fetchObject (info, "Client", null) as NetworkClient;
				// but we won't register the callback, else that causes a script re-load
				return;
			}

			theClient = fetchObject (info, "Client", null) as NetworkClient;
			netStatus = "Client";
			if (theClient != null) {
				
				theClient.RegisterHandler (cfxSIPConstants.messageNotificationID, OnSIPNetworkNotificationReceived);
			} else {
				Debug.Log ("WARNING: received NULL client from SIP Network Manager");
			}
			return;
		}

		if (theEvent == cfxSIPConstants.cfxSIPEventStartServer) {
			netStatus = "Server";
			NetworkServer.RegisterHandler (cfxSIPConstants.messageNotificationID, OnSIPNetworkNotificationReceived);
			return;
		}

		if (theEvent == cfxSIPConstants.cfxSIPEventStopClient) {
			netStatus = "none";
			theClient = null;
			return;
		}

		if (theEvent == cfxSIPConstants.cfxSIPEventStopServer) {
			netStatus = "none";
			return;
		}
	}


	//
	// Processing / Helper Methods
	// Since we only transfer strings across the network, we need to convert between Dict<string, string> and Dict(string,object>
	//

	string[] ListToArray(List<string> inStrings) {
		if (inStrings.Count < 1) return null;

		string[] outStrings = new string[inStrings.Count];
		int i = 0;
		while (i < inStrings.Count) {
			outStrings [i] = inStrings [i];
			i = i + 1;
		}
		return outStrings;
	}

	Dictionary<string,string> filterForStrings(Dictionary<string, object> info) {
		Dictionary<string, string> filteredInfo = new Dictionary<string, string>();
		foreach (string aKey in info.Keys) {
			if (info[aKey] is string) {
				filteredInfo[aKey] = info[aKey] as string;
			}
		}
		return filteredInfo;
	}

	void dumpDict(Dictionary<string, string> info){
		foreach (string key in info.Keys) {
			Debug.Log ("Info[" + key + "] = " + info [key]);
		}
	}

	public cfxSIPNetworkNotification convertToNetMessage(Dictionary<string, string> filteredInfo) {
		cfxSIPNetworkNotification theNetNotification = new cfxSIPNetworkNotification ();

		// mark this as a network message by adding the network tag 
		filteredInfo[cfxSIPConstants.cfxNetMessageTag] = "TRUE"; // you could change that to the connectionID or other to identify who sent it 

		List<string> myKeys = new List<string>(filteredInfo.Keys);
		theNetNotification.keys = ListToArray (myKeys);
		List<string> myValues = new List<string> (filteredInfo.Values);
		theNetNotification.values = ListToArray (myValues);
		return theNetNotification;
	}

	public Dictionary<string, object> convertToDict(cfxSIPNetworkNotification inMsg) {
		Dictionary<string, object> info = new Dictionary<string, object> ();
		int i = 0;
		int max = inMsg.keys.Length;
		while (i < max) {
			info [inMsg.keys [i]] = inMsg.values [i];
			i = i + 1;
		}
		info ["Object"] = gameObject; // overwrite string with this object for compatibility
		return info;
	}

	public void OnSendNetNotification(Dictionary<string, object> info) {
		// This is where a new net notification is fed into the system via a SIP netSendNotification
		// On entry, the info dictionary contains the masked (to send it here) notification name,
		// the original notification is retrieved from the info dictionary with the 
		// cfxSIPConstants.cfxNetMessageRealNotificationName key
		//
		// We enter here when any LOCAL instance wants to send out a notification
		// to the net. We have first to figure out if we are the server or client. 
		// If we are the server, we can broadcast to all clients.
		// If we are a client, we'll send the notification to the server 
		// recover "true" notification name
		string notificationName = fetchString(info, cfxSIPConstants.cfxNetMessageRealNotificationName, "SIP Error-real notification name not set");
		info.Remove (cfxSIPConstants.cfxNetMessageRealNotificationName);
		info ["NotificationName"] = notificationName;
		// now, convert/strip all non-string objects
		Dictionary<string, string> filteredInfo = filterForStrings (info);

		// now convert the dict to a network object
		cfxSIPNetworkNotification theNetNotification = convertToNetMessage (filteredInfo);

		// first, see if we are server
		// we can use NetworkServer.sendtoAll
		if (netStatus == "Server") {
			Debug.Log ("Server is initiating notification. Broadcast to all");
			// send it to all clientes on the network
			if (!NetworkServer.SendToAll (cfxSIPConstants.messageNotificationID, theNetNotification)) {
				Debug.Log ("Sending message via network failed");
			}

			// we may need to send it locally, too...
			return;
		}
		// see if we are client
		if (netStatus == "Client") {
			Debug.Log ("Client is initiating notification. Upcast to server for re-broadcast");
			// send to server for processing
			theClient.Send (cfxSIPConstants.messageNotificationID, theNetNotification);
			// it will come back through the server
			return;
		}

		// neither? OK; simply broadcast local

		sendNotification (notificationName, info);

	}



}
