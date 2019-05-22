using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    /// <summary>
    /// Extensions related to unity's Vector2s
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        ///  Extension that rotates a Vector2 by a number of degrees.
        /// </summary>
        /// <param name="v">The Vector's identity.</param>
        /// <param name="degrees">The degrees to rotate this vector by.</param>
        /// <returns></returns>
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        /// <summary>
        /// Extension that converts an array of Vector2 to an array of Vector3
        /// Source:  https://medium.com/@hyperparticle/draw-2d-physics-shapes-in-unity3d-2e0ec634381c
        /// </summary>
        /// <param name="vectors">The set of Vector2s to convert to Vector3s</param>
        /// <returns></returns>
        public static Vector3[] ToVector3(this Vector2[] vectors)
        {
            return System.Array.ConvertAll<Vector2, Vector3>(vectors, v => v);
        }

        /// <summary>
        /// Extension that, given a collection of vectors, returns a centroid.
        /// (i.e., an average of all vectors) 
        /// Source:  https://medium.com/@hyperparticle/draw-2d-physics-shapes-in-unity3d-2e0ec634381c
        /// </summary>
        /// <param name="vectors">The set of Vector2s to find the centroid of</param>
        /// <returns></returns>
        public static Vector2 Centroid(this ICollection<Vector2> vectors)
        {
            Vector2 aggregate = new Vector2();
            foreach (Vector2 v in vectors) { aggregate += v; }
            return aggregate / vectors.Count;
        }

        /// <summary>
        /// Extension returning the absolute value of a vector.
        /// Source:  https://medium.com/@hyperparticle/draw-2d-physics-shapes-in-unity3d-2e0ec634381c
        /// </summary>
        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        /// <summary>
        /// Extension returning the vector pointing between the caller and the given vector.
        /// </summary>
        public static Vector2 VectorBetween(this Vector2 vector, Vector2 otherVector)
        {
            return otherVector - vector;
        }
    }
}