using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ReportAudioChange:
 * A cfxNotificationManager UtilityScript
 * 
 * Copyright (C) 2017 by cf/x AG and Christian Franz
 * 
 * This script will broadcast a notification whenever the attached audio source starts or finishes playing
 * 
 * WARNING:
 * Due to insufficiencies in Unity's design, this is currently implemented aas a busy wait loop
 * and consumes resources even if nothing is happening. You control this script's efficieny with 
 * the 'samples per second' parameter. The higher the number, the more CPU it burns.
 * For most games, a value of 10 is a good value
 * 
 * If there is no audio source in the object, the script will shut down and not burn any CPU
 * 
 * Furthermore, if the object is disabled, the script suspends sampling until it is enabled again
 * 
 * When subscribing to this notification, the following fields are defined in the information dictionary 
 * - GameObject: the GameObject that is reporting the change (eitehr root or parent of the prefab)
 * - Time: Time (as a string) when this happened
 * - Module: "ReportAudioFinished"
 * - Event: "Start" / "Stop" / "ClipChange"
 * - Audio: the name of the audio clip  
 * - NotificationName: content of <theNotificationName> or "cfxGenericNotification" when empty
 * 
 */

public class ReportAudioChange : cfxNotificationAgent
{

    public bool reportStart = true;
    public bool reportStop = true;
    public bool reportClipChange = true;
    public int samplesPerSecond = 10;

    private string theModule = "ReportAudioChange";


    private AudioSource theAudioSource = null;
    private AudioClip lastClip = null;
    private bool wasPlayingLastTime = false;
    private bool sampling = false;
    private bool coroutineIsActive = false;


    // Use this for initialization
	public override void Start() {
		// call SIP's Start()
		base.Start ();
        // since we might burn CPU we only start on start (duh), and check here if there actually is an
        // audio source connectd
        theAudioSource = this.GetComponent<AudioSource>();
        if (theAudioSource == null)
        {
            if (verbose)
            {
                Debug.Log("No Audiosource in object. Notifier shutting down for this object");
            }
            return;
        }

        // initialize lastClip so we don't report it if
        // the initial clip wasn't null
        lastClip = theAudioSource.clip;

        // if we get here, we are all set. Start the co-routine 
        sampling = true;
        if (samplesPerSecond < 1) samplesPerSecond = 1;
        if (samplesPerSecond > 1000) samplesPerSecond = 1000;
        wasPlayingLastTime = theAudioSource.isPlaying;
        coroutineIsActive = true;
        StartCoroutine("sampleAudioSource");
    }

    private void OnEnable()
    {
        if (theAudioSource == null) return;
       
        sampling = true;
        if (!coroutineIsActive) {
            coroutineIsActive = true;
            StartCoroutine("sampleAudioSource");
        }    
    }

    private void OnDisable()
    {
        sampling = false; // will stop coroutine from re-starting next time
    }

    //
    // we sample the audio source's status with a coroutine every 1/n seconds
    //
    IEnumerator sampleAudioSource()
    {
        while (sampling)
        {
            bool isPlayingNow = theAudioSource.isPlaying;
            AudioClip theClip = theAudioSource.clip;

            if (isPlayingNow != wasPlayingLastTime)
            {
                // ok, state changed. let's report that
                // depending on the current state, we either report start or stop
                // this takes precedence over a clip change.
                if (isPlayingNow && reportStart)
                {
                    Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Start");
                    if (theClip == null) {
                        theDict.Add("Audio", "-");
                    } else {
                        theDict.Add("Audio", theClip.name);
                    }
                    // add any more information here
                    this.post(theDict);
                }

                if (!isPlayingNow && reportStop)
                {
                    Dictionary<string, object> theDict = this.getBasicInfo(theModule, "Stop");
                    // usually, when stopped, the Clip now is null. Not so if paused.
                    if (theClip == null) {
                        theDict.Add("Audio", "-");
                    } else {
                        theDict.Add("Audio", theClip.name);
                    }
                    // add any more information here
                    this.post(theDict);
                }
                // remember for next time so we don't trigger again
                wasPlayingLastTime = isPlayingNow;
            }  else {
                // if we get here, we check if the clip changed
                if (theClip != lastClip) {
                    if (reportClipChange) {
                        if (verbose) Debug.Log("detected clip change");

                        Dictionary<string, object> theDict = this.getBasicInfo(theModule, "ClipChange");
                        // usually, when stopped, the Clip now is null. Not so if paused.
                        if (theClip == null) {
                            theDict.Add("Audio", "-");
                        } else {
                            theDict.Add("Audio", theClip.name);
                        }
                        // add any more information here
                        this.post(theDict);
                    }
                    lastClip = theClip;
                }
            }

            yield return new WaitForSeconds(1.0f / samplesPerSecond);
        }
        // when we get here, the script was disabled. note that we have expired and 
        // need restarting with OnEnable
        coroutineIsActive = false;

    }
}
