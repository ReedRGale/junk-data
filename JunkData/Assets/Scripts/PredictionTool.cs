using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictionTool : MonoBehaviour
{
    public bool rightLocked;
    public bool leftLocked;
    [HideInInspector] public bool wouldDie;                     // Is 'death' a piece of data that means something?
    [HideInInspector] public bool wouldFall;                    // Is 'fall' a piece of data that means something?

    private int direction;
    private RaycastHit2D predeath = new RaycastHit2D();         // The last safe point before the next potential threat.
    private RaycastHit2D postdeath = new RaycastHit2D();         // The last safe point before the next potential threat.
    private RaycastHit2D prefall = new RaycastHit2D();          // The last safe point before the next potential fall.
    private RaycastHit2D postfall = new RaycastHit2D();          // The last safe point before the next potential fall.
    private Player player;
    private float width;                                        // The width of the collider.
    private Rigidbody2D rb2d;

    public const int PREDICTION_SLICES = 10;                    // How many slices to cut the lookahead data into.

    // CHANGE THIS TO SCALE WITH RADIUS
    private const float EXTREME_CHANGE = 0.01f;                 // Units between two height values before we can presume a fall.

    void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        width = gameObject.GetComponent<Collider2D>().bounds.extents.x * 2;
        player = gameObject.GetComponent<Player>();
    }

    void FixedUpdate()
    {
        // If the player isn't moving and is grounded, we don't want to undo any settings.
        if (Mathf.Abs(rb2d.velocity.x) < float.Epsilon || !player.grounded) return;

        // Reset bools.
        wouldDie = false;
        wouldFall = false;

        // Collect and analyze data from raycasts.
        SetDirectionLocks(CastRays(2 * StoppingDistance(), PREDICTION_SLICES), CastRays(2 * (StoppingDistance() + width), PREDICTION_SLICES));
    }

    // Collect data ahead of the unit.
    private RaycastHit2D[] CastRays(float predictionDistance, float slices)
    {
        // Slice up prediction line into even segments to create origin points for our detection lines.
        float pOriginX = predictionDistance / slices;

        // Prepare an array to hold all RaycastHits.
        RaycastHit2D[] data = new RaycastHit2D[PREDICTION_SLICES];

        // Use origins as x values to generate a line of points to cast rays from.
        for (int i = 0; i < PREDICTION_SLICES; i++)
        {
            data[i] = Physics2D.Raycast(new Vector2(rb2d.position.x + (MoveDir() * pOriginX * i), rb2d.position.y), Vector2.down);

            // DEBUG:
            Debug.DrawRay(new Vector2(rb2d.position.x + (MoveDir() * pOriginX * i), rb2d.position.y), Vector2.down, Color.cyan, 0.05f);
        }

        // Cast rays and return them in an array.
        return data;
    }

    // Take the data collected and set locking information.
    private void SetDirectionLocks(RaycastHit2D[] data, RaycastHit2D[] lookahead)
    {
        // LookForDanger(lookahead);  // Needs testing.
        LookForFall(data, lookahead);
        DirectionLock();     
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
                predeath = data[i - 1];
                postdeath = data[i];
                return;
            }
        }
    }

    // Lookahead for whether you'll fall or not.
    private void LookForFall(RaycastHit2D[] data, RaycastHit2D[] lookahead)
    {
        // Analyze data given and convert it to useful forms.
        float[] stoppingDeltas = AnalyzeHeightData(data);
        int pointsToPlayerWidth = AnalyzeFallLookahead(lookahead);

        Debug.Log(data[0].point.x - data[1].point.x);

        // Check for potential falls.
        for (int i = 0; i < stoppingDeltas.Length; i++)
            if (stoppingDeltas[i] > EXTREME_CHANGE)
            {
                wouldFall = true;
                prefall = data[i];
                postfall = data[i + 1];
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
            for (int i = 0; i < pointsToPlayerWidth || lookaheadFall + i < lookahead.Length; i++)
                // If the data says there are any points we experience where an extreme change isn't registered, we're not in danger of falling.
                if (prefall.point.y - lookahead[lookaheadFall + i].point.y < EXTREME_CHANGE)
                {
                    wouldFall = false;
                    break;
                }
        }
    }

    // Return an array of changes in heights between the collision points.
    private float[] AnalyzeHeightData(RaycastHit2D[] data)
    {
        float[] deltaHeight = new float[data.Length - 1];
        for (int i = 0; i < data.Length - 1; i++)
            deltaHeight[i] = data[i].point.y - data[i + 1].point.y;
        return deltaHeight;
    }

    // Return the length just shy of the player's length in prediction slices.
    private int AnalyzeFallLookahead(RaycastHit2D[] lookahead)
    {
        // Get the x value between any two points.
        float deltaX = lookahead[0].point.x - lookahead[1].point.x;

        // Return the width of the player divided by delta x rounded down.
        return (int)Mathf.Abs(Mathf.Floor(width / deltaX));
    }

    // Determine which directions should be locked.
    private void DirectionLock()
    {
        if (wouldFall || wouldDie)
        {
            // If the fall point's x magnitude outstrips the danger point's magnitude...
            RaycastHit2D closest = Mathf.Abs(rb2d.position.x - postfall.point.x) < Mathf.Abs(rb2d.position.x - postdeath.point.x) ? postfall : postdeath;

            // Determine which direction to lock movement.
            if (rb2d.position.x - closest.point.x > 0)
            {
                leftLocked = true;
                rightLocked = false;
            }
            else
            {
                leftLocked = false;
                rightLocked = true;
            }
        }
        else if (rb2d.velocity.x > 0.1f && leftLocked)
        {
            // Debug.Log("Left Unlocked X: " + rb2d.velocity.x);
            leftLocked = false;
        }
        else if (rb2d.velocity.x < -0.1f && rightLocked)
        {
            // Debug.Log("Right Unlocked X: " + rb2d.velocity.x);
            rightLocked = false;
        }
        else if (rb2d.velocity.y > 2)
        {
            leftLocked = false;
            rightLocked = false;
        }
    }

    // Calculate the stopping distance for our prediction. [KE = (1/2)mv^2]
    private float StoppingDistance() { return 0.5f * rb2d.mass * Mathf.Pow(rb2d.velocity.x, 2); }

    // Helper function to determine the direction of the player.
    // 0:  Not Moving
    // 1:  Moving Right
    // -1: Moving Left
    private int MoveDir() { return rb2d.velocity.x < 0 ? -1 : 1; }
}
