using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovableStructs
{
    public struct PredictedEvent
    {
        // The RaycastHit2D representing a place before just the predicted event.
        private RaycastHit2D _preEvent;
        public RaycastHit2D preEvent { get { return _preEvent; } }

        // The RaycastHit2D representing a place before just the predicted event.
        private RaycastHit2D _postEvent;
        public RaycastHit2D postEvent { get { return _postEvent; } }

        public PredictedEvent(RaycastHit2D pre, RaycastHit2D post)
        {
            _preEvent = pre;
            _postEvent = post;
        }
    }
}
