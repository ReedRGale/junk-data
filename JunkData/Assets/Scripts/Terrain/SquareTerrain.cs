using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

/// <summary>
/// Creates a piece of square-shaped destructible terrain.
/// </summary>
public class SquareTerrain : AbstractTerrain
{ 
    /// <summary>
    /// Creates a piece of Square Terrain with no skin or attributes.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        ShapeName = "SquareTerrain";
    }

    /// <summary>
    /// Uses the two sides to create a square's outline.
    /// </summary>
    /// <param name="sidePointA">The first point making up the side we're extrapolating the shape from.</param>
    /// <param name="sidePointB">The second point making up the side we're extrapolating the shape from.</param>
    /// <returns></returns>
    protected override Vector2[] ExtrapolateShape(TerrainSide theSide)
    {
        float theta = theSide.Angle - 90;
        Vector2 sidePointC = new Vector2(X(theSide.CornerB, theta, theSide.Distance), 
                                         Y(theSide.CornerB, theta, theSide.Distance));
        Vector2 sidePointD = new Vector2(X(theSide.CornerA, theta, theSide.Distance), 
                                         Y(theSide.CornerA, theta, theSide.Distance));
        return new Vector2[] { theSide.CornerA, theSide.CornerB, sidePointC, sidePointD };
    }

    /// <summary>
    /// The X value translation of a Vector's X to new X 'distance' away at a given 'angle'
    /// </summary>
    /// <param name="point">The point to translate.</param>
    /// <param name="angle">The angle--measured from the positive X axis--that the translated X should be taken relative to</param>
    /// <param name="distance">The distance away the translation should be from the original point</param>
    /// <returns></returns>
    private float X(Vector2 point, float angle, float distance) { return distance * Mathf.Cos(angle * Mathf.Deg2Rad) + point.x; }

    /// <summary>
    /// The Y value translation of a Vector's Y to new Y 'distance' away at a given 'angle'
    /// </summary>
    /// <param name="point">The point to translate.</param>
    /// <param name="angle">The angle--measured from the positive X axis--that the translated Y should be taken relative to</param>
    /// <param name="distance">The distance away the translation should be from the original point</param>
    /// <returns></returns>
    private float Y(Vector2 point, float angle, float distance) { return distance * Mathf.Sin(angle * Mathf.Deg2Rad) + point.y; }
}
