#if UNITY_EDITOR
using UnityEditor;
using EyeGaze.Runtime.Modules;

namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeBasicMetrics))]
    public class EyeGazeBasicMetricsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty fixationThresholdProp = serializedObject.FindProperty("fixationThreshold");

            SerializedProperty emitRepeatedVisualFixationsProp = serializedObject.FindProperty("emitRepeatedVisualFixations");
            SerializedProperty repeatedVisualFixationIntervalProp = serializedObject.FindProperty("repeatedVisualFixationInterval");

            SerializedProperty metricsMaskProp = serializedObject.FindProperty("metricsMask");

            SerializedProperty logFixationStartsProp = serializedObject.FindProperty("logFixationStarts");
            SerializedProperty logPeriodicSummaryProp = serializedObject.FindProperty("logPeriodicSummary");
            SerializedProperty summaryLogEveryNFramesProp = serializedObject.FindProperty("summaryLogEveryNFrames");

            SerializedProperty exportOnApplicationQuitProp = serializedObject.FindProperty("exportOnApplicationQuit");
            SerializedProperty useCustomOutputDirectoryProp = serializedObject.FindProperty("useCustomOutputDirectory");
            SerializedProperty customOutputDirectoryProp = serializedObject.FindProperty("customOutputDirectory");
            SerializedProperty outputFileNameProp = serializedObject.FindProperty("outputFileName");
            SerializedProperty generateTimestampedFileNameProp = serializedObject.FindProperty("generateTimestampedFileName");
            SerializedProperty includeInstanceIdProp = serializedObject.FindProperty("includeInstanceId");

            EditorGUILayout.LabelField("Fixation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fixationThresholdProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual Fixation Emission", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(emitRepeatedVisualFixationsProp);
            if (emitRepeatedVisualFixationsProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(repeatedVisualFixationIntervalProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Metrics Layer Filter", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(metricsMaskProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(logFixationStartsProp);
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