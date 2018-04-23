using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LockState { RIGHT, LEFT, UP, DOWN };

public class PredictionTool : MonoBehaviour
{
    [HideInInspector] public List<LockState> lockStates;                         // All directions currently locked.
    [HideInInspector]  public Dictionary<LockState, RaycastHit2D> lockData;     // Extra information about why a dirction is locked.

    private bool wouldDie;                                      // Is 'death' a piece of data that means something?
    private bool wouldFall;                                     // Is 'fall' a piece of data that means something?
    private int direction;
    private RaycastHit2D predeath = new RaycastHit2D();         // The last safe point before the next potential threat.
    private RaycastHit2D postdeath = new RaycastHit2D();         // The last safe point before the next potential threat.
    private RaycastHit2D prefall = new RaycastHit2D();          // The last safe point before the next potential fall.
    private RaycastHit2D postfall = new RaycastHit2D();          // The last safe point before the next potential fall.
    private Player player;
    private float width;                                        // The width of the collider.
    private Rigidbody2D rb2d;
    private float detectionLength;
    
    private const int PREDICTION_SLICES = 10;                    // How many slices to cut the lookahead data into.
    private const float ERROR_RANGE = 0.01f;
    private const float KEENING_VALUE = 0.001f;
    private const float CHECK_FROM = 0.5f;

    void Start()
    {
        // Initialize locking data.
        lockStates = new List<LockState>();
        lockData = new Dictionary<LockState, RaycastHit2D>();

        rb2d = gameObject.GetComponent<Rigidbody2D>();
        width = gameObject.GetComponent<Collider2D>().bounds.extents.x * 2;
        player = gameObject.GetComponent<Player>();
        detectionLength = width;
    }

    void FixedUpdate()
    {
        // If the player isn't moving and is grounded, we don't want to undo any settings.
        if (Mathf.Abs(rb2d.velocity.x) < float.Epsilon || !player.grounded) return;

        // Reset bools.
        wouldDie = false;
        wouldFall = false;

        // Collect and analyze data from raycasts.
        SetDirectionLocks(  CastRays(2 * StoppingDistance(), GetFallFocus()), 
                            CastRays(2 * (StoppingDistance() + width), GetFallFocus()));
    }

    // Collect data ahead of the unit stopping distance length beginning from x offset from the rb2d's position.
    private RaycastHit2D[] CastRays(float predictionDistance, float offset)
    {
        // Slice up prediction line into even segments to create origin points for our detection lines.
        float pOriginX = predictionDistance / PREDICTION_SLICES;

        // Prepare an array to hold all RaycastHits.
        RaycastHit2D[] data = new RaycastHit2D[PREDICTION_SLICES];

        // Use origins as x values to generate a line of points to cast rays from.
        for (int i = 0; i < PREDICTION_SLICES; i++)
        {
            data[i] = Physics2D.Raycast(new Vector2(rb2d.position.x + offset + (MoveDir() * pOriginX * i), rb2d.position.y), Vector2.down);

            // DEBUG:
            Debug.DrawRay(new Vector2(rb2d.position.x + offset + (MoveDir() * pOriginX * i), rb2d.position.y), Vector2.down, Color.green, 0.05f);
        }

        // Cast rays and return them in an array.
        return data;
    }

    // Take the data collected and set locking information.
    private void SetDirectionLocks(RaycastHit2D[] data, RaycastHit2D[] lookahead)
    {
        // LookForDanger(lookahead);  // Needs testing.
        LookForFall(data, lookahead);
        SetLocks();     
    }
    
    // If there's an object that might kill you up ahead, sets wouldDie and the points before and after death.
    private void LookForDanger(RaycastHit2D[] data)
    {
        wouldDie = false;
        int i = 0;
        for (i = 0; !wouldDie && i < data.Length; i++)
        {
            wouldDie = data[i].collider.tag.Equals("DangerZone");
            if (wouldDie)
            {
                KeenPrediction(data[i], RequireDeath, AssignDeath);
                return;
            }
        }
    }

    // Lookahead for whether you'll fall or not.
    private void LookForFall(RaycastHit2D[] data, RaycastHit2D[] lookahead)
    {
        // Analyze data given and convert it to useful forms.
        int pointsToPlayerWidth = AnalyzeFallLookahead(lookahead);

        // Check for potential falls.
        for (int i = 0; i < data.Length; i++)
            if (PastExtreme(data[i].point.y))
            {
                KeenPrediction(data[i], RequireFall, AssignFall);
                break;
            }

        // Simulate the shape beyond an extreme angle to determine if the unit would fall into it.
        if (wouldFall)
        {
            // Get the first point in the lookahead that's within extreme change range.
            int lookaheadFall = 0;
            for (int i = 0; i < lookahead.Length; i++)
                if (MoveDir() < 0 && postfall.point.x > lookahead[i].point.x || MoveDir() > 0 && postfall.point.x < lookahead[i].point.x)
                {
                    lookaheadFall = i;
                    break;
                }

            // Look beyond the unit's width beyond the fall point.
            for (int i = 0; i < pointsToPlayerWidth && lookaheadFall + i < lookahead.Length; i++)
                // If the data says there are any points we experience where an extreme change isn't registered, we're not in danger of falling.
                if (!PastExtreme(lookahead[lookaheadFall + i].point.y))
                {
                    wouldFall = false;
                    break;
                }
        }
    }

    // Return the length just shy of the player's width in prediction slices.
    private int AnalyzeFallLookahead(RaycastHit2D[] lookahead)
    {
        // Get the x value between any two points.
        float deltaX = lookahead[0].point.x - lookahead[1].point.x;

        // Return the width of the player divided by delta x rounded down.
        return (int)Mathf.Abs(Mathf.Floor(width / deltaX));
    }

    // Determine which directions should be locked.
    private void SetLocks()
    {
        if (wouldFall || wouldDie)
        {
            // If the fall point's x magnitude outstrips the danger point's magnitude...
            RaycastHit2D closestDoom = Mathf.Abs(GetFallFocus() - postfall.point.x) < Mathf.Abs(GetFallFocus() - postdeath.point.x) ? postfall : postdeath;

            // Determine which direction to lock movement.
            if (GetFallFocus() - closestDoom.point.x > 0)
                SetLock(LockState.LEFT, closestDoom);
            else
                SetLock(LockState.RIGHT, closestDoom);
        }
        else CheckUnlock();
    }

    // Set a particular direction as locked and record its hit data.
    private void SetLock(LockState state, RaycastHit2D stateData)
    {
        lockStates.Add(state);
        lockData.Remove(state);
        lockData.Add(state, stateData);
    }

    // Unlock a given state and clear its hit data.
    private void Unlock(LockState state)
    {
        lockStates.Remove(state);
        lockData.Remove(state);
    }

    // Check if any of the locking states don't need to be locked anymore.
    private void CheckUnlock()
    {
        // Prepare a list to store which states to unlock.
        List<LockState> states = new List<LockState>();

        // If fall focus isn't within the hit error range, unlock it.
        foreach (LockState s in lockStates)
            if (!InLockRange(s))
                states.Add(s);
        foreach (LockState s in states)
            Unlock(s);
    }

    // Check if a hit is within lockrange.
    private bool InLockRange(LockState state)
    {
        RaycastHit2D hit;
        lockData.TryGetValue(state, out hit);
        return hit.point.x >= GetFallFocus() - ERROR_RANGE && hit.point.x <= GetFallFocus() + ERROR_RANGE;
    }

    // Hone the prediction to get a more accurate set of before and after points.
    private delegate bool PredictionRequrements(RaycastHit2D before, RaycastHit2D after); // When 'before' becomes 'after'
    private delegate void AssignHits(RaycastHit2D before, RaycastHit2D after); // Assign 'before' and 'after' to the proper vars.
    private void KeenPrediction(RaycastHit2D start, PredictionRequrements req, AssignHits assign)
    {
        RaycastHit2D beforeKeen = Physics2D.Raycast(new Vector2(start.point.x, rb2d.position.y), Vector2.down);
        RaycastHit2D afterKeen = Physics2D.Raycast(new Vector2(start.point.x + (MoveDir() * KEENING_VALUE), rb2d.position.y), Vector2.down);

        // DEBUG:
        Debug.DrawRay(new Vector2(start.point.x + (MoveDir() * KEENING_VALUE), rb2d.position.y), Vector2.down, Color.green, 0.05f);

        // Shave off values until a very close changing point is reached.
        while (!req(beforeKeen, afterKeen))
        {
            beforeKeen = afterKeen;
            afterKeen = Physics2D.Raycast(new Vector2(afterKeen.point.x + (MoveDir() * KEENING_VALUE), rb2d.position.y), Vector2.down);

            // DEBUG:
            Debug.DrawRay(new Vector2(afterKeen.point.x + (MoveDir() * KEENING_VALUE), rb2d.position.y), Vector2.down, Color.green, 0.05f);
        }

        assign(beforeKeen, afterKeen);
    }

    private bool RequireDeath(RaycastHit2D before, RaycastHit2D after)
    {
        return !before.collider.tag.Equals("DangerZone") && after.collider.tag.Equals("DangerZone");
    }

    private bool RequireFall(RaycastHit2D before, RaycastHit2D after)
    {
        return PastExtreme(after.point.y);
    }

    private void AssignDeath(RaycastHit2D before, RaycastHit2D after)
    {
        predeath = before;
        postdeath = after;
        wouldDie = true;
    }

    private void AssignFall(RaycastHit2D before, RaycastHit2D after)
    {
        prefall = before;
        postfall = after;
        wouldFall = true;
    }

    // Calculate the stopping distance for our prediction. [KE = (1/2)mv^2]
    private float StoppingDistance() { return 0.5f * rb2d.mass * Mathf.Pow(rb2d.velocity.x * 1.1f, 2); }

    // Helper function to determine the direction of the player.
    // 0:  Not Moving
    // 1:  Moving Right
    // -1: Moving Left
    private int MoveDir() { return rb2d.velocity.x < 0 ? -1 : 1; }

    // Value to check falls from.
    private float GetFallFocus() { return MoveDir() * -width * CHECK_FROM;  }

    private bool PastExtreme(float y) { return rb2d.position.y - y > detectionLength; }
}
