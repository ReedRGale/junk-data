
/* Tells the unit it's in a contemplative state and that input should change accordingly. */
public class MovableContemplativeState : MovableInputState
{
        /* Constructors */


    public MovableContemplativeState(Movable theUnit) : base(theUnit) { }


        /* Movable Behavior */


    protected override void BaseEnter(Movable theUnit)
    {
        // Change the movement schema to whatever the FocusManager has focused at the moment.
        unit.InputContemplate();
    }

    protected override void BaseExit(Movable theUnit)
    {
        // Set the movement schema back to being unrestricted.
        unit.InputUnrestricted();
    }
}
