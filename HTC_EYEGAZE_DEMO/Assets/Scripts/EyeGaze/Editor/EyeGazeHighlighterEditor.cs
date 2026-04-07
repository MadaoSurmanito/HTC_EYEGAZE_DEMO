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

            EditorGUILayout.LabelField("Highlighting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("highlightColor"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif