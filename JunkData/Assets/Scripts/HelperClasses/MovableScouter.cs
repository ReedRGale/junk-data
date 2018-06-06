using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structs;


public class MovableScouter
{
    public List<MoveInput> lockStates;                                    // All directions currently locked.
    public Dictionary<MoveInput, RaycastHit2D> potentialLockData;         // Future information about locks that could happen.

    private delegate bool ShouldUnlock(RaycastHit2D hitData);
    private delegate bool IsPredictedState(RaycastHit2D before, RaycastHit2D after);           // When 'before' becomes 'after'
    private delegate void AssignHit(RaycastHit2D before, RaycastHit2D after);              // Assign 'before' and 'after' to the proper vars.

    private Movable unit;                                       // The Unit that is looking ahead.
    private MoveInput prevMoveState = MoveInput.NONE;         // Last check, what direction were we moving?
    private Dictionary<MoveInput, ShouldUnlock> unlockReqs;     // Set of delegates that contain the information whether to unlock a lockState.
    private int direction;
    private RaycastHit2D predeath;                              // The last safe point before the next potential threat.
    private RaycastHit2D postdeath;                             // The first danger point before the next potential threat.
    private RaycastHit2D prefall;                               // The last safe point before the next potential fall.
    private RaycastHit2D postfall;                              // The first danger point before the next potential fall.
    private float fallDetectionLength;
    private Queue<int> moveDirQueue;

        /* Constants */

    private const int PREDICTION_SLICES = 10;                   // How many slices to cut the lookahead data into.
    private const float KEENING_VALUE = 0.001f;
    private const float FOCUS_DIST_RATIO = 0.2f;                // How many unit widths should we be starting our fall detection from?
    private RaycastHit2D NULL_HIT;
    private const int MAX_KEEN = 20;

    private const string UNSUPPORTED_MOVESTATE = "This MoveState is not yet supported for this function.";


        /* Constructors */


    private MovableScouter() { }

    public MovableScouter(Movable theUnit)
    {
        // Initialize locking data.
        unit = theUnit;
        lockStates = new List<MoveInput>();
        potentialLockData = new Dictionary<MoveInput, RaycastHit2D>();
        unlockReqs = new Dictionary<MoveInput, ShouldUnlock>();
        NULL_HIT = new RaycastHit2D();
        NULL_HIT.point = new Vector2(float.MaxValue, float.MaxValue);
        predeath = NULL_HIT;
        postdeath = NULL_HIT;
        prefall = NULL_HIT;
        postfall = NULL_HIT;

        // Prepare previous move direction data queue.
        moveDirQueue = new Queue<int>();
        moveDirQueue.Enqueue(int.MaxValue);
        moveDirQueue.Enqueue(int.MaxValue);

        // Initialize unlock requirements.
        unlockReqs.Add(MoveInput.LEFT, ShouldLeftBeUnlocked);
        unlockReqs.Add(MoveInput.RIGHT, ShouldRightBeUnlocked);

        // Initialize player unit data.
        fallDetectionLength = unit.GetRadius();
    }

        /* Checking System */
        /* Uses the tools we design here to find the edge, then send the locking information. */


    // Call in the Movable's FixedUpdate to look ahead.
    public void LookAhead()
    {
        // Check if new move data is available.
        RecordMoveDir();

        // If the player isn't grounded, we don't need to make predictions.
        if (!unit.IsGrounded()) return;

        // Collect and analyze data from close and far raycasts.
        Contemplate(CastRays(GetCheckingMagnitude(), GetFallFocusDistance(), GetMoveDir()),
                    CastRays((GetCheckingMagnitude() + unit.GetWidth()), GetFallFocusDistance(), GetMoveDir()));

        // Record move state data for next loop.
        RecordMoveState();
    }


        /* Data Collection */

    /* Methods that collect data about what's ahead. */

    // Collect data ahead of the unit stopping distance length beginning from x offset from the rb2d's position.
    private RaycastHit2D[] CastRays(float predictionDistance, float offset, int moveDir, bool startFromOffset=false)
    {
        // Slice up prediction line into even segments to create origin points for our detection lines.
        float pOriginX = predictionDistance / PREDICTION_SLICES;

        // Use origins as x values to generate a line of points to cast rays from.
        RaycastHit2D[] data = new RaycastHit2D[PREDICTION_SLICES];
        for (int i = 0; i < PREDICTION_SLICES; i++)
            if (startFromOffset)
                data[i] = Physics2D.Raycast(new Vector2(offset + (moveDir * pOriginX * i), unit.GetRB2D().position.y), Vector2.down);
            else
                data[i] = Physics2D.Raycast(new Vector2(unit.GetRB2D().position.x + offset + (moveDir * pOriginX * i), unit.GetRB2D().position.y), Vector2.down);
        
        return data;
    }

