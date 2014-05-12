/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimpleScaleGesture))]
    public class SimpleScaleGestureEditor : TwoPointTransform2DGestureBaseEditor
    {
        public const string TEXT_SCALINGTHRESHOLD = "Minimum distance in cm touch points must move for the gesture to begin.";

        private SerializedProperty scalingThreshold;
        private SerializedProperty minScale;
        private SerializedProperty maxScale;

        protected override void OnEnable()
        {
            base.OnEnable();

            scalingThreshold = serializedObject.FindProperty("scalingThreshold");
            minScale = serializedObject.FindProperty("minScale");
            maxScale = serializedObject.FindProperty("maxScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(scalingThreshold, new GUIContent("Scaling Threshold (cm)", TEXT_SCALINGTHRESHOLD));
            EditorGUILayout.PropertyField(minScale, new GUIContent("Min Scale"));
            EditorGUILayout.PropertyField(maxScale, new GUIContent("Max Scale"));

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}