using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class cfxSIPNetworkManager : cfxSIPNetworkManagerIntegratedReceiver {

	//
	// Part of SIP for Unity, (C) 2018 by Christian Franz and cf/x AG
	//
	// this is the SIP Network Manager that you must use in your 
	// scenes (or base your own network manager on) if you want to 
	// send notifications across the network
	//

	// this version simply extends the normal Network Mananger by
	// functions to send notifications that control the behaviour 
	// of the networked notification mananger subpart

	public override void OnStartClient(NetworkClient client) {
		base.OnStartClient (client);
		Dictionary<string, object> theInfo = getBasicInfo (cfxSIPConstants.cfxSIPEventStartClient);
		theInfo ["Client"] = client;

		if (verbose) Debug.Log ("Started a client");
		sendNotification (cfxSIPConstants.cfxSIPNetStatusControl, theInfo);
	}

	public override void OnStartServer(){
		base.OnStartServer ();
		if (verbose) Debug.Log ("Started a Server");
		sendNotification (cfxSIPConstants.cfxSIPNetStatusControl, cfxSIPConstants.cfxSIPEventStartServer);
	}

	public override void OnStopClient() {
		base.OnStopClient ();
		if (verbose) Debug.Log ("Stopped a Client");
		sendNotification (cfxSIPConstants.cfxSIPNetStatusControl, cfxSIPConstants.cfxSIPEventStopClient);
	}

	public override void OnStopServer(){
		base.OnStopServer ();
		if (verbose) Debug.Log ("Stopped Server");
		sendNotification (cfxSIPConstants.cfxSIPNetStatusControl, cfxSIPConstants.cfxSIPEventStopServer);
	}


}
