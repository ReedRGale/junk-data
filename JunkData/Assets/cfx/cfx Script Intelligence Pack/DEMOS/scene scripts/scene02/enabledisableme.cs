using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enabledisableme : cfxNotificationIntegratedReceiver {

	public bool toggleObject = true;
	public bool toggleScript = true;

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("toggleEnable");
	}
	
	public override void OnNotification (string notificationName)
	{
		if (toggleObject) gameObject.SetActive (!gameObject.activeSelf);
		if (toggleScript) this.enabled = (!this.enabled);

	}
}
