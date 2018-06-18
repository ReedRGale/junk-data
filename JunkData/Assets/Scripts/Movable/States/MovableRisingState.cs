
public class MovableRisingState : MovableActionState
{
        /* Constructors */


    public MovableRisingState(Movable theUnit) : base(theUnit) { }

    
        /* State Behavior */


    protected override void HandleState()
    {
        if (ShouldWalk() && unit.GetActionState(1) is MovableWalkingState || unit.GetActionState(1) is MovableFallingState)
            SwitchState(new MovableWalkingState(unit));
        if (HasNotSwitched())
            base.HandleState();
    }
}
