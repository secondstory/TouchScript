﻿/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Simple Scale gesture which takes into account only the first two touch points.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Simple Scale Gesture")]
    public class SimpleScaleGesture : TwoPointTransform2DGestureBase
    {
        #region Private variables

        [SerializeField]
        private float scalingThreshold = .5f;

        private float scalingBuffer;
        private bool isScaling = false;
        
        [SerializeField]
        private Vector2 minScale = Vector2.one;
       
        [SerializeField]
        private Vector2 maxScale = Vector2.one * 10;
        #endregion

        #region Public properties

        /// <summary>
        /// Minimum distance in cm between touch points for gesture to begin.
        /// </summary>
        public float ScalingThreshold
        {
            get { return scalingThreshold; }
            set { scalingThreshold = value; }
        }

        public Vector3 MinScale { get { return minScale; } set { minScale = value; } }
        public Vector3 MaxScale { get { return maxScale; } set { maxScale = value; } }
        public Vector3 PivotPoint { get; set; }
        /// <summary>
        /// Contains local delta scale when gesture is recognized.
        /// Value is between 0 and +infinity, where 1 is no scale, 0.5 is scaled in half, 2 scaled twice.
        /// </summary>
        public float LocalDeltaScale { get; private set; }
        

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            if (!gotEnoughTouchPoints()) return;
            if (!relevantTouchPoints(touches)) return;

            Vector3 oldGlobalCenter3DPos, oldLocalCenter3DPos, newGlobalCenter3DPos, newLocalCenter3DPos;
            var deltaScale = 1f;

            var new2DPos1 = getPointScreenPosition(0);
            var new2DPos2 = getPointScreenPosition(1);
            if (Vector2.Distance(new2DPos1, new2DPos2) < minPointsDistanceInPixels) return;

            base.touchesMoved(touches);

            var old2DPos1 = getPointPreviousScreenPosition(0);
            var old2DPos2 = getPointPreviousScreenPosition(1);
            var old3DPos1 = ProjectionUtils.CameraToPlaneProjection(old2DPos1, projectionCamera, WorldTransformPlane);
            var old3DPos2 = ProjectionUtils.CameraToPlaneProjection(old2DPos2, projectionCamera, WorldTransformPlane);
            var new3DPos1 = ProjectionUtils.CameraToPlaneProjection(new2DPos1, projectionCamera, WorldTransformPlane);
            var new3DPos2 = ProjectionUtils.CameraToPlaneProjection(new2DPos2, projectionCamera, WorldTransformPlane);
            var newVector = new3DPos2 - new3DPos1;

            Vector2 oldCenter2DPos = (old2DPos1 + old2DPos2)*.5f;
            Vector2 newCenter2DPos = (new2DPos1 + new2DPos2)*.5f;

            if (isScaling)
            {
                deltaScale = newVector.magnitude/Vector3.Distance(old3DPos2, old3DPos1);
            } else
            {
                var old2DDist = Vector2.Distance(old2DPos1, old2DPos2);
                var new2DDist = Vector2.Distance(new2DPos1, new2DPos2);
                var delta2DDist = new2DDist - old2DDist;
                scalingBuffer += delta2DDist;
                var dpiScalingThreshold = ScalingThreshold*touchManager.DotsPerCentimeter;
                if (scalingBuffer*scalingBuffer >= dpiScalingThreshold*dpiScalingThreshold)
                {
                    isScaling = true;
                    var oldVector2D = (old2DPos2 - old2DPos1).normalized;
                    var startScale = (new2DDist - scalingBuffer)*.5f;
                    var startVector = oldVector2D*startScale;
                    deltaScale = newVector.magnitude/(ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos + startVector, projectionCamera, WorldTransformPlane) - ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos - startVector, projectionCamera, WorldTransformPlane)).magnitude;
                }
            }

            oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos, projectionCamera, WorldTransformPlane);
            newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, WorldTransformPlane);
            oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
            newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);
                
            if (Mathf.Abs(deltaScale - 1f) > 0.00001)
            {
                switch (State)
                {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        screenPosition = newCenter2DPos;
                        previousScreenPosition = oldCenter2DPos;
                        PreviousWorldTransformCenter = oldGlobalCenter3DPos;
                        WorldTransformCenter = newGlobalCenter3DPos;
                        PreviousWorldTransformCenter = oldGlobalCenter3DPos;
                        LocalTransformCenter = newLocalCenter3DPos;
                        PreviousLocalTransformCenter = oldLocalCenter3DPos;

                        LocalDeltaScale = deltaScale;



                        if (State == GestureState.Possible)
                        {
                            setState(GestureState.Began);
                            PivotPoint = WorldTransformCenter;

                        } else
                        {
                            setState(GestureState.Changed);
                        }
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            scalingBuffer = 0f;
            isScaling = false;
        }

        /// <inheritdoc />
        protected override void restart()
        {
            base.restart();

            LocalDeltaScale = 1f;
        }

        #endregion

        /// <summary>
        /// Calculates the scaling factor you should apply to the object being manipulated. A scaling factor of 2 means you
        /// should multiply your object's scale by 2. This assumes that the center of scaling is at the center of the object
        /// when you draw it.
        /// </summary>
        /// <param name="position1">The position of the first finger in the pinch gesture, in screen-space.</param>
        /// <param name="position2">The position of the second finger in the pinch gesture, in screen-space.</param>
        /// <param name="delta1">The delta of the first finger in the pinch gesture.</param>
        /// <param name="delta2">The delta of the second finger in the pinch gesture.</param>
        /// <returns>The scaling factor to apply to your object.</returns>
        public static float GetScaleFactor(Vector2 position1, Vector2 position2, Vector2 delta1, Vector2 delta2)
        {
            Vector2 oldPosition1 = position1 - delta1;
            Vector2 oldPosition2 = position2 - delta2;

            float distance = Vector2.Distance(position1, position2);
            float oldDistance = Vector2.Distance(oldPosition1, oldPosition2);

            if (oldDistance == 0 || distance == 0)
            {
                return 1.0f;
            }

            return distance / oldDistance;
        }

        /// <summary>
        /// Calculates the amount you should translate your object by during a pinch gesture.
        /// </summary>
        /// <param name="position1">The position of the first finger in the pinch gesture, in screen-space.</param>
        /// <param name="position2">The position of the second finger in the pinch gesture, in screen-space.</param>
        /// <param name="delta1">The delta of the first finger in the pinch gesture.</param>
        /// <param name="delta2">The delta of the second finger in the pinch gesture.</param>
        /// <param name="objectPos">The current position of your object, in screen-space.</param>
        /// <param name="scaleFactor">The current scale factor of your object. Call GetScaleFactor to retreive this.</param>
        /// <returns></returns>
        public static Vector2 GetTranslationDelta(Vector2 position1, Vector2 position2, Vector2 oldPosition1, Vector2 oldPosition2,
            Vector2 objectPos, float scaleFactor)
        {
        

            Vector2 newPos1 = position1 + (objectPos - oldPosition1) * scaleFactor;
            Vector2 newPos2 = position2 + (objectPos - oldPosition2) * scaleFactor;
            Vector2 newPos = (newPos1 + newPos2) / 2;

            return newPos - objectPos;
        }
    }
}