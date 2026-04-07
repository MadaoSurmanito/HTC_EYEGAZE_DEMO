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

            EditorGUILayout.LabelField("Raycast", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hitMask"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("referenceCamera"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optional Modules", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moduleBehaviours"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif