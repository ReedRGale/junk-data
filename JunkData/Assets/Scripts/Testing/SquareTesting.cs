using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test for SquareTerrain being able to be instantiated.
/// </summary>
public class SquareTesting : MonoBehaviour
{
    public SquareTerrain TerrainGameObject;
    private SquareTerrain TestTerrain;

    // Vectors to represent each quadrant.
    private Vector2 Q1 = new Vector2(0.5f, 0.5f);
    private Vector2 Q1_b = new Vector2(1.0f, 0.5f);
    private Vector2 Q2 = new Vector2(-0.5f, 0.5f);
    private Vector2 Q2_b = new Vector2(-1.0f, 0.5f);
    private Vector2 Q3 = new Vector2(-0.5f, -0.5f);
    private Vector2 Q3_b = new Vector2(-1.0f, -1.0f);
    private Vector2 Q4 = new Vector2(0.5f, -0.5f);
    private Vector2 Q4_b = new Vector2(1.0f, -0.5f);


    void Start ()
    {
        // Testing SIN and COS functions...

        // Testing SIN at critical points...
        //Debug.Log("0 Degrees: " + Mathf.Sin(0 * Mathf.Deg2Rad));
        //Debug.Log("0 Degrees, Expected: 0");
        //Debug.Log("45 Degrees: " + Mathf.Sin(45 * Mathf.Deg2Rad));
        //Debug.Log("0 Degrees, Expected: 0.707");
        //Debug.Log("90 Degrees: " + Mathf.Sin(90 * Mathf.Deg2Rad));
        //Debug.Log("0 Degrees, Expected: 1");
        //Debug.Log("135 Degrees: " + Mathf.Sin(135 * Mathf.Deg2Rad));
        //Debug.Log("135 Degrees, Expected: 0.707");
        //Debug.Log("180 Degrees: " + Mathf.Sin(180 * Mathf.Deg2Rad));
        //Debug.Log("180 Degrees, Expected: 0");
        //Debug.Log("225 Degrees: " + Mathf.Sin(225 * Mathf.Deg2Rad));
        //Debug.Log("225 Degrees, Expected: -0.707");
        //Debug.Log("270 Degrees: " + Mathf.Sin(270 * Mathf.Deg2Rad));
        //Debug.Log("270 Degrees, Expected: -1");
        //Debug.Log("315 Degrees: " + Mathf.Sin(315 * Mathf.Deg2Rad));
        //Debug.Log("315 Degrees, Expected: -0.707");
        //Debug.Log("360 Degrees: " + Mathf.Sin(360 * Mathf.Deg2Rad));
        //Debug.Log("360 Degrees, Expected: 0");

        // Code snippet to test making a simple piece of terrain.
        var prefab = TerrainGameObject;
        TestTerrain = Instantiate(prefab);

        // Testing Quadrant 1...
        //SquareTestQ1Q1();
        //SquareTestQ1Q2();
        //SquareTestQ1Q3();
        //SquareTestQ1Q4();

        // Testing Quadrant 2...
        //SquareTestQ2Q1();
        //SquareTestQ2Q2();
        //SquareTestQ2Q3();
        //SquareTestQ2Q4();

        // Testing Quadrant 3...
        //SquareTestQ3Q1();
        //SquareTestQ3Q2();
        //SquareTestQ3Q3();
        //SquareTestQ3Q4();

        // Testing Quadrant 4...
        //SquareTestQ4Q1();
        //SquareTestQ4Q2();
        //SquareTestQ4Q3();
        //SquareTestQ4Q4();

        TestTerrain.ConsolidateShape(new TerrainSide(new Vector2(1.0f , 0.5f), new Vector2(0f, 1.5f))); // Produces rhombus
    }

    void SquareTestQ1Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q1_b)); }
    void SquareTestQ1Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q2)); }
    void SquareTestQ1Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q3)); }
    void SquareTestQ1Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q4)); }

    void SquareTestQ2Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q1)); }
    void SquareTestQ2Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q2_b)); }
    void SquareTestQ2Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q3)); }
    void SquareTestQ2Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q4)); }

    void SquareTestQ3Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q1)); }
    void SquareTestQ3Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q2)); }
    void SquareTestQ3Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q3_b)); }
    void SquareTestQ3Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q4)); }

    void SquareTestQ4Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q1)); }
    void SquareTestQ4Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q2)); }
    void SquareTestQ4Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q3)); }
    void SquareTestQ4Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q4_b)); }
}
