using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test for TriangleTerrain being able to be instantiated.
/// </summary>
public class HexTest : MonoBehaviour
{
    public HexTerrain TerrainGameObject;
    private HexTerrain TestTerrain;

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
        //HexTestQ1Q1();
        //HexTestQ1Q2();
        //HexTestQ1Q3();
        //HexTestQ1Q4();

        // Testing Quadrant 2...
        //HexTestQ2Q1();
        HexTestQ2Q2();
        //HexTestQ2Q3();
        //HexTestQ2Q4();

        // Testing Quadrant 3...
        //HexTestQ3Q1();
        //HexTestQ3Q2();
        //HexTestQ3Q3();
        //HexTestQ3Q4();

        // Testing Quadrant 4...
        //HexTestQ4Q1();
        //HexTestQ4Q2();
        //HexTestQ4Q3();
        //HexTestQ4Q4();

        // Manual Testing where I can get creative...
        //TestTerrain.ConsolidateShape(new TerrainSide(new Vector2(1.0f, 0.5f), new Vector2(0f, 1.5f))); // Produces rhombus
    }

    void HexTestQ1Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q1_b)); }
    void HexTestQ1Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q2)); }
    void HexTestQ1Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q3)); }
    void HexTestQ1Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q1, Q4)); }

    void HexTestQ2Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q1)); }
    void HexTestQ2Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q2_b)); }
    void HexTestQ2Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q3)); }
    void HexTestQ2Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q2, Q4)); }

    void HexTestQ3Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q1)); }
    void HexTestQ3Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q2)); }
    void HexTestQ3Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q3_b)); }
    void HexTestQ3Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q3, Q4)); }

    void HexTestQ4Q1() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q1)); }
    void HexTestQ4Q2() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q2)); }
    void HexTestQ4Q3() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q3)); }
    void HexTestQ4Q4() { TestTerrain.ConsolidateShape(new TerrainSide(Q4, Q4_b)); }
}
