
/* States that tend to represent what input the unit can recieve. */
public abstract class MovableInputState : MovableState
{
        /* Constructors */


    public MovableInputState(Movable theUnit) : base(theUnit) { }


        /* State Behavior */


    protected override void HandleState()
    {
        if      (ShouldLock())          { SwitchState(new MovableLockedState(unit)); }
        else if (ShouldDeconstruct())   { SwitchState(new MovableDeconstructedState(unit)); }
        else if (ShouldContemplate())   { SwitchState(new MovableContemplativeState(unit)); }
        else if (ShouldStun())          { SwitchState(new MovableStunnedState(unit)); }
        else                            { SwitchState(new MovableUnrestrictedState(unit)); }
    }


    /* Helper Methods */


    protected override void SwitchState(MovableState theState)
    {
        OnExit();
        unit.SetInputState(theState as MovableInputState);
        theState.OnEnter();
    }

    protected override bool HasNotSwitched() { return unit.GetInputState().GetType() == GetType(); }

    protected override bool ShouldSwitch(MovableState theState) { return unit.GetInputState().GetType() != theState.GetType(); }

    private bool ShouldLock() { return unit.IsInputLocked() && ShouldSwitch(new MovableLockedState(unit)); }
    private bool ShouldDeconstruct() { return unit.IsDeconstructed() && ShouldSwitch(new MovableDeconstructedState(unit)); }
    private bool ShouldContemplate()
    {
        // return Object.NotNull(FocusManager.FunctionFocus()) && ShouldSwitch(new MovableContemplativeState(unit))
        return false;
    }
    private bool ShouldStun() { return unit.GetDamage() > 0 && ShouldSwitch(new MovableStunnedState(unit)); }
}
