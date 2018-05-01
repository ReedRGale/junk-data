using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportAnimatorStateChange : cfxNotificationAgent {

    public bool reportStateChange = true;
    public int layer = 0;
    public int samplesPerSecond = 10;

    private string theModule = "ReportAnimatorStateChange";


    private Animator theAnimator = null;
    private int lastHash =-1;
    private bool sampling = false;
    private bool coroutineIsActive = false;


    // Use this for initialization
    public override void Start() {
		base.Start ();
		// since we might burn CPU we only start on start (duh), and check here if there actually is an
        // audio source connectd
        theAnimator = this.GetComponent<Animator>();
        if (theAnimator == null)
        {
            if (verbose)
            {
                Debug.Log("No Animator in object. Notifier shutting down for this object");
            }
            return;
        }

        // initialize lastClip so we don't report it if
        // the initial clip wasn't null
        AnimatorStateInfo theInfo = theAnimator.GetCurrentAnimatorStateInfo(layer);
        lastHash = theInfo.shortNameHash;

        // if we get here, we are all set. Start the co-routine 
        sampling = true;
        if (samplesPerSecond < 1) samplesPerSecond = 1;
        if (samplesPerSecond > 1000) samplesPerSecond = 1000;
        coroutineIsActive = true;
        StartCoroutine("sampleAnimator");
    }

    private void OnEnable()
    {
        if (theAnimator == null) return;

        sampling = true;
        if (!coroutineIsActive)
        {
            coroutineIsActive = true;
            StartCoroutine("sampleAnimator");
        }
    }

    private void OnDisable()
    {
        sampling = false; // will stop coroutine from re-starting next time
    }

    //
    // we sample the audio source's status with a coroutine every 1/n seconds
    //
    IEnumerator sampleAnimator()
    {
        while (sampling)
        {
            AnimatorStateInfo theInfo = theAnimator.GetCurrentAnimatorStateInfo(layer);
            int thisHash = theInfo.shortNameHash;
            if (thisHash != lastHash)
            {
                // ok, state changed. let's report that
                // depending on the current state, we either report start or stop
                // this takes precedence over a clip change.
                if (reportStateChange)
                {
                    Dictionary<string, object> theDict = this.getBasicInfo(theModule, "ClipChange");
                    theDict.Add("Hash", thisHash.ToString());
                    // add any more information here
                    this.post(theDict);
                }

                // remember for next time so we don't trigger again
                lastHash = thisHash;
            }
            else
            {

            }

            yield return new WaitForSeconds(1.0f / samplesPerSecond);
        }
        // when we get here, the script was disabled. note that we have expired and 
        // need restarting with OnEnable
        coroutineIsActive = false;

    }
}
