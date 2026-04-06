using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// This helper module measures how long the user looks at each object hit by the eye gaze raycast
// and can export all dwell data to a .txt file.
public class EyeGazeDwellTracker : MonoBehaviour
{
    [Header("Debug")]
    // Enables or disables logging when the current gaze target changes
    [SerializeField] private bool logTargetChanges = true;

    // Enables or disables periodic summary logs for all tracked objects
    [SerializeField] private bool logPeriodicSummary = false;

    // Number of frames between each summary log when periodic summaries are enabled
    [SerializeField] private int summaryLogEveryNFrames = 300;

    [Header("Export")]
    // Enables automatic export when the application closes
    [SerializeField] private bool exportOnApplicationQuit = true;

    // If true, a custom folder path will be used instead of the default persistent data path
    [SerializeField] private bool useCustomOutputDirectory = false;

    // Optional custom folder path where the output file will be stored
    [SerializeField] private string customOutputDirectory = "";

    // Output file name without extension if generateTimestampedFileName is false
    [SerializeField] private string outputFileName = "eye_gaze_dwell_report";

    // If true, the output file name will include a timestamp to avoid overwriting previous exports
    [SerializeField] private bool generateTimestampedFileName = true;

    // If true, object instance IDs will also be exported for easier technical identification
    [SerializeField] private bool includeInstanceId = true;

    // Stores total accumulated dwell time per object instance
    private readonly Dictionary<GameObject, float> totalDwellTimes = new();

    // Stores how many separate gaze entries each object has received
    private readonly Dictionary<GameObject, int> gazeEntryCounts = new();

    // Current object being looked at
    private GameObject currentTarget;

    // Continuous dwell time on the current object
    private float currentTargetContinuousTime;

    // Initialize internal state
    public void Initialize()
    {
        totalDwellTimes.Clear();
        gazeEntryCounts.Clear();
        currentTarget = null;
        currentTargetContinuousTime = 0f;
    }

    // Update the currently gazed object and accumulate dwell time
    public void UpdateCurrentTarget(GameObject newTarget, float deltaTime)
    {
        // If the user is still looking at the same target, continue accumulating time
        if (newTarget == currentTarget)
        {
            if (currentTarget != null)
            {
                currentTargetContinuousTime += deltaTime;
                AddDwellTime(currentTarget, deltaTime);
            }
        }
        // If the target has changed, finalize the previous target and begin tracking the new one
        else
        {
            if (logTargetChanges && currentTarget != null)
            {
                Debug.Log(
                    $"[GAZE DWELL] Exit '{currentTarget.name}' | " +
                    $"ContinuousTime={currentTargetContinuousTime:F3}s | " +
                    $"TotalTime={GetTotalDwellTime(currentTarget):F3}s"
                );
            }

            currentTarget = newTarget;
            currentTargetContinuousTime = 0f;

            if (currentTarget != null)
            {
                if (!gazeEntryCounts.ContainsKey(currentTarget))
                {
                    gazeEntryCounts[currentTarget] = 0;
                }

                gazeEntryCounts[currentTarget]++;

                if (logTargetChanges)
                {
                    Debug.Log(
                        $"[GAZE DWELL] Enter '{currentTarget.name}' | " +
                        $"EntryCount={gazeEntryCounts[currentTarget]}"
                    );
                }

                currentTargetContinuousTime += deltaTime;
                AddDwellTime(currentTarget, deltaTime);
            }
        }

        // Optionally log a periodic summary of all tracked objects
        if (logPeriodicSummary && summaryLogEveryNFrames > 0 && Time.frameCount % summaryLogEveryNFrames == 0)
        {
            LogSummary();
        }
    }

    // Clear the currently tracked object without erasing accumulated history
    public void ClearCurrentTarget()
    {
        currentTarget = null;
        currentTargetContinuousTime = 0f;
    }

    // Returns the total accumulated dwell time for a given object
    public float GetTotalDwellTime(GameObject target)
    {
        if (target == null)
        {
            return 0f;
        }

        return totalDwellTimes.TryGetValue(target, out float value) ? value : 0f;
    }

    // Returns the object currently being looked at
    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    // Returns the current continuous dwell time on the currently looked-at object
    public float GetCurrentTargetContinuousTime()
    {
        return currentTargetContinuousTime;
    }

    // Log a summary of total dwell times for all tracked objects
    public void LogSummary()
    {
        Debug.Log("[GAZE DWELL] ----- Summary Start -----");

        foreach (KeyValuePair<GameObject, float> pair in totalDwellTimes)
        {
            GameObject target = pair.Key;
            float totalTime = pair.Value;
            int entryCount = gazeEntryCounts.TryGetValue(target, out int count) ? count : 0;

            if (target != null)
            {
                Debug.Log(
                    $"[GAZE DWELL] Object='{target.name}' | " +
                    $"TotalTime={totalTime:F3}s | " +
                    $"EntryCount={entryCount}"
                );
            }
        }

        Debug.Log("[GAZE DWELL] ----- Summary End -----");
    }

    // Returns a read-only snapshot of all accumulated dwell times
    public Dictionary<GameObject, float> GetDwellTimesSnapshot()
    {
        return new Dictionary<GameObject, float>(totalDwellTimes);
    }

