using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableWalkingState : MovableActionState
{
    protected float walkVelocity = 1.25f;
    protected float walkAccel = 0.7f;
    protected float horizontalHeld = 0;
    private const float ACCEPT_LOCK_OVERRIDE = 0.4f;

        /* Constructors */


    public MovableWalkingState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */

    protected override void HandleState()
    {
        if (HasNotSwitched() && ShouldJump())
            SwitchState(new MovableJumpingState(unit));
        base.HandleState();
    }

    protected override void BaseAction(Movable theUnit)
    {
        // Update the horizontal.
        HoldingHorizontal();

        if (WalkingUnlocked() || UnlockForced())
        {
            unit.GetRB2D().WakeUp();

            // Move if we're not at max movespeed.
            if (WithinSpeedLimit())
                unit.GetRB2D().AddForce((unit.GetCollisionAnalyzer().GetBaseWalkVector() + -unit.GetLocalPhysics().ParallelGForce()) * walkAccel);
        }
        else if (unit.IsGrounded())
            SwitchState(new MovableSlidingState(unit));
    }

    // Check if walking is locked.
    private bool WalkingUnlocked()
    {
        return !(unit.GetMoveInput() <= 0 && unit.GetScouter().lockStates.Contains(MoveInput.LEFT))
               && !(unit.GetMoveInput() >= 0 && unit.GetScouter().lockStates.Contains(MoveInput.RIGHT));
    }

    // Check if the unit is trying to force an unlock.
    private bool UnlockForced() { return !WalkingUnlocked() && horizontalHeld >= ACCEPT_LOCK_OVERRIDE; }

    // Records the amount of time that a value has been holding this value.
    private void HoldingHorizontal()
    {
        horizontalHeld = unit.GetMoveInput(1) == unit.GetMoveInput()
              ? horizontalHeld + Time.deltaTime : 0;
    }

    // Returns whether the object is moving within injected limits.
    private bool WithinSpeedLimit() { return Mathf.Abs(unit.GetRB2D().velocity.x) < walkVelocity; }
}
