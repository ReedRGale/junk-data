using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeActivateDisEnable : cfxNotificationIntegratedReceiver {

	// 
	// cf/x SIP integration prefab:
	// remotely enable/disable scripts
	// or activate / deactivate object

	public string activate;
	public string deactivate;
	public string enable;
	public string disable;

	public bool autocollectScripts = true;

	public List<MonoBehaviour> scripts;

	public override void Start() {
		// call SIP's Start()
		base.Start ();
		subscribeTo (activate);
		subscribeTo (deactivate);
		subscribeTo (enable);
		subscribeTo (disable);

		// just for show. will be updated every call, but we pre-fill the array
		// so we can see in editor what would be in here
		if (autocollectScripts) {
			scripts = new List<MonoBehaviour>(gameObject.GetComponents<MonoBehaviour> ());
		}

	}
	
	public override void OnNotification (string notificationName)
	{
		if (notificationName == activate) {
			gameObject.SetActive (true);
			return;
		}

		if (notificationName == deactivate) {
			gameObject.SetActive (true);
			return;
		}

		if (notificationName == enable) {
			if (autocollectScripts) {
				scripts = new List<MonoBehaviour>(gameObject.GetComponents<MonoBehaviour> ());
			}

			foreach (MonoBehaviour aScript in scripts) {
				aScript.enabled = true;
			}
		}

		if (notificationName == disable) {
			if (autocollectScripts) {
				scripts = new List<MonoBehaviour>(gameObject.GetComponents<MonoBehaviour> ());
			}

			foreach (MonoBehaviour aScript in scripts) {
				aScript.enabled = false;
			}
		}

	}
}
