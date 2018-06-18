using Extensions;
using UnityEngine;

public class MovablePhysics
{
    Movable unit;


        /* Constructors */

    // Explicitly hide the parameterless constructor.
    private MovablePhysics() {}

    // Generate a local physics object for a movable.
    public MovablePhysics(Movable theUnit) { unit = theUnit; }


        /* Outward-Facing Methods */


    // Representation of the downward force acting on the unit.
    public Vector2 GForce() { return Physics2D.gravity * unit.GetRB2D().gravityScale * unit.GetRB2D().mass; }

    // A hidden delegate of the collision angle.
    public float AngleOfIncline() { return unit.GetCollisionAnalyzer().GetCollisionAngle(); }

    // Cancellation force that undoes the force of gravity angled inward toward the ground.
    public Vector2 NormalForce() { return -PerpendicularGForce(); }

    // Force of gravity perpendicular to the ground.
    public Vector2 PerpendicularGForce()
    { return new Vector2(0, GForce().Rotate(AngleOfIncline()).y).Rotate(-AngleOfIncline()); }

    // Force of gravity parallel to the ground.
    public Vector2 ParallelGForce()
    { return new Vector2(GForce().Rotate(AngleOfIncline()).x, 0).Rotate(-AngleOfIncline()); }
}
