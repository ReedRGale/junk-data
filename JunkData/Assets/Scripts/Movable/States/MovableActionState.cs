
/* States that tend to do something each cycle and describe what the unit is doing. */
using UnityEngine;

public class MovableActionState : MovableState
{
    private const int LEFT_CLICK = 0;                           // Int representing the value of a left click.

        /* Constructors */


    public MovableActionState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */


    protected override void HandleState()
    {
        if (ShouldWarp())       { SwitchState(new MovableWarpState(unit)); }
        else if (ShouldFly())   { SwitchState(new MovableFlyingState(unit)); }
        else if (ShouldRise())  { SwitchState(new MovableRisingState(unit)); }
        else if (ShouldFall())  { SwitchState(new MovableFallingState(unit)); }
        else if (ShouldSlide()) { SwitchState(new MovableSlidingState(unit)); }
        else if (IsStatic())    { SwitchState(new MovableStaticState(unit)); }
    }


        /* Behavior Modification */


    public override bool ModifyAction(Modification mod)
    {
        actionMods.Add(mod);
        return true;
    }

    public override bool ModifyEnter(Modification mod)
    {
        enterMods.Add(mod);
        return true;
    }

    public override bool ModifyExit(Modification mod)
    {
        exitMods.Add(mod);
        return true;
    }

    public override bool RemoveAction(Modification mod)
    {
        actionMods.Remove(mod);
        return true;
    }

    public override bool RemoveEnter(Modification mod)
    {
        enterMods.Remove(mod);
        return true;
    }

    public override bool RemoveExit(Modification mod)
    {
        exitMods.Remove(mod);
        return true;
    }

    public override bool RedefineAction(Modification mod)
    {
        action = mod;
        return true;
    }

    public override bool RedefineEnter(Modification mod)
    {
        enter = mod;
        return true;
    }

    public override bool RedefineExit(Modification mod)
    {
        exit = mod;
        return true;
    }


        /* Helper Methods */


    protected override void SwitchState(MovableState theState)
    {
        OnExit();
        unit.SetActionState(theState as MovableActionState);
        theState.OnEnter();
    }

    protected override bool HasNotSwitched() { return unit.GetActionState().GetType() == GetType(); }

    protected override bool ShouldSwitch(MovableState theState) { return unit.GetActionState().GetType() != theState.GetType(); }

    private bool ShouldWarp()
    {
        // FocusManager.FunctionFocus().name.Equals("warp") && ShouldSwitch(new MovableWarpingState(unit));
        return false;
    }
    private bool ShouldFly() { return unit.GetNyoooom() && unit.GetDamage() <= 0 && ShouldSwitch(new MovableFlyingState(unit)); }
    private bool ShouldRise() { return unit.GetRB2D().velocity.y > 0 && !unit.IsGrounded() && ShouldSwitch(new MovableRisingState(unit)); }
    private bool ShouldFall() { return unit.GetRB2D().velocity.y < 0 && !unit.IsGrounded() && ShouldSwitch(new MovableFallingState(unit)); }
    private bool ShouldSlide()
    {
        return unit.IsGrounded()
            && Mathf.Abs(unit.GetRB2D().velocity.x) > float.Epsilon
            && unit.GetMoveInput() == 0
            && ShouldSwitch(new MovableSlidingState(unit));
    }
    protected bool IsStatic() { return unit.GetRB2D().velocity.magnitude < float.Epsilon && unit.IsGrounded() && ShouldSwitch(new MovableStaticState(unit)); }
    protected bool ShouldJump() { return Input.GetMouseButtonUp(LEFT_CLICK) && ShouldSwitch(new MovableJumpingState(unit)); }
    protected bool ShouldWalk() { return Mathf.Abs(unit.GetMoveInput()) > 0 && ShouldSwitch(new MovableWalkingState(unit)); }
}
