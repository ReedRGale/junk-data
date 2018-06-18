using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structs;


public class MovableScouter
{
    private delegate bool IsPredictedState(RaycastHit2D before, RaycastHit2D after);

    private Movable unit;                                       // The Unit that is looking ahead.
    private int direction;

        /* Constants */

    private const int PREDICTION_SLICES = 10;                   // How many slices to cut the lookahead data into.
    private const float KEENING_VALUE = 0.001f;
    private const float FOCUS_DIST_RATIO = 0.2f;                // How many unit widths should we be starting our fall detection from?
    private const int MAX_KEEN = 20;


        /* Constructors */


    private MovableScouter() { }

    public MovableScouter(Movable theUnit)
    {
        // Initialize locking data.
        unit = theUnit;
    }


        /* Checking System */
    /* Uses the tools we design here to find the edge, then send the locking information. */


    // Call in the Movable's FixedUpdate to look ahead.
    public void LookAhead()
    {
        // If the player isn't grounded, we don't need to make predictions.
        if (!unit.IsGrounded()) return;

        // PredictedEvent potentialDanger = LookForDanger();
        PredictedEvent potentialFall = LookForFall(CheckCloseby(), CheckFaroff());
        // Use data to SetLocks();
    }


        /* Data Collection */

    /* Methods that collect data about what's ahead. */

    // Collect data ahead of the unit stopping distance length beginning from x offset from the rb2d's position.
    private RaycastHit2D[] CastRays(float predictionDistance, float x, int moveDir)
    {
        // Slice up prediction line into even segments to create origin points for our detection lines.
        float pOriginX = predictionDistance / PREDICTION_SLICES;

        // Use origins as x values to generate a line of points to cast rays from.
        RaycastHit2D[] data = new RaycastHit2D[PREDICTION_SLICES];
        for (int i = 0; i < PREDICTION_SLICES; i++)
            data[i] = Physics2D.Raycast(new Vector2(x + (moveDir * pOriginX * i), unit.GetRB2D().position.y), Vector2.down);
        
        return data;
    }

    // Hone the prediction to get a more accurate set of before and after points.
    private PredictedEvent KeenPrediction(RaycastHit2D start, IsPredictedState isState)
    {
        RaycastHit2D beforeKeen = Physics2D.Raycast(new Vector2(start.point.x, unit.GetRB2D().position.y), Vector2.down);
        RaycastHit2D afterKeen = Physics2D.Raycast(new Vector2(start.point.x + (unit.GetMoveInput() * KEENING_VALUE), unit.GetRB2D().position.y), Vector2.down);
        int timesShaved = 0;
        while (!isState(beforeKeen, afterKeen) && timesShaved < MAX_KEEN)
        {
            beforeKeen = afterKeen;
            afterKeen = Physics2D.Raycast(new Vector2(afterKeen.point.x + (unit.GetMoveInput() * KEENING_VALUE), unit.GetRB2D().position.y), Vector2.down);
            timesShaved++;
        }

        return new PredictedEvent(beforeKeen, afterKeen);
    }


        /* Data Analysis and Manipulation */
    /* Use the data to determine various information. */


    // If there's an object that might kill you up ahead, sets wouldDie and the points before and after death.
    //private void LookForDanger(RaycastHit2D[] data)
    //{
    //    wouldDie = false;
    //    int i = 0;
    //    for (i = 0; !wouldDie && i < data.Length; i++)
    //    {
    //        wouldDie = data[i].collider.tag.Equals("DangerZone");
    //        if (wouldDie)
    //        {
    //            KeenPrediction(data[i], RequireDeath, AssignDeath);
    //            return;
    //        }
    //    }
    //}

    // Lookahead for whether you'll fall or not.
    private PredictedEvent LookForFall(RaycastHit2D[] close, RaycastHit2D[] far)
    {
        if (ShouldLookahead())
        {
            PredictedEvent potentialFall = FindPotentialFall(close);
            if (SomethingWillOccur(potentialFall))
                if (FalseAlarm(FirstPointBeyondEvent(potentialFall, far), close, far))
                    return NullEvent();

            return potentialFall;
        }

        return NullEvent();
    }

    // Check if the next step is ascending or decending based on a focus of collision.
    public int GetEscalation()
    {
        RaycastHit2D[] close = CastRays(KEENING_VALUE * 10, FurthestContactFromX().point.x, unit.GetMoveInput());
        return CompareHeightDifference(close[0].point.y, close[1].point.y);
    }

    private bool RequireFall(RaycastHit2D before, RaycastHit2D after) { return IsPastExtreme(before.point.y, after.point.y); }


