using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test for TriangleTerrain being able to be instantiated.
/// </summary>
public class TriangleTest : MonoBehaviour
{
    public TriangleTerrain TerrainGameObject;
    private TriangleTerrain TestTerrain;

    // Vectors to represent each quadrant.
    private Vector2 Q1 = new Vector2(0.5f, 0.5f);
    private Vector2 Q1_b = new Vector2(1.0f, 0.5f);
    private Vector2 Q2 = new Vector2(-0.5f, 0.5f);
    private Vector2 Q2_b = new Vector2(-1.0f, 0.5f);
    private Vector2 Q3 = new Vector2(-0.5f, -0.5f);
    private Vector2 Q3_b = new Vector2(-1.0f, -1.0f);
    private Vector2 Q4 = new Vector2(0.5f, -0.5f);
    private Vector2 Q4_b = new Vector2(1.0f, -0.5f);


    void Start()
    {
        // Code snippet to test making a simple piece of terrain.
        var prefab = TerrainGameObject;
        TestTerrain = Instantiate(prefab);

        // Testing Quadrant 1...
        //TriangleTestQ1Q1();
        //TriangleTestQ1Q2();
        //TriangleTestQ1Q3();
        //TriangleTestQ1Q4();

        // Testing Quadrant 2...
        //TriangleTestQ2Q1();
        //TriangleTestQ2Q2();
        //TriangleTestQ2Q3();
        //TriangleTestQ2Q4();

        // Testing Quadrant 3...
        //TriangleTestQ3Q1();
        //TriangleTestQ3Q2();
        //TriangleTestQ3Q3();
        //TriangleTestQ3Q4();

        // Testing Quadrant 4...
        //TriangleTestQ4Q1();
        //TriangleTestQ4Q2();
        //TriangleTestQ4Q3();
        //TriangleTestQ4Q4();

        // Manual Testing where I can get creative...
        TestTerrain.ConsolidateShape(new TerrainSide(new Vector2(1.0f, 0.5f), new Vector2(0f, 1.5f))); // Produces rhombus
    }

    void TriangleTestQ1Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q1_b)); }
    void TriangleTestQ1Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q2)); }
    void TriangleTestQ1Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q3)); }
    void TriangleTestQ1Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q4)); }

    void TriangleTestQ2Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q1)); }
    void TriangleTestQ2Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q2_b)); }
    void TriangleTestQ2Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q3)); }
    void TriangleTestQ2Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q4)); }

    void TriangleTestQ3Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q1)); }
    void TriangleTestQ3Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q2)); }
    void TriangleTestQ3Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q3_b)); }
    void TriangleTestQ3Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q4)); }

    void TriangleTestQ4Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q1)); }
    void TriangleTestQ4Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q2)); }
    void TriangleTestQ4Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q3)); }
    void TriangleTestQ4Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q4_b)); }
}
