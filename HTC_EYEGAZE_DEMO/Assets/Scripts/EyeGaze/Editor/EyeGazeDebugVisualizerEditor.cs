#if UNITY_EDITOR
using UnityEditor;
using EyeGaze.Runtime.Modules;

namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeDebugVisualizer))]
    public class EyeGazeDebugVisualizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty enableDebugRayProp = serializedObject.FindProperty("enableDebugRay");
            SerializedProperty debugLineRendererProp = serializedObject.FindProperty("debugLineRenderer");
            SerializedProperty debugRayColorProp = serializedObject.FindProperty("debugRayColor");
            SerializedProperty debugCameraLineRendererProp = serializedObject.FindProperty("debugCameraLineRenderer");
            SerializedProperty debugCameraRayColorProp = serializedObject.FindProperty("debugCameraRayColor");
            SerializedProperty debugOffsetLineRendererProp = serializedObject.FindProperty("debugOffsetLineRenderer");
            SerializedProperty debugOffsetLineColorProp = serializedObject.FindProperty("debugOffsetLineColor");
            SerializedProperty enableDebugLogsProp = serializedObject.FindProperty("enableDebugLogs");
            SerializedProperty debugLogEveryNFramesProp = serializedObject.FindProperty("debugLogEveryNFrames");

            EditorGUILayout.PropertyField(enableDebugRayProp);
            if (enableDebugRayProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(debugLineRendererProp);
                EditorGUILayout.PropertyField(debugRayColorProp);
                EditorGUILayout.PropertyField(debugCameraLineRendererProp);
                EditorGUILayout.PropertyField(debugCameraRayColorProp);
                EditorGUILayout.PropertyField(debugOffsetLineRendererProp);
                EditorGUILayout.PropertyField(debugOffsetLineColorProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(enableDebugLogsProp);
            if (enableDebugLogsProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(debugLogEveryNFramesProp);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif