using UnityEngine;
using Extensions;

/// <summary>
/// A helper class that contains two Vector2s that make up the side of a Terrain shape.
/// </summary>
public class TerrainSide
{
    /// <summary>
    /// The first point making up one side of the terrain shape.
    /// </summary>
    public Vector2 CornerA { get; private set; }

    /// <summary>
    /// The second point making up one side of the terrain shape.
    /// </summary>
    public Vector2 CornerB { get; private set; }

    /// <summary>
    /// The distance between the two points for easy reference.
    /// </summary>
    public float Distance { get; private set; }

    /// <summary>
    /// The angle between the side and the X Axis.
    /// </summary>
    public float Angle { get; private set; }

    /// <summary>
    /// Constructor for a side.
    /// </summary>
    /// <param name="a">The first point making up one side of the terrain shape</param>
    /// <param name="b">The second point making up one side of the terrain shape.</param>
    public TerrainSide(Vector2 a, Vector2 b)
    {
        CornerA = a;
        CornerB = b;

        // Precalculated to save on time calculating over and over again.
        Distance = Vector2.Distance(a, b);
        Angle = Vector2.Angle(CornerA.VectorBetween(CornerB), Vector2.right);

        // Make sure this is the proper angle between...
        if (CornerA.VectorBetween(CornerB).y < 0)
            Angle = 360 - Angle;
    }

    public override string ToString()
    {
        return "The side from " + CornerA + " to " + CornerB + ".";
    }
}
