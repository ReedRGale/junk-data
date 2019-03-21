using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovableStructs
{
    public struct CircumferenceHit
    {
        // The RaycastHit2D actually representing the coordinates of the hit.
        private RaycastHit2D _hit;
        public RaycastHit2D hit { get { return _hit; } }

        // The angle of the hit relative to the initial direction.
        private float _angle;
        public float angle { get { return _angle; } }

        // The initial direction vector from the center we're using to measure the hit.
        private Vector2 _initialDirection;
        public Vector2 initialDirection { get { return _initialDirection; } }

        public CircumferenceHit(RaycastHit2D theHit, float theAngle)
        {
            _hit = theHit;
            _initialDirection = Vector2.right;
            _angle = theAngle;
        }

        public CircumferenceHit(RaycastHit2D theHit, float theAngle, Vector2 theInitialDirection)
        {
            _hit = theHit;
            _initialDirection = theInitialDirection;
            _angle = theAngle;
        }
    }
}