    // Export all current dwell data to a .txt file
    public void ExportToTxt()
    {
        try
        {
            string outputDirectory = GetOutputDirectory();
            Directory.CreateDirectory(outputDirectory);

            string filePath = Path.Combine(outputDirectory, GetOutputFileName());

            StringBuilder sb = new StringBuilder();

            // Write metadata header
            sb.AppendLine("Eye Gaze Dwell Report");
            sb.AppendLine($"ExportedAt={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"CurrentTarget={(currentTarget != null ? currentTarget.name : "<none>")}");
            sb.AppendLine($"CurrentTargetContinuousTime={currentTargetContinuousTime.ToString("F6", CultureInfo.InvariantCulture)}");
            sb.AppendLine();

            // Write table header
            if (includeInstanceId)
            {
                sb.AppendLine("ObjectName\tInstanceID\tTotalDwellTimeSeconds\tEntryCount\tIsCurrentTarget");
            }
            else
            {
                sb.AppendLine("ObjectName\tTotalDwellTimeSeconds\tEntryCount\tIsCurrentTarget");
            }

            foreach (KeyValuePair<GameObject, float> pair in totalDwellTimes)
            {
                GameObject target = pair.Key;
                float totalTime = pair.Value;

                if (target == null)
                {
                    continue;
                }

                int entryCount = gazeEntryCounts.TryGetValue(target, out int count) ? count : 0;
                bool isCurrentTarget = target == currentTarget;

                if (includeInstanceId)
                {
                    sb.AppendLine(
                        $"{target.name}\t" +
                        $"{target.GetInstanceID()}\t" +
                        $"{totalTime.ToString("F6", CultureInfo.InvariantCulture)}\t" +
                        $"{entryCount}\t" +
                        $"{(isCurrentTarget ? "1" : "0")}"
                    );
                }
                else
                {
                    sb.AppendLine(
                        $"{target.name}\t" +
                        $"{totalTime.ToString("F6", CultureInfo.InvariantCulture)}\t" +
                        $"{entryCount}\t" +
                        $"{(isCurrentTarget ? "1" : "0")}"
                    );
                }
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            Debug.Log($"[GAZE DWELL] Exported dwell report to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GAZE DWELL] Failed to export dwell report: {ex.Message}");
        }
    }

    // Export automatically when the application closes if enabled
    private void OnApplicationQuit()
    {
        if (exportOnApplicationQuit)
        {
            ExportToTxt();
        }
    }

    // Resolve the output directory using either the custom path or Unity's persistent data path
    private string GetOutputDirectory()
    {
        if (useCustomOutputDirectory && !string.IsNullOrWhiteSpace(customOutputDirectory))
        {
            return customOutputDirectory;
        }

        return Path.Combine(Application.persistentDataPath, "EyeGazeLogs");
    }

    // Build the output file name according to the current settings
    private string GetOutputFileName()
    {
        string safeBaseName = string.IsNullOrWhiteSpace(outputFileName)
            ? "eye_gaze_dwell_report"
            : outputFileName.Trim();

        if (generateTimestampedFileName)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
            return $"{safeBaseName}_{timestamp}.txt";
        }

        return $"{safeBaseName}.txt";
    }

    // Add dwell time to the internal accumulator for a given object
    private void AddDwellTime(GameObject target, float deltaTime)
    {
        if (target == null)
        {
            return;
        }

        if (!totalDwellTimes.ContainsKey(target))
        {
            totalDwellTimes[target] = 0f;
        }

        totalDwellTimes[target] += deltaTime;
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(EyeGazeDwellTracker))]
public class EyeGazeDwellTrackerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnityEditor.SerializedProperty logTargetChangesProp = serializedObject.FindProperty("logTargetChanges");
        UnityEditor.SerializedProperty logPeriodicSummaryProp = serializedObject.FindProperty("logPeriodicSummary");
        UnityEditor.SerializedProperty summaryLogEveryNFramesProp = serializedObject.FindProperty("summaryLogEveryNFrames");

        UnityEditor.SerializedProperty exportOnApplicationQuitProp = serializedObject.FindProperty("exportOnApplicationQuit");
        UnityEditor.SerializedProperty useCustomOutputDirectoryProp = serializedObject.FindProperty("useCustomOutputDirectory");
        UnityEditor.SerializedProperty customOutputDirectoryProp = serializedObject.FindProperty("customOutputDirectory");
        UnityEditor.SerializedProperty outputFileNameProp = serializedObject.FindProperty("outputFileName");
        UnityEditor.SerializedProperty generateTimestampedFileNameProp = serializedObject.FindProperty("generateTimestampedFileName");
        UnityEditor.SerializedProperty includeInstanceIdProp = serializedObject.FindProperty("includeInstanceId");

        UnityEditor.EditorGUILayout.LabelField("Debug", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.PropertyField(logTargetChangesProp);
        UnityEditor.EditorGUILayout.PropertyField(logPeriodicSummaryProp);
        if (logPeriodicSummaryProp.boolValue)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(summaryLogEveryNFramesProp);
            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("Export", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.PropertyField(exportOnApplicationQuitProp);

        UnityEditor.EditorGUILayout.PropertyField(useCustomOutputDirectoryProp);
        if (useCustomOutputDirectoryProp.boolValue)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(customOutputDirectoryProp);
            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUILayout.PropertyField(outputFileNameProp);
        UnityEditor.EditorGUILayout.PropertyField(generateTimestampedFileNameProp);
        UnityEditor.EditorGUILayout.PropertyField(includeInstanceIdProp);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif