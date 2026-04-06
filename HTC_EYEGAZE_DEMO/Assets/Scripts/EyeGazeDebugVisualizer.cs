using UnityEngine;

// This helper module is responsible only for debug visualization and debug logging.
public class EyeGazeDebugVisualizer : MonoBehaviour
{
    [Header("Debug")]
    // Enables or disables all debug line visualizations
    [SerializeField] private bool enableDebugRay = false;

    // Optional LineRenderer to visualize the eye gaze ray in build and in VR
    [SerializeField] private LineRenderer debugLineRenderer;

    // Color of the eye gaze debug ray visualization
    [SerializeField] private Color debugRayColor = Color.red;

    // Optional LineRenderer to visualize the forward direction of the reference camera
    [SerializeField] private LineRenderer debugCameraLineRenderer;

    // Color of the camera debug ray visualization
    [SerializeField] private Color debugCameraRayColor = Color.blue;

    // Optional LineRenderer to visualize the offset between the camera position and the gaze origin
    [SerializeField] private LineRenderer debugOffsetLineRenderer;

    // Color of the offset visualization between camera and gaze origin
    [SerializeField] private Color debugOffsetLineColor = Color.white;

    // Enables or disables periodic debug logs comparing the gaze origin and the camera position
    [SerializeField] private bool enableDebugLogs = false;

    // Number of frames between each debug log when enableDebugLogs is active
    [SerializeField] private int debugLogEveryNFrames = 60;

    private Camera referenceCamera;
    private float maxDistance;

    // Initialize internal references used by this module
    public void Initialize(Camera cameraReference, float rayMaxDistance)
    {
        referenceCamera = cameraReference;
        maxDistance = rayMaxDistance;

        ConfigureLineRenderer(debugLineRenderer, debugRayColor);
        ConfigureLineRenderer(debugCameraLineRenderer, debugCameraRayColor);
        ConfigureLineRenderer(debugOffsetLineRenderer, debugOffsetLineColor);
    }

    // Disable all debug visuals
    public void DisableAll()
    {
        if (debugLineRenderer != null)
        {
            debugLineRenderer.enabled = false;
        }

        if (debugCameraLineRenderer != null)
        {
            debugCameraLineRenderer.enabled = false;
        }

        if (debugOffsetLineRenderer != null)
        {
            debugOffsetLineRenderer.enabled = false;
        }
    }

    // Update all debug visuals and logs using the latest gaze data
    public void UpdateVisualization(Vector3 gazeOrigin, Vector3 gazeDirection, Vector3 gazeEndPoint)
    {
        if (!enableDebugRay)
        {
            DisableAll();
            return;
        }

        // Draw the eye gaze ray
        if (debugLineRenderer != null)
        {
            debugLineRenderer.enabled = true;
            debugLineRenderer.SetPosition(0, gazeOrigin);
            debugLineRenderer.SetPosition(1, gazeEndPoint);
        }

        // Draw the camera forward ray for comparison
        if (debugCameraLineRenderer != null && referenceCamera != null)
        {
            Vector3 cameraStart = referenceCamera.transform.position;
            Vector3 cameraEnd = cameraStart + referenceCamera.transform.forward * maxDistance;

            debugCameraLineRenderer.enabled = true;
            debugCameraLineRenderer.SetPosition(0, cameraStart);
            debugCameraLineRenderer.SetPosition(1, cameraEnd);
        }

        // Draw the offset line between the camera position and the gaze origin
        if (debugOffsetLineRenderer != null && referenceCamera != null)
        {
            debugOffsetLineRenderer.enabled = true;
            debugOffsetLineRenderer.SetPosition(0, referenceCamera.transform.position);
            debugOffsetLineRenderer.SetPosition(1, gazeOrigin);
        }

        // Periodically log the gaze origin, the camera position, and the difference between them for debugging alignment issues
        if (enableDebugLogs && referenceCamera != null && debugLogEveryNFrames > 0 && Time.frameCount % debugLogEveryNFrames == 0)
        {
            Vector3 cameraPosition = referenceCamera.transform.position;
            Vector3 offset = gazeOrigin - cameraPosition;

            Debug.Log(
                $"[EYE DEBUG] " +
                $"GazeOrigin={gazeOrigin} | " +
                $"CameraPosition={cameraPosition} | " +
                $"Offset={offset} | " +
                $"OffsetMagnitude={offset.magnitude} | " +
                $"Direction={gazeDirection}"
            );
        }
    }

    // Configure a LineRenderer with the basic settings needed by this module
    private void ConfigureLineRenderer(LineRenderer lineRenderer, Color color)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = enableDebugRay;

        if (lineRenderer.material != null && lineRenderer.material.HasProperty("_Color"))
        {
            lineRenderer.material.color = color;
        }
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(EyeGazeDebugVisualizer))]
public class EyeGazeDebugVisualizerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnityEditor.SerializedProperty enableDebugRayProp = serializedObject.FindProperty("enableDebugRay");
        UnityEditor.SerializedProperty debugLineRendererProp = serializedObject.FindProperty("debugLineRenderer");
        UnityEditor.SerializedProperty debugRayColorProp = serializedObject.FindProperty("debugRayColor");
        UnityEditor.SerializedProperty debugCameraLineRendererProp = serializedObject.FindProperty("debugCameraLineRenderer");
        UnityEditor.SerializedProperty debugCameraRayColorProp = serializedObject.FindProperty("debugCameraRayColor");
        UnityEditor.SerializedProperty debugOffsetLineRendererProp = serializedObject.FindProperty("debugOffsetLineRenderer");
        UnityEditor.SerializedProperty debugOffsetLineColorProp = serializedObject.FindProperty("debugOffsetLineColor");
        UnityEditor.SerializedProperty enableDebugLogsProp = serializedObject.FindProperty("enableDebugLogs");
        UnityEditor.SerializedProperty debugLogEveryNFramesProp = serializedObject.FindProperty("debugLogEveryNFrames");

        UnityEditor.EditorGUILayout.PropertyField(enableDebugRayProp);
        if (enableDebugRayProp.boolValue)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(debugLineRendererProp);
            UnityEditor.EditorGUILayout.PropertyField(debugRayColorProp);
            UnityEditor.EditorGUILayout.PropertyField(debugCameraLineRendererProp);
            UnityEditor.EditorGUILayout.PropertyField(debugCameraRayColorProp);
            UnityEditor.EditorGUILayout.PropertyField(debugOffsetLineRendererProp);
            UnityEditor.EditorGUILayout.PropertyField(debugOffsetLineColorProp);
            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.PropertyField(enableDebugLogsProp);
        if (enableDebugLogsProp.boolValue)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(debugLogEveryNFramesProp);
            UnityEditor.EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif