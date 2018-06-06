using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableWarpingState : MovableActionState
{
        /* Constructors */


    public MovableWarpingState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */

    protected override void BaseEnter(Movable theUnit)
    {
        // Set true, just in case we're deconstructed.
        unit.GetGameObject().SetActive(true);

        // Warp to the given position.
        unit.GetRB2D().position = unit.GetMousePosition();
    }
}
