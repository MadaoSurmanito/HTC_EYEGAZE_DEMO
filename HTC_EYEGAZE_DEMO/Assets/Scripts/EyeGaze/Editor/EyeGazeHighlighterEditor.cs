#if UNITY_EDITOR
using UnityEditor;
using EyeGaze.Runtime.Modules;

namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeHighlighter))]
    public class EyeGazeHighlighterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty highlightColorProp = serializedObject.FindProperty("highlightColor");
            SerializedProperty highlightMaskProp = serializedObject.FindProperty("highlightMask");

            EditorGUILayout.LabelField("Highlighting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(highlightColorProp);
            EditorGUILayout.PropertyField(highlightMaskProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif