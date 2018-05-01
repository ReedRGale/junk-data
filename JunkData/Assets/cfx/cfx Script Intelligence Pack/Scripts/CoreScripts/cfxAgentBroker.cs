using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * Copyright (C) 2017, 2018 by cf/x Ag and Christian Franz
 * 
 * cfxAgentBroker
 * Attach a daughter script to a parent object or it's root, and then
 * copy all pre-configured properties 
 * 
 * This script is NOT SIP enabled, it's merely an incredibly well crafted helper script
 * 
 */

public class cfxAgentBroker : MonoBehaviour {

	public cfxIntegratedQuery theScript; // we attach this to the root or parent
    public bool attachToRoot = true;

	public bool removeOriginalAfterCopy = true;

	public bool verbose = false;

	// Use this for initialization
	void Start () {
	    // this script's only purpose is to attach the payload to
        // the enclosing object or its root, and then configure 
        // the parameters 

        // And all of this magic happen on Awake, not start!
	}

    // Since we can't move a component between objects even if they are in the same
    // hierarchy, we clone the attached script, and then copy the configured
    // properties to the clone
    // we use Unity's reflection API here, although implementing a copy method for 
    // SIP would potentially be cleaner.

    public static void copyComponentProperties(Component original, GameObject destination)
    {
        if (original == null) return;
        if (destination == null) return;

        Component[] componentList = destination.GetComponents<Component>();
        System.Type type = original.GetType();
        System.Reflection.FieldInfo[] fields = type.GetFields();

        Component theCopy = null;

        foreach (Component existingComponent in componentList)
        {
            // If we already have one of them
            if (original.GetType() == existingComponent.GetType())
            {
                // the component exists already, remember it
                theCopy = existingComponent;
            }
        }

        // Add it only to destination if it doesn't already exist
        if (theCopy == null) theCopy = destination.AddComponent(type);

        // Copied fields can be restricted with BindingFlags
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(theCopy, field.GetValue(original));
        }

    }


    private void Awake()
    {
        if (theScript == null) return;

        GameObject theTarget = null;

        if (attachToRoot) {
            Transform theRoot = this.transform.root;
            theTarget = theRoot.gameObject;

        } else {
            Transform theParent = this.transform.parent;
            theTarget = theParent.gameObject;
        }

        if (theTarget == null) return;
		if (verbose)
			Debug.Log("found a target");
         
		Component theScriptComponent = gameObject.GetComponent<cfxIntegratedQuery>(); // gets the atatched canned script
        System.Type theType = theScriptComponent.GetType();
        theTarget.AddComponent(theType); // we now added a fresh instance of our canned Agent to the target

		cfxIntegratedQuery theTargetComponent = theTarget.GetComponent<cfxIntegratedQuery>();
		if (verbose)
			Debug.Log(theTargetComponent);

        copyComponentProperties(theScriptComponent, theTarget);
		// Warning: we just copied ALL of theScriptComponent. This means that we also copy unique stuff. make sure we dont.
		//theTargetComponent.uuid = null; // if we don't null this, it's the same from source, so the following Destroy will unsubscribe all
		theTargetComponent.resetUUID (); // use reset, as uuid has become a read-only property
		theTargetComponent.theNotificationManager = null; // ditto

		// in order to not interfere, we now destroy or disable the original
		if (removeOriginalAfterCopy) {
			// safe bet: destroy this instance
			Destroy (theScript);
		} else {
			// keep, and disable our own script so it will not interfere
			theScript.enabled = false;
		}

    }

}
