using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structs;

public class CollisionRecord
{
    // The stored GroundedHits.
    private List<CircumferenceHit> groundedHits;

    // The greatest angle from Vector2.down that this is colliding with this unit this cycle.
    private float collisionAngle;
    
    public CollisionRecord()
    {
        groundedHits = null;
        collisionAngle = float.NaN;
    }

    public CollisionRecord(List<CircumferenceHit> theHits)
    {
        groundedHits = theHits;
        collisionAngle = float.NaN;
    }

    public CollisionRecord(float theAngle)
    {
        groundedHits = null;
        collisionAngle = theAngle;
    }

    public CollisionRecord(List<CircumferenceHit> theHits, float theAngle)
    {
        groundedHits = theHits;
        collisionAngle = theAngle;
    }

    public void SetGroundedHits(List<CircumferenceHit> theHits)
    {
        if (groundedHits == null) groundedHits = theHits;
        else
            Debug.Log("Tried to assign to nonnull hits list within the cycle.");
    }

    public void SetCollisionAngle(float theAngle)
    {
        if (float.IsNaN(collisionAngle)) collisionAngle = theAngle;
        else
            Debug.Log("Tried to assign to an assigned angle within the cycle.");
    }

    public List<CircumferenceHit> GetGroundedHits() { return groundedHits; }
    public float GetCollisionAngle() { return collisionAngle; }
}

