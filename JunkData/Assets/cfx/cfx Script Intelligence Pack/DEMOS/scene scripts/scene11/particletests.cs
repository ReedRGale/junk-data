using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particletests : cfxNotificationIntegratedReceiver {

	private bool particlesEmitting = true;

	// Use this for initialization
	public override void Start() {
		base.Start ();
		subscribeTo ("Input");
	}

	public override void OnNotification (string notificationName)
	{
		//Debug.Log ("On the book in ParticleTests");
		//theNotificationManager.dumpSubscriptions ();

		if (notificationName == "Input") {
			if (particlesEmitting) {
				postNotification("StopParticles");
				particlesEmitting = false;
				Debug.Log ("Stopped Particles");
			} else {
				postNotification("StartParticels");
				particlesEmitting = true;
				Debug.Log ("Started Particles");

			}

		}
	}

}
