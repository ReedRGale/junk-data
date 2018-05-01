using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponChooser : cfxNotificationReceiver {

    public string notificationName;
    public string weaponName;
    public bool observeRootName;

    /*
     * This script waits for a notification to switch to a certain weapon
     * and if the "Event" matches weaponName, activates the mesh for this
     * weapon. If the name does not match it deactivates the mesh for 
     * good measure
     * 
     * If observe root name is true, the scripts looks at the root name for
     * this object and will only equip/deequip if it matches the name that is 
     * given in the "RootName" entry of the info dict
     * In order to use, you must attach it to every weapon in the model,
     * with correct names for each
     * 
     */

	public override void Start() {
		// call SIP's Start()
		base.Start ();
        subscribeTo(notificationName, weaponNotification);
    }


    public void weaponNotification( Dictionary<string, object> info) {
        if (observeRootName)
        {
            string myRootName = this.transform.root.gameObject.name;
            if (!info.ContainsKey("RootName")) return;
            if (myRootName != (info["RootName"] as string)) return;

            if (verbose) Debug.Log("Found root for weapon");
        }

        if (weaponName == null) {
            Debug.Log("Weapon name not defined, unable to choose this weapon");
            return;
        }

        if (!info.ContainsKey("Event")) return;
        GameObject theParent = this.transform.parent.gameObject;
        MeshRenderer theMeshRenderer = theParent.GetComponent<MeshRenderer>();

        // bad coders would write: theMeshRenderer = (weaponName == (info["Event"] as string));
        // because they believe it's more efficient or cooler. It's neitehr, just more difficult to read and maintain.
        // that's what we have optimizers in the compiler for, guys!

        if (weaponName == (info["Event"] as string)) {
            // enable this mesh
            theMeshRenderer.enabled = true;
            if (verbose) Debug.Log("Enabled mesh for " + theParent.name);
        } else {
            theMeshRenderer.enabled = false;
            if (verbose) Debug.Log("Disabled mesh for " + theParent.name);
        }
    }

}