    // Hone the prediction to get a more accurate set of before and after points.
    private void KeenPrediction(RaycastHit2D start, IsPredictedState isState, AssignHit assign)
    {
        RaycastHit2D beforeKeen = Physics2D.Raycast(new Vector2(start.point.x, unit.GetRB2D().position.y), Vector2.down);
        RaycastHit2D afterKeen = Physics2D.Raycast(new Vector2(start.point.x + (GetMoveDir() * KEENING_VALUE), unit.GetRB2D().position.y), Vector2.down);

        // Shave off values until a very close changing point is reached; if none is found before max_keen, assign no lock data.
        int timesShaved = 0;
        while (!isState(beforeKeen, afterKeen) && timesShaved < MAX_KEEN)
        {
            beforeKeen = afterKeen;
            afterKeen = Physics2D.Raycast(new Vector2(afterKeen.point.x + (GetMoveDir() * KEENING_VALUE), unit.GetRB2D().position.y), Vector2.down);
            timesShaved++;

            // DEBUG:
            Debug.DrawRay(new Vector2(afterKeen.point.x + (GetMoveDir() * KEENING_VALUE), unit.GetRB2D().position.y), Vector2.down, Color.green, 0.5f);
        }

        if (timesShaved < MAX_KEEN) assign(beforeKeen, afterKeen);
    }


        /* Data Analysis and Manipulation */
    /* Use the data to determine various information. */


    // Take the data collected and turn it into useful forms.
    private void Contemplate(RaycastHit2D[] close, RaycastHit2D[] far)
    {
        // LookForDanger(lookahead);  // Needs testing.
        LookForFall(close, far);
        SetLocks();
    }

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
    private void LookForFall(RaycastHit2D[] close, RaycastHit2D[] far)
    {
        // Don't lookahead if you already have a hit for this direction.
        if (unit.GetActionState() is MovableWalkingState && (MoveStateChanged() || !LockDataIsRelevant()))
        {
            // Check for potential falls.
            for (int i = 0; i < close.Length; i++)
                if (IsPastExtreme(close[0].point.y, close[i].point.y))
                {
                    if (i != 0) KeenPrediction(close[i - 1], RequireFall, AssignFall);
                    else KeenPrediction(close[i], RequireFall, AssignFall);
                    break;
                }

            // Simulate the shape beyond an extreme angle to determine if the unit would fall into it.
            if (LockDataIsRelevant())
            {
                // Get the first point in the lookahead that's within extreme change range.
                int lookaheadFall = 0;
                for (int i = 0; i < far.Length; i++)
                    if (GetMoveDir() < 0 && postfall.point.x > far[i].point.x || GetMoveDir() > 0 && postfall.point.x < far[i].point.x)
                    {
                        lookaheadFall = i;
                        break;
                    }

                // Look beyond the unit's width beyond the fall point.
                int pointsToPlayerWidth = FindPointsToPlayerWidth(far);
                for (int i = 0; i < pointsToPlayerWidth && lookaheadFall + i < far.Length; i++)
                    // If the data says there are any points we experience where an extreme change isn't registered, we're not in danger of falling.
                    if (!IsPastExtreme(close[0].point.y, far[lookaheadFall + i].point.y))
                    {
                        Debug.Log("I saw ahead and figured I'd remove data...?");
                        Debug.DrawRay(new Vector2(unit.GetRB2D().position.x, close[0].point.y), Vector2.right, Color.red, 1f);
                        Debug.DrawRay(new Vector2(unit.GetRB2D().position.x, far[lookaheadFall + i].point.y), Vector2.right, Color.red, 1f);

                        potentialLockData.Remove(GetCurrentMoveState());
                        lockStates.Remove(GetCurrentMoveState());
                        break;
                    }
            }
        }
    }

    // Check if the next step is ascending or decending based on a focus of collision.
    public int GetEscalation()
    {
        // Get furthest collision in case of multiple collisions.
        RaycastHit2D furthestCollision = FindCollisionFurthestFromX();

        // Compare current location to the next location to find escalation type.
        RaycastHit2D[] close = CastRays(KEENING_VALUE * 10, furthestCollision.point.x, unit.GetMoveInput(), true);

        return CompareHeightDifference(close[0].point.y, close[1].point.y);
    }

        /* UnlockReq Delegates */
        
    private bool ShouldLeftBeUnlocked(RaycastHit2D hit) { return GetFallFocus(MoveInput.LEFT) > hit.point.x; }
    private bool ShouldRightBeUnlocked(RaycastHit2D hit) { return GetFallFocus(MoveInput.RIGHT) < hit.point.x; }

        /* UnlockReq Delegates */

    private bool RequireDeath(RaycastHit2D before, RaycastHit2D after) { return !before.collider.tag.Equals("DangerZone") && after.collider.tag.Equals("DangerZone"); }
    private bool RequireFall(RaycastHit2D before, RaycastHit2D after) { return IsPastExtreme(before.point.y, after.point.y); }

        /* AssignHit Delegates */

