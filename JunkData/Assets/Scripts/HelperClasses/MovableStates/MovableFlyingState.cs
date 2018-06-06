using UnityEngine;

/* A state that represents the fact that the unit is flying. */
public class MovableFlyingState : MovableActionState
{
        /* Constructors */


    public MovableFlyingState(Movable theUnit) : base(theUnit) { }


    /* State Behavior */

    protected override void BaseEnter(Movable theUnit) { unit.SetMouseData(); }

    protected override void BaseAction(Movable theUnit)
    {
        if (unit.GetRB2D().velocity.magnitude < float.Epsilon)
            unit.GetRB2D().AddForce(unit.GetMouseDirection() * Movable.JUMP_FORCE, ForceMode2D.Impulse);
        else
            unit.GetRB2D().AddForce(-unit.GetRB2D().velocity);
    }
}
