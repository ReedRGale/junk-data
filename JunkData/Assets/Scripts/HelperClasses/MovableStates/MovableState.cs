using System.Collections;
using System.Collections.Generic;

public abstract class MovableState
{
    // A delegate allowing for modifications to state behavior.
    public delegate void Modification(Movable theUnit);

    /* Basic Functionality */

    protected Movable unit;
    protected Modification action;
    protected Modification enter;
    protected Modification exit;

    /* Additional Functionality */

    protected List<Modification> actionMods = new List<Modification>();
    protected List<Modification> enterMods = new List<Modification>();
    protected List<Modification> exitMods = new List<Modification>();


        /* Constructors */


    // Explicitly hide empty constructor.
    protected MovableState() { }

    // Set base behavior on start.
    public MovableState(Movable theUnit)
    {
        unit = theUnit;
        action = BaseAction;
        enter = BaseEnter;
        exit = BaseExit;
    }


        /* State Behavior */
        /* Changes the data of the Movable; aka the stuff that actually makes things happen or not happen.  */


    // Defines on call action.
    public void PerformAction()
    {
        action(unit);
        RunMods(actionMods);
        HandleState();
    }

    // Performed upon entering this state.
    public void OnEnter()
    {
        enter(unit);
        RunMods(enterMods);
    }

    // Performed upon exiting this state.
    protected void OnExit()
    {
        exit(unit);
        RunMods(exitMods);
    }

    // Checks if state should be changed and handles behavior upon switching.
    protected abstract void HandleState();

    // The basic action that this function performs on call.
    protected virtual void BaseAction(Movable theUnit) { }

    // The basic action that this function performs upon entering this state.
    protected virtual void BaseEnter(Movable theUnit) { }

    // The basic action that this function performs upon exiting this state.
    protected virtual void BaseExit(Movable theUnit) { }


        /* Behavior Modification */
        /* Defines how the state tells the Movable to act. This behavior can be changed at any time,
         * though it is BEST FORM to modify it at the beginning of the match upon Movable creation.         */


    // Add a modification to the action performed per call while in this state. Returns true if modification succeeded.
    public virtual bool ModifyAction(Modification mod) { return false; }

    // Add a modification to the action performed upon entering this state. Returns true if modification succeeded.
    public virtual bool ModifyEnter(Modification mod) { return false; }

    // Add a modification to the action performed upon exiting this state. Returns true if modification succeeded.
    public virtual bool ModifyExit(Modification mod) { return false; }

    // Remove a modification to the action performed per call while in this state. Returns true if modification succeeded.
    public virtual bool RemoveAction(Modification mod) { return false; }

    // Remove a modification to the action performed upon entering this state. Returns true if modification succeeded.
    public virtual bool RemoveEnter(Modification mod) { return false; }

    // Remove a modification to the action performed upon exiting this state. Returns true if modification succeeded.
    public virtual bool RemoveExit(Modification mod) { return false; }

    // Overwrites the basic action performed per call while in this state. Returns true if modification succeeded.
    public virtual bool RedefineAction(Modification mod) { return false; }

    // Overwrites the basic action performed upon entering this state. Returns true if modification succeeded.
    public virtual bool RedefineEnter(Modification mod) { return false; }

    // Overwrites the basic action performed upon exiting this state. Returns true if modification succeeded.
    public virtual bool RedefineExit(Modification mod) { return false; }

    // Returns to the base action performed per call while in this state.
    public void ResetAction(Modification mod) { action = BaseAction; }

    // Returns to the base action performed upon entering this state.
    public void ResetEnter(Modification mod) { enter = BaseEnter; }

    // Returns to the base action performed upon exiting this state.
    public void ResetExit(Modification mod) { exit = BaseExit; }


        /* Helper Classes */


    // Helper that clarifies when all mods are being run.
    protected void RunMods(List<Modification> mods) { foreach (Modification mod in mods) { mod(unit); } }

    // Handle callbacks for when a state is changed.
    protected abstract void SwitchState(MovableState theState);

    // Check if the state of the unit is this state already.
    protected abstract bool HasNotSwitched();

    // Check if the state of the unit isn't the same as the state you want to transition to.
    protected abstract bool ShouldSwitch(MovableState theState);
}
