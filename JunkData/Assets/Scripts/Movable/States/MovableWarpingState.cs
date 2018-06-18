using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableWarpState : MovableActionState
{
        /* Constructors */


    public MovableWarpState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */

    protected override void BaseEnter(Movable theUnit)
    {
        // Warp
    }
}
