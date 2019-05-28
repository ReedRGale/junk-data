using UnityEngine;

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
    /// Uses a given side to create a square's outline.
    /// </summary>
    /// <param name="theSide">The side object conaining the first two points in the square.</param>
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
}
