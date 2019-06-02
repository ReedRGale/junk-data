using UnityEngine;

/// <summary>
/// Creates a piece of regular hexagonal destructible terrain.
/// </summary>
public class HexTerrain : AbstractTerrain
{
    /// <summary>
    /// Creates a piece of Hex Terrain with no skin or attributes.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        ShapeName = "TriangleTerrain";
    }

    /// <summary>
    /// Uses a given side to create a hexagon's outline.
    /// </summary>
    /// <param name="theSide">The side object containing the first two points in the hexagon.</param>
    /// <returns></returns>
    protected override Vector2[] ExtrapolateShape(TerrainSide theSide)
    {
        // The angle we need to translate by to get another point in an equilateral triangle
        float theta = theSide.Angle + 60;

        // Create intermediary point to help make the other point translations simpler.
        Vector2 sidePointI = new Vector2(X(theSide.CornerA, theta, theSide.Distance),
                                         Y(theSide.CornerA, theta, theSide.Distance));

        // Create the other 4 corner points.
        Vector2 sidePointC = GenerateHexagonalTranslation(sidePointI, theSide.CornerB);
        Vector2 sidePointD = GenerateHexagonalTranslation(sidePointI, sidePointC);
        Vector2 sidePointE = GenerateHexagonalTranslation(sidePointI, sidePointD);
        Vector2 sidePointF = GenerateHexagonalTranslation(sidePointI, sidePointE);

        return new Vector2[] { theSide.CornerA, theSide.CornerB, sidePointC, sidePointD, sidePointE, sidePointF };
    }

    /// <summary>
    /// Helper function to simplify the rotation of a side to generate the other parts of the hexagon.
    /// </summary>
    /// <param name="center">The centerpoint of the hexagon.</param>
    /// <param name="edge">An edgepoint of the hexagon.</param>
    /// <returns></returns>
    private Vector2 GenerateHexagonalTranslation(Vector2 center, Vector2 edge)
    {
        TerrainSide internalSide = new TerrainSide(center, edge);
        float theta = internalSide.Angle + 60;
        Vector2 newEdge = new Vector2(X(internalSide.CornerA, theta, internalSide.Distance),
                                      Y(internalSide.CornerA, theta, internalSide.Distance));
        return newEdge;
    }
}
