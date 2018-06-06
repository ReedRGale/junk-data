using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* While in this state, the unit is stunned and can no longer move
 * (jump and walk states are no longer accessible). */
public class MovableStunnedState : MovableInputState
{
        /* Constructors */


    public MovableStunnedState(Movable theUnit) : base(theUnit) { }
}
