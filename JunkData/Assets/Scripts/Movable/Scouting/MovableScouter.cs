using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovableStructs;


public class MovableScouter
{
    private Movable unit;                                       // The Unit that is looking ahead.
    private int direction;

        /* Constants */

    private const int PREDICTION_SLICES = 10;                   // How many slices to cut the lookahead data into.
    private const float KEENING_VALUE = 0.001f;
    private const int MAX_KEEN = 20;


        /* Constructors */


    private MovableScouter() { }

    public MovableScouter(Movable theUnit)
    {
        // Initialize locking data.
        unit = theUnit;
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


        /* Data Analysis and Manipulation */
    /* Use the data to determine various information. */


    // Check if the next step is ascending or decending based on a focus of collision.
    public int GetEscalation()
    {
        RaycastHit2D[] close = CastRays(KEENING_VALUE * 10, FurthestContactFromX().point.x, unit.GetMoveInput());
        return CompareHeightDifference(close[0].point.y, close[1].point.y);
    }


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

    private float DistanceFromX(CircumferenceHit ch) { return Mathf.Abs(unit.GetRB2D().position.x - ch.hit.point.x); }
}
