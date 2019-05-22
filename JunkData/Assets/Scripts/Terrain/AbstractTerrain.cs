using System.Linq;
using UnityEngine;
using Extensions;
using System.Collections.Generic;

/// <summary>
/// The abstract form of land that can be walked on and destroyed in the game.
/// </summary>
public abstract class AbstractTerrain : MonoBehaviour
{
    /// <summary>
    /// The type of shape that this object is.
    /// </summary>
    public string ShapeName { get; protected set; }

    /// <summary>
    /// ~~~Unimplemented~~~
    /// Boolean relating to functions that say whether the terrain can be destroyed or not.
    /// </summary>
    public bool IsDestructible { get; set; }

    /// <summary>
    /// ~~~Unimplemented~~~
    /// A variable representing how many points would be gained from destroying the block
    /// </summary>
    public float Density { get; protected set; }

    /// <summary>
    /// A list of the sides of this shape.
    /// </summary>
    public List<TerrainSide> ShapeSides = new List<TerrainSide>();

    /// <summary>
    /// ~~~Unimplemented~~~
    /// Object that controls an image that is displayed on the block.
    /// </summary>
    //private TerrainSkin _skin;

    /// <summary>
    /// ~~~Unimplemented~~~
    /// Set of special attributes that define the block's behavior--besides being stuff to walk on and destroy.
    /// </summary>
    //private List<TerrainAttribute> _attributes;

    /// <summary>
    /// The mesh filter that renders this Game Object's mesh.
    /// </summary>
    private MeshFilter _meshFilter;


    // Unity Callbacks


    /// <summary>
    /// Creates a default terrain object without a skin or attributes.
    /// </summary>
    public virtual void Awake()
    {
        ShapeName = "AbstractShape";
        IsDestructible = true;
        Density = 1;

        _meshFilter = GetComponent<MeshFilter>();
    }

    // Public Methods


    /// <summary>
    /// ~~~Unimplemented~~~
    /// Replace the skin with a broken block gif--probably tv static or white noise or something.
    /// </summary>
    public void RuinSkin()
    {

    }

    /// <summary>
    /// ~~~Unimplemented~~~ 
    /// Adds an attribute to the terrain.
    /// </summary>
    /// <param name="theAttribute">The attribute to add to the terrain's functionality.</param>
    //public void AddAttribute(TerrainAttribute theAttribute) { }

    /// <summary>
    /// ~~~Unimplemented~~~
    /// An attribute of the type to be removed from the terrain.
    /// </summary>
    /// <param name="theAttribute">The attribute of which type we are to remove from the terrain's functionality</param>
    //public void RemoveAttribute(TerrainAttribute theAttribute) { }

    /// <summary>
    /// Sets the shape up in gamespace when called. Much of this code is sourced from:
    /// https://medium.com/@hyperparticle/draw-2d-physics-shapes-in-unity3d-2e0ec634381c
    /// </summary>
    /// <param name="theSide">The side of terrain we're working from.</param>
    public void ConsolidateShape(TerrainSide theSide)
    {
        Vector2[] vertices = ExtrapolateShape(theSide);

        // Set the gameobject's position to be the center of mass
        var center = vertices.Centroid();
        transform.position = center;

        // Update the mesh relative to the transform
        var relativeVertices = vertices.Select(v => v - center).ToArray();
        _meshFilter.mesh = TerrainMesh(relativeVertices);
    }

    /// <summary>
    /// ~~~Unimplemented~~~ 
    /// Takes a side in the form of two Vector2s and turns it into the proper shape based on the child object's type.
    /// The order the points are in determines the 'direction' that shape is formulated.
    /// </summary>
    /// <param name="theSide">The side of terrain we're working from.</param>
    /// <returns>A set of vectors making up the completed shape's outline.</returns>
    protected abstract Vector2[] ExtrapolateShape(TerrainSide theSide);

    /// <summary>
    /// ~~~Unimplemented~~~ 
    /// Takes an array of Vector2s that represent a shape's outline and turns it into a Mesh Unity can use.
    /// </summary>
    /// <param name="meshVectors">The points making up the outline of the shape.</param>
    /// <returns>A mesh representing the shape in a form that can be attached to a Game Object and interpreted by a Line Renderer.</returns>
    protected Mesh TerrainMesh(Vector2[] meshVectors)
    {
        int[] triangles = new Triangulator(meshVectors).Triangulate();

        // Assign each vertex a randomly chosen fill color.
        Color[] colors = Enumerable.Repeat(new Color(Random.Range(0.25f, 0.75f),  // Red
                                                     Random.Range(0.25f, 0.75f),  // Green
                                                     Random.Range(0.25f, 0.75f)), // Blue
                                           meshVectors.Length).ToArray();
        
        var mesh = new Mesh
        {
            name = ShapeName,
            vertices = meshVectors.ToVector3(),
            triangles = triangles,
            colors = colors
        };

        mesh = UpdateMesh(mesh);

        return mesh;
    }

    /// <summary>
    /// Various housekeeping functions to make sure the Mesh's components are up to date.
    /// </summary>
    /// <param name="theMesh">The mesh to check up on.</param>
    protected Mesh UpdateMesh(Mesh theMesh)
    {
        theMesh.RecalculateNormals();
        theMesh.RecalculateBounds();
        theMesh.RecalculateTangents();

        return theMesh;
    }
}

