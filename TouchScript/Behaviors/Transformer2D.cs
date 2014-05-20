/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Events;
using TouchScript.Gestures;
using TouchScript.Gestures.Simple;
using UnityEngine;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Simple Component which transforms an object according to events from gestures.
    /// </summary>
    [AddComponentMenu("TouchScript/Behaviors/Transformer2D")]
    public class Transformer2D : MonoBehaviour
    {
        #region Unity fields

        /// <summary>
        /// Max movement speed
        /// </summary>
        public float Speed = 10f;

        #endregion

        #region Private variables

        private Vector3 localPositionToGo, localScaleToGo;
        private Quaternion localRotationToGo;

        private Vector3 lastLocalPosition, lastLocalScale;
        private Quaternion lastLocalRotation;

        #endregion

        #region Unity

        private void Start()
        {
            setDefaults();

            if (GetComponent<SimplePanGesture>() != null)
            {
                GetComponent<SimplePanGesture>().StateChanged += onPanStateChanged;
            }
            if (GetComponent<SimpleScaleGesture>() != null)
            {
                GetComponent<SimpleScaleGesture>().StateChanged += onScaleStateChanged;
            }
            if (GetComponent<SimpleRotateGesture>() != null)
            {
                GetComponent<SimpleRotateGesture>().StateChanged += onRotateStateChanged;
            }
        }

        private void Update()
        {
            var fraction = Speed*Time.deltaTime;
            if (transform.localPosition != lastLocalPosition)
            {
                // changed by someone else
                localPositionToGo = transform.localPosition;
            }

            transform.localPosition = lastLocalPosition = Vector3.Lerp(transform.localPosition, localPositionToGo, fraction);
            //if (transform.parent.localScale != lastLocalScale)
            //{
            //    localScaleToGo = transform.parent.localScale;

            //}
            //transform.localScale = lastLocalScale = Vector3.Lerp(transform.localScale, localScaleToGo, fraction);
           if(parentObject == null || localScaleToGo !=parentObject.transform.localScale )scaleFromPosition(localScaleToGo, middlePoint);

            //if (transform.localRotation != lastLocalRotation)
            //{
            //    localRotationToGo = transform.localRotation;
            //}
            //transform.localRotation = lastLocalRotation = Quaternion.Lerp(transform.localRotation, localRotationToGo, fraction);
           if (parentObject == null || localRotationToGo != parentObject.transform.localRotation) rotateFromPosition(localRotationToGo, middlePoint);

         //   transform.localScale = localScaleToGo;
           
         //   // Translate manipulation
         //   var originalCenter = transform.localPosition;
         //   var newCenter = Pivot;
         //   transform.localPosition = newCenter - originalCenter;

         //   Debug.Log(Pivot);
         //   // Rotation manipulation
         ////   transform.localRotation = localRotationToGo;// Quaternion.Angle(localRotationToGo, transform.localRotation);
        }

        #endregion

        #region Private functions
        static Vector3 prevPos ;
        private GameObject parentObject;
        private void scaleFromPosition(Vector3 scale, Vector3 fromPos)
        {
            if (parentObject == null)
            {
                parentObject = new GameObject();
                parentObject.transform.parent = transform.parent;
                this.transform.parent = parentObject.transform;
            }
            if (!fromPos.Equals(prevPos))
            {
                Debug.Log("notequal"+ " "+ fromPos +" " +prevPos);
                Vector3 prevParentPos = parentObject.transform.position;
                parentObject.transform.position = fromPos;
                Vector3 diff = parentObject.transform.position - prevParentPos;
                Vector3 pos = new Vector3(diff.x / parentObject.transform.localScale.x * -1, diff.y / parentObject.transform.localScale.y * -1, transform.position.z);
                transform.localPosition = new Vector3(transform.localPosition.x + pos.x, transform.localPosition.y + pos.y, pos.z);
            }
            parentObject.transform.localScale = scale;
            prevPos = fromPos;
        }
        private void rotateFromPosition(Quaternion rotate, Vector3 fromPos)
        {
            if (parentObject == null)
            {
                parentObject = new GameObject();
                parentObject.transform.parent = transform.parent;
                this.transform.parent = parentObject.transform;
            }
            if (!fromPos.Equals(prevPos))
            {
                Debug.Log("notequal" + " " + fromPos + " " + prevPos);
                Vector3 prevParentPos = parentObject.transform.position;
                parentObject.transform.position = fromPos;
                Vector3 diff = parentObject.transform.position - prevParentPos;
                Vector3 pos = new Vector3(diff.x / parentObject.transform.localScale.x * -1, diff.y / parentObject.transform.localScale.y * -1, transform.position.z);
                transform.localPosition = new Vector3(transform.localPosition.x + pos.x, transform.localPosition.y + pos.y, pos.z);
            }
            Debug.Log(fromPos + " > " + prevPos);
            parentObject.transform.localRotation = rotate;
            prevPos = fromPos;
        }
        private void setDefaults()
        {
            localPositionToGo = lastLocalPosition = transform.localPosition;
            localRotationToGo = lastLocalRotation = transform.localRotation;
            localScaleToGo = lastLocalScale = transform.localScale;
        }

        #endregion

        #region Event handlers
        public Vector3 Pivot= Vector3.zero;
        private void onPanStateChanged(object sender, GestureStateChangeEventArgs e)
        {
            switch (e.State)
            {
                case Gesture.GestureState.Began:
                case Gesture.GestureState.Changed:
                    var gesture = (SimplePanGesture)sender;
                    Pivot = gesture.Pivot;
                    if (gesture.LocalDeltaPosition != Vector3.zero)
                    {
                        localPositionToGo += gesture.LocalDeltaPosition;
                    }
                    break;
            }
        }

        private void onRotateStateChanged(object sender, GestureStateChangeEventArgs e)
        {
            switch (e.State)
            {
                case Gesture.GestureState.Began:
                case Gesture.GestureState.Changed:
                    var gesture = (SimpleRotateGesture)sender;

                    if (Math.Abs(gesture.LocalDeltaRotation) > 0.01)
                    {
                        if (transform.parent == null)
                        {
                            localRotationToGo = Quaternion.AngleAxis(gesture.LocalDeltaRotation, gesture.WorldTransformPlane.normal)*localRotationToGo;
                        } else
                        {
                            localRotationToGo = Quaternion.AngleAxis(gesture.LocalDeltaRotation, transform.parent.InverseTransformDirection(gesture.WorldTransformPlane.normal))*localRotationToGo;
                        }
                    }
                    middlePoint = gesture.PivotPoint;
                    break;
            }
        }
        private Vector3 middlePoint;
        private void onScaleStateChanged(object sender, GestureStateChangeEventArgs e)
        {
            switch (e.State)
            {
                case Gesture.GestureState.Began:
                case Gesture.GestureState.Changed:
                    var gesture = (SimpleScaleGesture)sender;

                    if (Math.Abs(gesture.LocalDeltaScale - 1) > 0.00001)
                    {

                        var nextScale = localScaleToGo * gesture.LocalDeltaScale;
                        var min = gesture.MinScale;
                        var max = gesture.MaxScale;

                        if(min.x < nextScale.x && min.y < nextScale.y && max.x > nextScale.x && max.y > nextScale.y )
                            localScaleToGo *= gesture.LocalDeltaScale;
                        
                    }
                    middlePoint = gesture.PivotPoint;
                    // localPositionToGo = gesture.TranslationDelta;
                    Debug.Log(">>" + middlePoint);
                    break;
            }
        }

        #endregion
    }
}