    private void AssignDeath(RaycastHit2D before, RaycastHit2D after)
    {
        // Set data about death point.
        predeath = before;
        postdeath = after;

        // Set locking data.
        potentialLockData.Remove(GetCurrentMoveState());
        potentialLockData.Add(GetCurrentMoveState(), before);
    }
    private void AssignFall(RaycastHit2D before, RaycastHit2D after)
    {
        // Set data about fall point.
        prefall = before;
        postfall = after;

        // Set locking data.
        potentialLockData.Remove(GetCurrentMoveState());
        potentialLockData.Add(GetCurrentMoveState(), before);
    }


        


        /* External Data Manipulation */

    
        /* Getters */
    // Helper function to determine the direction of the player.
    // 0:  Not Moving
    // 1:  Moving Right
    // -1: Moving Left
    public int GetMoveDir() { return unit.GetRB2D().velocity.x == 0 ? 0 : (int)(Mathf.Sign(unit.GetRB2D().velocity.x)); }

    // Check the move direction queue to see what the last move type was.
    public int GetPrevMoveDir() { return moveDirQueue.Peek(); }

    // Magnitude of the distance from the fall focus to any potential fall.
    public float GetCheckingMagnitude() { return unit.GetWidth() - (unit.GetWidth() * 0.5f - Mathf.Abs(GetFallFocusDistance())); }

    // Distance and direction from the radius to detect falls.
    public float GetFallFocusDistance()
    {
        if (GetMoveDir() == 0)
            return GetPrevMoveDir() * -unit.GetWidth() * FOCUS_DIST_RATIO;
        return GetMoveDir() * -unit.GetWidth() * FOCUS_DIST_RATIO;
    }

    // Distance and direction from the radius to detect falls originating from a specific move state.
    public float GetFallFocusDistance(MoveInput state)
    {
        switch (state)
        {
            case MoveInput.LEFT:
                return unit.GetWidth() * FOCUS_DIST_RATIO;
            case MoveInput.RIGHT:
                return -unit.GetWidth() * FOCUS_DIST_RATIO;
        }
        throw new ArgumentException(UNSUPPORTED_MOVESTATE);
    }

    // X value of the fall focus shifted from the rigidbody's X position.
    public float GetFallFocus() { return unit.GetRB2D().position.x + GetFallFocusDistance(); }

    // X value of the fall focus for a specific move state shifted from the rigidbody's X position.
    public float GetFallFocus(MoveInput state) { return unit.GetRB2D().position.x + GetFallFocusDistance(state); }

    // Is the difference in height between the two given points great enough to warrent a potential fall?
    public bool IsPastExtreme(float initY, float nextY) { return initY - nextY > fallDetectionLength; }

    // Is the move state the same this update as it was last update?
    public bool MoveStateChanged() { return prevMoveState != GetCurrentMoveState(); }

    // Return the current move state.
    private MoveInput GetCurrentMoveState()
    {
        MoveInput currentMoveState = prevMoveState;
        if (GetMoveDir() != 0)
            currentMoveState = GetMoveDir() < 0 ? MoveInput.LEFT : MoveInput.RIGHT;
        else if (potentialLockData.Count == 0)
            currentMoveState = MoveInput.NONE;
        return currentMoveState;
    }


        /* Helper Functions */

    // Return the escalation given the two y values.
    private int CompareHeightDifference(float curr, float next)
    {
        if (curr < next) return 1;          // Ascend
        else if (curr > next) return -1;    // Descend
        else return 0;                      // Level
    }

    // Return the length just shy of the player's width in prediction slices.
    private int FindPointsToPlayerWidth(RaycastHit2D[] lookahead)
    {
        // Get the x value between any two points.
        float deltaX = lookahead[0].point.x - lookahead[1].point.x;

        // Return the width of the player divided by delta x rounded down.
        return (int)Mathf.Abs(Mathf.Floor(unit.GetWidth() / deltaX));
    }

    // Return the point furthest from the rigidbody's X value.
    private RaycastHit2D FindCollisionFurthestFromX()
    {
        RaycastHit2D escalationPoint = new RaycastHit2D();
        float furthestFromX = 0;
        foreach (CircumferenceHit ch in unit.GetCollisionAnalyzer().GetGroundedHits())
        {

            Color debugColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            Debug.DrawRay(ch.hit.point, Vector2.up, debugColor, 0.05f);

            if (DistanceFromX(ch) > furthestFromX)
            {
                furthestFromX = DistanceFromX(ch);
                escalationPoint = ch.hit;
            }
        }

        return escalationPoint;
    }

    // Records the current move state for the next physics check.
    private void RecordMoveDir()
    {
        // Prepares to read the back of the queue.
        Queue<int>.Enumerator moveDirEnum = moveDirQueue.GetEnumerator();
        moveDirEnum.MoveNext();
        moveDirEnum.MoveNext();

        // Update the MoveDir Queue.
        if (GetMoveDir() != moveDirEnum.Current)
        {
            moveDirQueue.Dequeue();
            moveDirQueue.Enqueue(GetMoveDir());
        }
    }

    // Records the current move state for the next physics check.
    private void RecordMoveState() { prevMoveState = GetCurrentMoveState(); }

    private float DistanceFromX(CircumferenceHit ch) { return Mathf.Abs(unit.GetRB2D().position.x - ch.hit.point.x); }
}
