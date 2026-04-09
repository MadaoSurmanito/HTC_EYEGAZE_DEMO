#if UNITY_EDITOR
using UnityEditor;
using EyeGaze.Runtime.Core;

namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeSystem))]
    public class EyeGazeSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty maxDistanceProp = serializedObject.FindProperty("maxDistance");
            SerializedProperty hitMaskProp = serializedObject.FindProperty("hitMask");

            SerializedProperty fallbackFixationDistanceProp = serializedObject.FindProperty("fallbackFixationDistance");
            SerializedProperty clampVisualFixationDistanceProp = serializedObject.FindProperty("clampVisualFixationDistance");
            SerializedProperty maxVisualFixationDistanceProp = serializedObject.FindProperty("maxVisualFixationDistance");

            SerializedProperty referenceCameraProp = serializedObject.FindProperty("referenceCamera");
            SerializedProperty moduleBehavioursProp = serializedObject.FindProperty("moduleBehaviours");

            EditorGUILayout.LabelField("Raycast", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maxDistanceProp);
            EditorGUILayout.PropertyField(hitMaskProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fallback Visual Fixation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fallbackFixationDistanceProp);
            EditorGUILayout.PropertyField(clampVisualFixationDistanceProp);
            if (clampVisualFixationDistanceProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxVisualFixationDistanceProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(referenceCameraProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optional Modules", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(moduleBehavioursProp, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif