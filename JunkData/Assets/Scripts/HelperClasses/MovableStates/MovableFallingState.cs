
public class MovableFallingState : MovableActionState
{
        /* Constructors */


    public MovableFallingState(Movable theUnit) : base(theUnit) { }

    
        /* State Behavior */


    protected override void HandleState()
    {
        if (ShouldWalk() && unit.GetActionState(1) is MovableWalkingState)
            SwitchState(new MovableWalkingState(unit));
        if (HasNotSwitched())
            base.HandleState();
    }
}
