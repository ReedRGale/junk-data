using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dissolvensignal : cfxNotificationIntegratedReceiver {

	public float fadeTime = 1;

	private bool fading = false;
	private bool fadeOut = false;
	private float currentFadeTime;

	private Material mat;

	// Use this for initialization
	public override void Start() {
		// call SIP's Start()
		base.Start ();
		// your own code here
		subscribeTo ("Dissolve");
		subscribeTo ("Resolve");
		mat = GetComponent<Renderer>().material;
		if (fadeTime < 0.1)
			fadeTime = 1f;
	}
	
	// Update is called once per frame
	void Update () {
		if (!fading)
			return;	
		// we are fading. Set shader's dissolve amount
		float amount;
		if (fadeOut) {
			amount = 1 - (currentFadeTime / fadeTime);
		} else {
			amount = currentFadeTime / fadeTime;
		}
		mat.SetFloat ("_DissolveAmount", amount);
		currentFadeTime = currentFadeTime - Time.deltaTime;
		if (currentFadeTime < 0) {
			fading = false; // stop fading altogether
			if (fadeOut)     {
				StopAllCoroutines(); // erase all previous intnances of this
				sendNotification("Resolve", 1.0f, true); // start fade in in one second, LOCAL post
			}
		}
	}

	private void startFadeOut() {
		fading = true;
		currentFadeTime = fadeTime;
		fadeOut = true;
	}


	private void startFadeIn() {
		fading = true;
		currentFadeTime = fadeTime;
		fadeOut = false;
	}

	public override void OnNotification (string notificationName)
	{
		if (fading)
			return;
		
		if (notificationName == "Dissolve") {
			StopAllCoroutines(); 
			startFadeOut ();
		}

		if (notificationName == "Resolve") {
			startFadeIn ();
		}
	}

}
