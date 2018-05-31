using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structs;
using Extensions;
using System;

public class MovableCollisionAnalyzer
{
        /* Fields */
    
    private Movable unit;
    private double currentRecord = 0;

        /* Constants */

    private const float RIGHT_ANGLE = 90f;
    private Vector2 MIN_CONTACT_RANGE = new Vector2(-0.8660254f, -0.5f);
    private Vector2 MAX_CONTACT_RANGE = new Vector2(0.8660254f, -0.5f);
    private const float DELTA_CONTACT_ANGLE = 0.5f;
    private const float RADIUS_CORRECTION = 0.01f;
    private Dictionary<double, CollisionRecord> hitRecords = new Dictionary<double, CollisionRecord>();


        /* Constructors */

    // Explicitly hide the empty constructor.
    private MovableCollisionAnalyzer() { }

    // Each MCA is linked to its unit.
    public MovableCollisionAnalyzer(Movable theUnit) { unit = theUnit; }

        /* Outward-Facing Functions */

        
    // Increments the cycle value.
    public void IncrementCurrentCycle()
    {
        currentRecord++;
        hitRecords[currentRecord] = new CollisionRecord();
    }

    // Return the walk vector.
    public Vector2 GetBaseWalkVector() { return new Vector2(unit.GetMoveXInput() * WalkX(), unit.GetEscalation() * WalkY()); }

    public List<CircumferenceHit> GetGroundedHits()
    {
        // If we already have a record of this, don't reperform the algorithm.
        if (HitsRecorded()) return hitRecords[currentRecord].GetGroundedHits();

        List<CircumferenceHit> hits = CollectInexactHits();
        if (hits.Count > 0) { hits = GleanExactHits(hits); }

        // Record hits for future use.
        if (hitRecords.ContainsKey(currentRecord)) hitRecords[currentRecord].SetGroundedHits(hits);
        else hitRecords[currentRecord] = new CollisionRecord(hits);

        return hits;
    }

    // Returns whether the unit is grounded or not. If called from an external source, force an increment of the cycle.
    public bool IsGrounded(bool fromExternal)
    {
        if (fromExternal) IncrementCurrentCycle();
        return GetGroundedHits().Count > 0;
    }

    // Get the collision angle, measured from Vector2.down.
    public float GetCollisionAngle()
    {
        // If we already have a record of this, don't reperform the algorithm.
        if (AngleRecorded()) return hitRecords[currentRecord].GetCollisionAngle();

        // Assume colliding with ground initially.
        float collisionAngle = 0;

        // Iterate through all contacts and set the collision angle to the greatest value.
        List<CircumferenceHit> contactList = GetGroundedHits();
        foreach (CircumferenceHit ch in contactList)
        {
            Vector2 length = ch.hit.point - unit.GetRB2D().position;
            float angle = AngleOfIncline(ch.hit, length);
            collisionAngle = Mathf.Abs(angle) > collisionAngle ? angle : collisionAngle;
        }

        // Record angle for future use.
        if (hitRecords.ContainsKey(currentRecord)) hitRecords[currentRecord].SetCollisionAngle(collisionAngle);
        else hitRecords[currentRecord] = new CollisionRecord(collisionAngle);

        return collisionAngle;
    }


        /* Helper Functions */


    // Finds the proper angle.
    private float AngleOfIncline(RaycastHit2D hit, Vector2 length)
    {
        if (hit.point.x < unit.GetRB2D().position.x) return Vector2.Angle(length, Vector2.down);
        else return -Vector2.Angle(length, Vector2.down);
    }

    // Cuts out clusters from a list of hits.
    private List<List<CircumferenceHit>> DetermineHitClusters(List<CircumferenceHit> hitList, float clusterDeviance)
    {
        // Check list for clusters and store them.
        List<List<CircumferenceHit>> clusters = new List<List<CircumferenceHit>>();

        // If there's only one contact point, there's nothing to compare.
        if (hitList.Count == 1) { clusters.Add(hitList); }
        else
        {
            List<CircumferenceHit> cluster = new List<CircumferenceHit>();
            for (int i = 1; i < hitList.Count; i++)
                if (WithinClusterDeviance(hitList, i, clusterDeviance))
                    cluster.Add(hitList[i - 1]);
                else
                {
                    cluster.Add(hitList[i - 1]);
                    clusters.Add(cluster);
                    cluster = new List<CircumferenceHit>();
                }
            ManageFinalClusterHit(ref cluster, ref clusters, hitList);
        }

        return clusters;
    }

