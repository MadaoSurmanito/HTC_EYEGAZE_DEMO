#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using EyeGaze.Runtime.Core;


namespace EyeGaze.Editor
{
    [CustomEditor(typeof(EyeGazeAOI))]
    public class EyeGazeAOIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EyeGazeAOI aoi = (EyeGazeAOI)target;

            SerializedProperty aoiIdProp = serializedObject.FindProperty("aoiId");
            SerializedProperty aoiLabelProp = serializedObject.FindProperty("aoiLabel");

            SerializedProperty includeInMetricsProp = serializedObject.FindProperty("includeInMetrics");
            SerializedProperty includeInDwellProp = serializedObject.FindProperty("includeInDwell");
            SerializedProperty allowHighlightProp = serializedObject.FindProperty("allowHighlight");
            SerializedProperty allowScanpathProp = serializedObject.FindProperty("allowScanpath");

            EditorGUILayout.HelpBox(
                "EyeGazeAOI defines the semantic role of this object as an Area Of Interest for eye tracking analysis. " +
                "It is independent from colliders and XR interaction components.",
                MessageType.Info
            );

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Identification", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(aoiIdProp);
            EditorGUILayout.PropertyField(aoiLabelProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Usage", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(includeInMetricsProp);
            EditorGUILayout.PropertyField(includeInDwellProp);
            EditorGUILayout.PropertyField(allowHighlightProp);
            EditorGUILayout.PropertyField(allowScanpathProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inspector Checks", EditorStyles.boldLabel);

            Collider collider = aoi.GetComponent<Collider>();
            if (collider == null)
            {
                EditorGUILayout.HelpBox(
                    "This object has no Collider. The gaze raycast will not be able to hit it directly.",
                    MessageType.Warning
                );
            }

            if (aoi.gameObject.layer == 0)
            {
                EditorGUILayout.HelpBox(
                    "This object is still in the Default layer. If your metrics module uses a layer mask, make sure this layer is included or move the AOI to the expected layer.",
                    MessageType.Warning
                );
            }

            UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = aoi.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
            if (interactable != null)
            {
                EditorGUILayout.HelpBox(
                    "This object also has an XR interactable component. That is valid, but remember XR interaction and AOI semantics are independent concerns.",
                    MessageType.Info
                );
            }

            if (
                !includeInMetricsProp.boolValue &&
                !includeInDwellProp.boolValue &&
                !allowHighlightProp.boolValue &&
                !allowScanpathProp.boolValue
            )
            {
                EditorGUILayout.HelpBox(
                    "All usage flags are currently disabled. This AOI will exist semantically, but no current module is allowed to use it.",
                    MessageType.Warning
                );
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Recommended setup:\n" +
                "- Add a Collider so the gaze raycast can hit the object.\n" +
                "- Add EyeGazeAOI if the object should count as an experimental AOI.\n" +
                "- Add XR Simple Interactable only if XR interaction is also needed.",
                MessageType.None
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif