using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableDeconstructedState : MovableInputState
{
        /* Constructors */


    public MovableDeconstructedState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */

    protected override void BaseEnter(Movable theUnit)
    {
        // Set the logic for the deconstruction animation.

        // When that is done, deactivate the object.
        unit.GetGameObject().SetActive(false);

        // Set the focus to warp automatically.
        // FocusManager.SetFocus(Functions.Warp, Mode.CantCancel);
    }
}
