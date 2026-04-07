using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    // This helper module measures how long the user looks at each object hit by the eye gaze raycast
    // and can export all dwell data to a .txt file.
    public class EyeGazeDwellTracker : EyeGazeModuleBase
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

        // Called once by the main system during initialization.
        public override void Initialize(EyeGazeSystem systemReference)
        {
            base.Initialize(systemReference);
            InitializeState();
        }

        // Called every frame when valid gaze data is available.
        public override void ProcessFrame(EyeGazeFrameData frameData)
        {
            UpdateCurrentTarget(frameData.HitObject, frameData.DeltaTime);
        }

        // Called when tracking is lost or invalid gaze data must be handled.
        public override void HandleTrackingLost(float deltaTime)
        {
            UpdateCurrentTarget(null, deltaTime);
        }

        // Called when the main system is disabled and the module should clear transient state.
        public override void ResetModuleState()
        {
            ClearCurrentTarget();
        }

        // Initialize internal state
        public void InitializeState()
        {
            totalDwellTimes.Clear();
            gazeEntryCounts.Clear();
            currentTarget = null;
            currentTargetContinuousTime = 0f;
        }

        // Update the currently gazed object and accumulate dwell time
        public void UpdateCurrentTarget(GameObject newTarget, float deltaTime)
        {
            if (newTarget == currentTarget)
            {
                ContinueCurrentTarget(deltaTime);
            }
            else
            {
                SwitchToNewTarget(newTarget, deltaTime);
            }

            TryWritePeriodicSummary();
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
                string outputDirectory = EyeGazeUtils.GetOutputDirectory(useCustomOutputDirectory, customOutputDirectory);
                Directory.CreateDirectory(outputDirectory);

                string filePath = Path.Combine(
                    outputDirectory,
                    EyeGazeUtils.GetOutputFileName(outputFileName, "eye_gaze_dwell_report", generateTimestampedFileName)
                );

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
                    WriteExportLine(sb, pair.Key, pair.Value);
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

        // Continue accumulating dwell time on the same target
        private void ContinueCurrentTarget(float deltaTime)
        {
            if (currentTarget == null)
            {
                return;
            }

            currentTargetContinuousTime += deltaTime;
            AddDwellTime(currentTarget, deltaTime);
        }

        // Finalize the old target and start tracking a new target
        private void SwitchToNewTarget(GameObject newTarget, float deltaTime)
        {
            LogTargetExitIfNeeded();

            currentTarget = newTarget;
            currentTargetContinuousTime = 0f;

            if (currentTarget == null)
            {
                return;
            }

            RegisterTargetEntry(currentTarget);
            currentTargetContinuousTime += deltaTime;
            AddDwellTime(currentTarget, deltaTime);
        }

        // Write exit log for the previously tracked object
        private void LogTargetExitIfNeeded()
        {
            if (!logTargetChanges || currentTarget == null)
            {
                return;
            }

            Debug.Log(
                $"[GAZE DWELL] Exit '{currentTarget.name}' | " +
                $"ContinuousTime={currentTargetContinuousTime:F3}s | " +
                $"TotalTime={GetTotalDwellTime(currentTarget):F3}s"
            );
        }

        // Register a new gaze entry for the given object
        private void RegisterTargetEntry(GameObject target)
        {
            if (!gazeEntryCounts.ContainsKey(target))
            {
                gazeEntryCounts[target] = 0;
            }

            gazeEntryCounts[target]++;

            if (logTargetChanges)
            {
                Debug.Log(
                    $"[GAZE DWELL] Enter '{target.name}' | " +
                    $"EntryCount={gazeEntryCounts[target]}"
                );
            }
        }

        // Export a single line for one tracked object
        private void WriteExportLine(StringBuilder sb, GameObject target, float totalTime)
        {
            if (target == null)
            {
                return;
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

        // Write periodic summary logs if enabled
        private void TryWritePeriodicSummary()
        {
            if (logPeriodicSummary && summaryLogEveryNFrames > 0 && Time.frameCount % summaryLogEveryNFrames == 0)
            {
                LogSummary();
            }
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
}