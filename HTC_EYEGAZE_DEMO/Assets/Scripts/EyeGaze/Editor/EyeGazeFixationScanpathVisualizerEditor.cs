#if UNITY_EDITOR
using UnityEditor;
using EyeGaze.Runtime.Modules;

namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeFixationScanpathVisualizer))]
    public class EyeGazeFixationScanpathVisualizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty basicMetricsProp = serializedObject.FindProperty("basicMetrics");

            SerializedProperty fixationNodePrefabProp = serializedObject.FindProperty("fixationNodePrefab");
            SerializedProperty nodesParentProp = serializedObject.FindProperty("nodesParent");

            SerializedProperty surfaceOffsetProp = serializedObject.FindProperty("surfaceOffset");
            SerializedProperty mergeDistanceProp = serializedObject.FindProperty("mergeDistance");

            SerializedProperty baseNodeScaleProp = serializedObject.FindProperty("baseNodeScale");
            SerializedProperty scaleIncreasePerFixationProp = serializedObject.FindProperty("scaleIncreasePerFixation");
            SerializedProperty maxNodeScaleProp = serializedObject.FindProperty("maxNodeScale");

            SerializedProperty drawScanpathLineProp = serializedObject.FindProperty("drawScanpathLine");
            SerializedProperty lineMaterialProp = serializedObject.FindProperty("lineMaterial");
            SerializedProperty lineWidthProp = serializedObject.FindProperty("lineWidth");
            SerializedProperty useWorldSpaceLineProp = serializedObject.FindProperty("useWorldSpaceLine");

            SerializedProperty clearVisualsOnResetProp = serializedObject.FindProperty("clearVisualsOnReset");

            EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(basicMetricsProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Node Creation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fixationNodePrefabProp);
            EditorGUILayout.PropertyField(nodesParentProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(surfaceOffsetProp);
            EditorGUILayout.PropertyField(mergeDistanceProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(baseNodeScaleProp);
            EditorGUILayout.PropertyField(scaleIncreasePerFixationProp);
            EditorGUILayout.PropertyField(maxNodeScaleProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Line", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(drawScanpathLineProp);
            if (drawScanpathLineProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(lineMaterialProp);
                EditorGUILayout.PropertyField(lineWidthProp);
                EditorGUILayout.PropertyField(useWorldSpaceLineProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lifecycle", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(clearVisualsOnResetProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif