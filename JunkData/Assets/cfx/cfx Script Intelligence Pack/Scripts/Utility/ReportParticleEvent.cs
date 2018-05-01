using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ReportObjectCollision:
 * A cfxNotificationManager UtilityScript
 *
 * Copyright(C) 2017 by cf/x AG and Christian Franz
 *
 * This script 
 * - starts / stops all children particle emitters when the respective notification was received
 * - broadcasts a notification whenever the GameObject reports a collision or trigger with it's particlesystems:
 *
 * When subscribing to the <particleEvent> notification, the following fields are defined in the information dictionary
 * - GameObject: the GameObject that is reporting the change (eitehr root or parent of the prefab)
 * - Time: Time (as a string) when this happened
 * - Module: "ReportObjectCollision"
 * - Event: "Collision" / "Trigger" 
 * - Collision: the collision information  
 * - NotificationName: content of <particleEvent> or "cfxGenericNotification" when empty
 * 
 * On a particle Collison, the "Other" key contains the other GameObject
 * 
 */

public class ReportParticleEvent : cfxNotificationIntegratedReceiver {

	// remotely start and stop the particle system
	public string startOnNotification = "StartParticels";
	public string stopOnNotification = "StopParticles";

	public string particleEvent = "ParticleEvent";

    public bool reportCollision = true;
    public bool reportTrigger = true;

	// manage a list of all my particle systems for quick access
	private  List<ParticleSystem> myParticleSystems = null;

	public override void Start() {
		// call SIP's Start()
		base.Start ();
		if (startOnNotification.Length > 0) {
			subscribeTo (startOnNotification);
		}

		if (stopOnNotification.Length > 0) {
			subscribeTo (stopOnNotification);
		}

		myParticleSystems = new List<ParticleSystem> ();
		// get all particle systems
		myParticleSystems.AddRange (GetComponentsInChildren<ParticleSystem> ());
	}

    private void OnParticleCollision(GameObject other)
    {
        if (reportCollision)
        {
            Dictionary<string, object> theDict = this.getBasicInfo("Collision");
            theDict.Add("Other", other);
            // add more information here
			sendNotification (particleEvent, theDict);
        }

    }

    private void OnParticleTrigger()
    {
        if (reportCollision)
        {
            Dictionary<string, object> theDict = this.getBasicInfo("Trigger");
            // add more information here
			sendNotification (particleEvent, theDict);
        }
    }

	public override void OnNotification (string notificationName)
	{

		if (notificationName == startOnNotification) {
			if (verbose) 
				Debug.Log("received start particle command");
			foreach (ParticleSystem p in myParticleSystems) {
				p.Play ();
				var em = p.emission;
				em.enabled = true;

			}
			return;
		}


		if (notificationName == stopOnNotification) {
			if (verbose) 
				Debug.Log("received stop particle command");
			foreach (ParticleSystem p in myParticleSystems) {
				p.Stop ();
				var em = p.emission;
				em.enabled = false;
			}
			return;
		}

	}
}