        /* External Data Manipulation */


        /* Getters */

    // Magnitude of the distance from the fall focus to any potential fall.
    public float GetCheckingMagnitude() { return unit.GetWidth() - (unit.GetWidth() * 0.5f - Mathf.Abs(GetFallFocusDistance())); }

    // Distance and direction from the radius to detect falls.
    public float GetFallFocusDistance()
    {
        if (unit.GetMoveInput() == 0)
            return unit.GetMoveInput(1) * -unit.GetWidth() * FOCUS_DIST_RATIO;
        return unit.GetMoveInput() * -unit.GetWidth() * FOCUS_DIST_RATIO;
    }

    // Distance and direction from the radius to detect falls originating from a specific move state.
    public float GetFallFocusDistance(MoveInput state) { return unit.GetMoveInput() * -unit.GetWidth() * FOCUS_DIST_RATIO; }

    // X value of the fall focus shifted from the rigidbody's X position.
    public float GetFallFocus() { return unit.GetRB2D().position.x + GetFallFocusDistance(); }

    // X value of the fall focus for a specific move state shifted from the rigidbody's X position.
    public float GetFallFocus(MoveInput state) { return unit.GetRB2D().position.x + GetFallFocusDistance(state); }

    // Is the difference in height between the two given points great enough to warrent a potential fall?
    public bool IsPastExtreme(float initY, float nextY) { return initY - nextY > unit.GetRadius(); }


        /* Helper Functions */


    // Return the escalation given the two y values.
    private int CompareHeightDifference(float curr, float next)
    {
        if (curr < next) return 1;          // Ascend
        else if (curr > next) return -1;    // Descend
        else return 0;                      // Level
    }

    // Return the point furthest from the rigidbody's X value.
    private RaycastHit2D FurthestContactFromX()
    {
        RaycastHit2D escalationPoint = new RaycastHit2D();
        float furthestFromX = 0;
        foreach (CircumferenceHit ch in unit.GetCollisionAnalyzer().GetGroundedHits())
        {
            if (DistanceFromX(ch) > furthestFromX)
            {
                furthestFromX = DistanceFromX(ch);
                escalationPoint = ch.hit;
            }
        }

        return escalationPoint;
    }


    // SORT AFTER REFACTOR

    private PredictedEvent FindPotentialFall(RaycastHit2D[] close)
    {
        for (int i = 1; i < close.Length; i++)
            if (IsPastExtreme(close[0].point.y, close[i].point.y))
                return KeenPrediction(close[i - 1], RequireFall);

        return NullEvent();
    }

    private bool ShouldLookahead() { return unit.GetActionState() is MovableWalkingState; }

    private float DistanceFromX(CircumferenceHit ch) { return Mathf.Abs(unit.GetRB2D().position.x - ch.hit.point.x); }

    private RaycastHit2D[] CheckCloseby() { return CastRays(GetCheckingMagnitude(), unit.GetRB2D().position.x + GetFallFocusDistance(), unit.GetMoveInput()); }

    private RaycastHit2D[] CheckFaroff() { return CastRays((GetCheckingMagnitude() + unit.GetWidth()), unit.GetRB2D().position.x + GetFallFocusDistance(), unit.GetMoveInput()); }

    // Checks if the hit collided with something.
    private int FirstPointBeyondEvent(PredictedEvent seenEvent, RaycastHit2D[] scoutingHits)
    {
        for (int i = 0; i < scoutingHits.Length; i++)
            if (BeyondEvent(seenEvent, scoutingHits[i].point.x))
                return i;
        return -1;
    }

    private bool SomethingWillOccur(PredictedEvent seenEvent) { return seenEvent.preEvent.transform != null; }

    private bool BeyondEvent(PredictedEvent seenEvent, float unitX)
    {
        return unit.GetMoveInput() < 0 && seenEvent.postEvent.point.x > unitX
            || unit.GetMoveInput() > 0 && seenEvent.postEvent.point.x < unitX;
    }

    private PredictedEvent NullEvent() { return new PredictedEvent(new RaycastHit2D(), new RaycastHit2D()); }

    private bool WithinRecordedPoints(int i, RaycastHit2D[] far) { return i < far.Length; }

    private bool FalseAlarm(int pointBeyond, RaycastHit2D[] close, RaycastHit2D[] far)
    {
        for (int i = 0; WithinRecordedPoints(pointBeyond + i, far); i++)
            if (!IsPastExtreme(close[0].point.y, far[pointBeyond + i].point.y))
                return true;
        return false;
    }
}
