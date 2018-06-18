using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableSlidingState : MovableActionState
{
        /* Constructors */


    public MovableSlidingState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */


    protected override void BaseEnter(Movable theUnit)
    {
        theUnit.GetRB2D().Sleep();
    }
}
