using UnityEngine;
using UnityEngine.InputSystem;

// This script reads eye gaze data, performs the gaze raycast,
// highlights objects being looked at, and delegates visualization and dwell tracking to helper modules.
public class EyeGazeHighlighter : MonoBehaviour
{
    [Header("Raycast")]
    // Maximum distance for the gaze raycast
    [SerializeField] private float maxDistance = 10f;

    // Layer mask to specify which objects can be highlighted
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Highlighting")]
    // Color to use for highlighting
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("References")]
    // Camera used as reference (usually HMD / Main Camera)
    [SerializeField] private Camera referenceCamera;

    [Header("Modules")]
    // Module used to visualize gaze rays and debug info in scene
    [SerializeField] private EyeGazeDebugVisualizer debugVisualizer;

    // Module used to measure how long each object is being looked at
    [SerializeField] private EyeGazeDwellTracker dwellTracker;

    // InputActions for eye gaze position, rotation and tracking state
    private InputAction gazePositionAction;
    private InputAction gazeRotationAction;
    private InputAction gazeTrackedAction;

    // Reference to the currently highlighted object's renderer
    private Renderer currentRenderer;

    // Store the original color of the currently highlighted object
    private Color originalColor;
    private bool hasOriginalColor;

    // Store the last valid gaze position and rotation
    private Vector3 lastValidPosition;
    private Quaternion lastValidRotation = Quaternion.identity;
    private bool hasValidGazePose;

    // Initialize InputActions and modules
    private void Awake()
    {
        // InputActions for eye gaze
        gazePositionAction = new InputAction(
            name: "EyeGazePosition",
            type: InputActionType.Value,
            binding: "<EyeGaze>/pose/position"
        );

        gazeRotationAction = new InputAction(
            name: "EyeGazeRotation",
            type: InputActionType.Value,
            binding: "<EyeGaze>/pose/rotation"
        );

        gazeTrackedAction = new InputAction(
            name: "EyeGazeTracked",
            type: InputActionType.Value,
            binding: "<EyeGaze>/isTracked"
        );

        // Use main camera if none assigned
        if (referenceCamera == null)
        {
            referenceCamera = Camera.main;
        }

        // Initialize helper modules
        if (debugVisualizer != null)
        {
            debugVisualizer.Initialize(referenceCamera, maxDistance);
        }

        if (dwellTracker != null)
        {
            dwellTracker.Initialize();
        }
    }

    // Enable InputActions
    private void OnEnable()
    {
        gazePositionAction.Enable();
        gazeRotationAction.Enable();
        gazeTrackedAction.Enable();
    }

    // Disable InputActions and clean state
    private void OnDisable()
    {
        gazePositionAction.Disable();
        gazeRotationAction.Disable();
        gazeTrackedAction.Disable();

        ClearHighlight();

        if (debugVisualizer != null)
        {
            debugVisualizer.DisableAll();
        }

        if (dwellTracker != null)
        {
            dwellTracker.ClearCurrentTarget();
        }
    }

    // Main update loop
    private void Update()
    {
        // Read gaze data
        Vector3 gazePosition = gazePositionAction.ReadValue<Vector3>();
        Quaternion gazeRotation = gazeRotationAction.ReadValue<Quaternion>();
        bool isTracked = gazeTrackedAction.ReadValue<float>() > 0.5f;

        hasValidGazePose = isTracked;

        // If valid, store pose
        if (hasValidGazePose)
        {
            lastValidPosition = gazePosition;
            lastValidRotation = gazeRotation;
        }
        else
        {
            // No tracking → reset everything
            ClearHighlight();

            if (debugVisualizer != null)
            {
                debugVisualizer.DisableAll();
            }

            if (dwellTracker != null)
            {
                dwellTracker.UpdateCurrentTarget(null, Time.deltaTime);
            }

            return;
        }

        // Ray from gaze
        Vector3 direction = lastValidRotation * Vector3.forward;
        Ray ray = new Ray(lastValidPosition, direction);

        Vector3 rayEndPoint = lastValidPosition + direction * maxDistance;

        GameObject hitObject = null;

        // Raycast
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
        {
            rayEndPoint = hit.point;
            hitObject = hit.collider.gameObject;

            Renderer hitRenderer = hit.collider.GetComponent<Renderer>();

            if (hitRenderer != currentRenderer)
            {
                ClearHighlight();

                if (hitRenderer != null && hitRenderer.material.HasProperty("_Color"))
                {
                    currentRenderer = hitRenderer;
                    originalColor = currentRenderer.material.color;
                    hasOriginalColor = true;
                    currentRenderer.material.color = highlightColor;
                }
            }
        }
        else
        {
            ClearHighlight();
        }

        // Update visual debug (ray, hit, etc.)
        if (debugVisualizer != null)
        {
            debugVisualizer.UpdateVisualization(
                gazeOrigin: lastValidPosition,
                gazeDirection: direction,
                gazeEndPoint: rayEndPoint
            );
        }

        // Update dwell tracking
        if (dwellTracker != null)
        {
            dwellTracker.UpdateCurrentTarget(hitObject, Time.deltaTime);
        }
    }

    // Restore original color and clear highlight
    private void ClearHighlight()
    {
        if (currentRenderer != null && hasOriginalColor && currentRenderer.material.HasProperty("_Color"))
        {
            currentRenderer.material.color = originalColor;
        }

        currentRenderer = null;
        hasOriginalColor = false;
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(EyeGazeHighlighter))]
public class EyeGazeHighlighterEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistance"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("hitMask"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("highlightColor"));
        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("referenceCamera"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("debugVisualizer"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("dwellTracker"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif