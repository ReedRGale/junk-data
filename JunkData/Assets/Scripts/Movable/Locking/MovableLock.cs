using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovableStructs;

public enum MoveInput { NONE, RIGHT, LEFT };

/*
 * A MovableLock is an abstract parent class defining the functionality of more specific locks.
 * 
 * The general idea of a lock is that there are certain cases that a Movable should suddenly stop
 * moving. But since these behaviors are abnormal or edge cases, they are defined under locks which
 * understand for themselves when the locking should occur. That way, the basic movement function
 * occurring outside this object can proceed normally, but cycle through locks to know if it should
 * halt normal movement and override it with some other behavior.
 */
public abstract class MovableLock
{
    /// <summary>
    /// The event is the PredictedEvent the Scout saw that prompted the creation of the lock.
    /// </summary>
    private PredictedEvent significantEvent;

    /// <summary>
    /// This is the Movable object whose Scout saw a potential relevant event that required a lock
    /// </summary>
    private Movable unit;

    // Explicitly hiding the empty constructor.
    private MovableLock() { }

    /// <summary>
    /// The primary constructor for the movable lock, asking for all data relevant to its formation.
    /// </summary>
    /// <param name="theEvent">The event is the PredictedEvent the Scout saw that prompted the creation of the lock.</param>
    /// <param name="unit">This is the Movable object whose Scout saw a potential relevant event that required a lock</param>
    public MovableLock(PredictedEvent theEvent, Movable theUnit)
    {
        significantEvent = theEvent;
        unit = theUnit;
    }

    /// <summary>
    /// Function that tells external sources whether they should lock their movement or not.
    /// </summary>
    /// <returns>If true, lock movement. If false, don't.</returns>
    public abstract bool ShouldLock();

    /// <summary>
    /// Function that tells external sources whether they should still hold onto this lock or not.
    /// 
    /// For example, if the unit should hang off a ledge, if they've moved more than three times that unit's
    /// width, we may no longer want to have the object hold onto that lock. This function would contain the logic
    /// for whether the lock is still needed.
    /// </summary>
    /// <returns></returns>
    public abstract bool IsRelevant();
}
