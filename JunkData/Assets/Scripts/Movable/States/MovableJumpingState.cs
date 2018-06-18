using UnityEngine;

public class MovableJumpingState : MovableActionState
{
        /* Constructors */


    public MovableJumpingState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */

    protected override void BaseEnter(Movable theUnit)
    {
        unit.SetMouseData();
        unit.GetRB2D().WakeUp();
        unit.GetRB2D().AddForce(unit.GetMouseDirection() * Movable.JUMP_FORCE, ForceMode2D.Impulse);
    }
}
