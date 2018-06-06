using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableStaticState : MovableActionState
{
        /* Constructors */


    public MovableStaticState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */


    protected override void HandleState()
    {
        if (ShouldJump()) { SwitchState(new MovableJumpingState(unit)); }
        else if (ShouldWalk()) { SwitchState(new MovableWalkingState(unit)); }
        if (HasNotSwitched())
            base.HandleState();
    }
}