    // Collect information about inexact groups of hitclusters.
    private List<CircumferenceHit> CollectInexactHits()
    {
        List<CircumferenceHit> hits = new List<CircumferenceHit>();
        Vector2 direction = MIN_CONTACT_RANGE;
        float angle = 0;
        while (direction.x <= MAX_CONTACT_RANGE.x)
        {
            // Record raycast if it is within contact radius.
            CircumferenceHit cHit = new CircumferenceHit(Physics2D.Raycast(unit.GetRB2D().position, direction), angle, MIN_CONTACT_RANGE);
            if (CollisionOccurred(cHit) && cHit.hit.distance <= unit.GetRadius() + RADIUS_CORRECTION) { hits.Add(cHit); }

            // Increment Angle
            angle += DELTA_CONTACT_ANGLE;
            direction = direction.Rotate(DELTA_CONTACT_ANGLE);
        }

        return hits;
    }

    // Takes a list of clusters of hits and culls it down to only the most relevant hits.
    private List<CircumferenceHit> GleanExactHits(List<CircumferenceHit> list)
    {
        // Determine clusters.
        List<List<CircumferenceHit>> clusters = DetermineHitClusters(list, 3 * DELTA_CONTACT_ANGLE);

        // Retrieve the collision closest to the circumference.
        List<CircumferenceHit> exactHits = new List<CircumferenceHit>();
        foreach (List<CircumferenceHit> c in clusters) { exactHits.Add(GleanExactHit(c)); }

        return exactHits;
    }

    // Checks if, within a list, at a given index whether it's preceding element is within cluster deviation.
    private bool WithinClusterDeviance(List<CircumferenceHit> list, int index, float clusterDeviance)
    {
        return Mathf.Abs(list[index].angle - list[index - 1].angle) <= clusterDeviance;
    }

    // Ties up loose ends involving the final value of the cluster.
    private void ManageFinalClusterHit(ref List<CircumferenceHit> cluster, ref List<List<CircumferenceHit>> clusters, List<CircumferenceHit> hitList)
    {
        if (cluster.Count == 0) return;

        cluster.Add(hitList[hitList.Count - 1]);
        clusters.Add(cluster);
    }

    // Takes a cluster and finds the hit closest to the center of the collider.
    private CircumferenceHit GleanExactHit(List<CircumferenceHit> list)
    {
        CircumferenceHit exactHit = list[0];
        for (int i = 1; i < list.Count; i++)
            if (list[i].hit.distance < list[i - 1].hit.distance)
                exactHit = list[i];

        return exactHit;
    }

    // Convert angle to Y Force.
    private float WalkY()
    { return Mathf.Abs(GetCollisionAngle()) < RIGHT_ANGLE ? 1f * (Mathf.Abs(GetCollisionAngle()) / RIGHT_ANGLE) : 0; }

    // Convert angle to X Force.
    private float WalkX()
    { return Mathf.Abs(GetCollisionAngle()) < RIGHT_ANGLE ? (RIGHT_ANGLE - Mathf.Abs(GetCollisionAngle())) / RIGHT_ANGLE : 0; }

    // Checks if the hit collided with something.
    private bool CollisionOccurred(CircumferenceHit cHit) { return cHit.hit.collider != null; }

    // Checks if the grounded hits have already been recorded.
    private bool HitsRecorded() { return    hitRecords.ContainsKey(currentRecord) && 
                                            hitRecords[currentRecord].GetGroundedHits() != null; }

    // Checks if the angle has already been recorded
    private bool AngleRecorded() { return   hitRecords.ContainsKey(currentRecord) && 
                                            !float.IsNaN(hitRecords[currentRecord].GetCollisionAngle()); }

}
