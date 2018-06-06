using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* While in this state, MovableActionStates can be changed without restrictions. */
public class MovableUnrestrictedState : MovableInputState
{
        /* Constructors */


    public MovableUnrestrictedState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */


    protected override void BaseEnter(Movable theUnit) { theUnit.SetInputLocked(false); }
}
