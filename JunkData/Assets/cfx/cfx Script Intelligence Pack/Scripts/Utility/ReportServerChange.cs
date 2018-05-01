using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ReportObjectServerChanges:
 * A cfxNotificationManager UtilityScript
 * 
 * Copyright (C) 2017 by cf/x AG and Christian Franz
 * 
 * This script will broadcast a notification whenever the GameObject reports a collision with it's collider:
 * 
 * When subscribing to this notification, the following fields are defined in the information dictionary 
 * - GameObject: the GameObject that is reporting the change (eitehr root or parent of the prefab)
 * - Time: Time (as a string) when this happened
 * - Module: "ReportObjectStateChange"
 * - Event: "Connect" / "Disconnect" / "ConnectFail" / "MasterFail" / "MasterEvent"
 * - NotificationName: content of <theNotificationName> or "cfxGenericNotification" when empty
 * 
 * On Disconnect, the Networkinfo is available under the "Info" key
 * On Fail, the Network error is available under the "Error" key
 * On Master Event, the event is available under the "MasterEvent" key
 */


public class ReportServerChange : cfxNotificationAgent {

    public bool reportConnect = true;
    public bool reportDisconnect = true;
    public bool reportConnectFailure = true;
    public bool reportMasterFailure = true;
    public bool reportMasterEvent = true;


    private string theModule = "ReportServerChange";

    private void OnConnectedToServer()
    {
        if (reportConnect)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Connect");
            // add any more information here
            this.post(theDict);
        }
    }

    private void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (reportDisconnect)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Disconnect");
            theDict.Add("Info", info);
            this.post(theDict);
        }
    }

    private void OnFailedToConnect(NetworkConnectionError error)
    {
        if (reportConnectFailure)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "ConnectFail");
            theDict.Add("Error", error);
            this.post(theDict);
        }

    }

    private void OnFailedToConnectToMasterServer(NetworkConnectionError error)
    {
        if (reportConnectFailure)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "MasterFail");
            theDict.Add("Error", error);
            this.post(theDict);
        }
    }

    private void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (reportMasterEvent)
        {
            Dictionary<string, object> theDict = this.getBasicInfo(theModule, "MasterEvent");
            theDict.Add("MasterEvent", msEvent);
            this.post(theDict);
        }
    }

}
