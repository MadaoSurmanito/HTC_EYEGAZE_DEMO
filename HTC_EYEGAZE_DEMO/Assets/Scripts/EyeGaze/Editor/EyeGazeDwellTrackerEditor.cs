#if UNITY_EDITOR
using UnityEditor;
using EyeGaze.Runtime.Modules;

namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeDwellTracker))]
    public class EyeGazeDwellTrackerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty logTargetChangesProp = serializedObject.FindProperty("logTargetChanges");
            SerializedProperty logPeriodicSummaryProp = serializedObject.FindProperty("logPeriodicSummary");
            SerializedProperty summaryLogEveryNFramesProp = serializedObject.FindProperty("summaryLogEveryNFrames");

            SerializedProperty exportOnApplicationQuitProp = serializedObject.FindProperty("exportOnApplicationQuit");
            SerializedProperty useCustomOutputDirectoryProp = serializedObject.FindProperty("useCustomOutputDirectory");
            SerializedProperty customOutputDirectoryProp = serializedObject.FindProperty("customOutputDirectory");
            SerializedProperty outputFileNameProp = serializedObject.FindProperty("outputFileName");
            SerializedProperty generateTimestampedFileNameProp = serializedObject.FindProperty("generateTimestampedFileName");
            SerializedProperty includeInstanceIdProp = serializedObject.FindProperty("includeInstanceId");

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(logTargetChangesProp);
            EditorGUILayout.PropertyField(logPeriodicSummaryProp);
            if (logPeriodicSummaryProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(summaryLogEveryNFramesProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(exportOnApplicationQuitProp);

            EditorGUILayout.PropertyField(useCustomOutputDirectoryProp);
            if (useCustomOutputDirectoryProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customOutputDirectoryProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(outputFileNameProp);
            EditorGUILayout.PropertyField(generateTimestampedFileNameProp);
            EditorGUILayout.PropertyField(includeInstanceIdProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif