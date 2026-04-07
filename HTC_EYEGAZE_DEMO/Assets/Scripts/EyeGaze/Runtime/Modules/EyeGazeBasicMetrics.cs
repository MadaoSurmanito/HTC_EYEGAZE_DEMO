using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    // This helper module computes basic eye tracking metrics for each object hit by the gaze raycast.
    // Implemented metrics:
    // - FB  (Fixations Before)
    // - TFF (Time to First Fixation)
    // - FD  (Average Fixation Duration)
    // - TFD (Total Fixation Duration)
    // - FC  (Fixation Count)
    public class EyeGazeBasicMetrics : EyeGazeModuleBase
    {
        [Header("Fixation")]
        // Minimum continuous gaze time required to consider that a fixation has started
        [SerializeField] private float fixationThreshold = 0.15f;

        [Header("Debug")]
        // Enables or disables logging when a fixation starts
        [SerializeField] private bool logFixationStarts = false;

        // Enables or disables periodic summary logs
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
        [SerializeField] private string outputFileName = "eye_gaze_basic_metrics";

        // If true, the output file name will include a timestamp to avoid overwriting previous exports
        [SerializeField] private bool generateTimestampedFileName = true;

        // If true, object instance IDs will also be exported for easier technical identification
        [SerializeField] private bool includeInstanceId = true;

        // Stores all metrics per tracked object instance
        private readonly Dictionary<GameObject, BasicMetricsData> metricsByObject = new();

        // Current object being looked at
        private GameObject currentTarget;

        // Continuous time spent looking at the current object in the current gaze segment
        private float currentTargetContinuousTime;

        // Whether the current gaze segment has already become a valid fixation
        private bool currentSegmentHasBecomeFixation;

        // Total number of fixations started globally in the current session
        private int totalFixationsStarted;

        // Start time of the current session
        private float sessionStartTime;

        // Data structure that stores the basic metrics for a single object
        [Serializable]
        public class BasicMetricsData
        {
            // FB: number of fixations that occurred before the first fixation on this object
            public int fixationsBefore = -1;

            // TFF: time in seconds from session start until the first fixation on this object
            public float timeToFirstFixation = -1f;

            // TFD: total accumulated fixation duration on this object
            public float totalFixationDuration = 0f;

            // FC: number of fixations on this object
            public int fixationCount = 0;

            // Sum of all individual fixation durations on this object
            public float totalDurationAcrossFixations = 0f;

            // FD: average fixation duration on this object
            public float GetAverageFixationDuration()
            {
                if (fixationCount <= 0)
                {
                    return 0f;
                }

                return totalDurationAcrossFixations / fixationCount;
            }
        }

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
            metricsByObject.Clear();
            currentTarget = null;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
            totalFixationsStarted = 0;
            sessionStartTime = Time.time;
        }

        // Update the currently gazed object and compute fixation-based metrics
        public void UpdateCurrentTarget(GameObject newTarget, float deltaTime)
        {
            if (newTarget == currentTarget)
            {
                ContinueCurrentSegment(deltaTime);
            }
            else
            {
                StartNewSegment(newTarget, deltaTime);
            }

            AccumulateFixationDuration(deltaTime);
            TryWritePeriodicSummary();
        }

        // Clear the currently tracked object without erasing accumulated history
        public void ClearCurrentTarget()
        {
            FinalizeCurrentSegment();
            currentTarget = null;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
        }

        // Returns the metrics of a specific object, or null if it has never been tracked
        public BasicMetricsData GetMetrics(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            return metricsByObject.TryGetValue(target, out BasicMetricsData data) ? data : null;
        }

        // Returns a read-only snapshot of all metrics
        public Dictionary<GameObject, BasicMetricsData> GetMetricsSnapshot()
        {
            Dictionary<GameObject, BasicMetricsData> snapshot = new();

            foreach (KeyValuePair<GameObject, BasicMetricsData> pair in metricsByObject)
            {
                BasicMetricsData source = pair.Value;

                snapshot[pair.Key] = new BasicMetricsData
                {
                    fixationsBefore = source.fixationsBefore,
                    timeToFirstFixation = source.timeToFirstFixation,
                    totalFixationDuration = source.totalFixationDuration,
                    fixationCount = source.fixationCount,
                    totalDurationAcrossFixations = source.totalDurationAcrossFixations
                };
            }

            return snapshot;
        }

        // Log a summary of all current metrics
        public void LogSummary()
        {
            Debug.Log("[GAZE BASIC METRICS] ----- Summary Start -----");

            foreach (KeyValuePair<GameObject, BasicMetricsData> pair in metricsByObject)
            {
                GameObject target = pair.Key;
                BasicMetricsData data = pair.Value;

                if (target == null)
                {
                    continue;
                }

                Debug.Log(
                    $"[GAZE BASIC METRICS] Object='{target.name}' | " +
                    $"FB={data.fixationsBefore} | " +
                    $"TFF={data.timeToFirstFixation.ToString("F3", CultureInfo.InvariantCulture)}s | " +
                    $"FD={data.GetAverageFixationDuration().ToString("F3", CultureInfo.InvariantCulture)}s | " +
                    $"TFD={data.totalFixationDuration.ToString("F3", CultureInfo.InvariantCulture)}s | " +
                    $"FC={data.fixationCount}"
                );
            }

            Debug.Log("[GAZE BASIC METRICS] ----- Summary End -----");
        }

        // Export all current metrics to a .txt file
        public void ExportToTxt()
        {
            try
            {
                string outputDirectory = EyeGazeUtils.GetOutputDirectory(useCustomOutputDirectory, customOutputDirectory);
                Directory.CreateDirectory(outputDirectory);

                string filePath = Path.Combine(
                    outputDirectory,
                    EyeGazeUtils.GetOutputFileName(outputFileName, "eye_gaze_basic_metrics", generateTimestampedFileName)
                );

                StringBuilder sb = new StringBuilder();

                // Write metadata header
                sb.AppendLine("Eye Gaze Basic Metrics Report");
                sb.AppendLine($"ExportedAt={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"FixationThresholdSeconds={fixationThreshold.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"SessionElapsedSeconds={(Time.time - sessionStartTime).ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"CurrentTarget={(currentTarget != null ? currentTarget.name : "<none>")}");
                sb.AppendLine($"CurrentTargetContinuousTime={currentTargetContinuousTime.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"TotalFixationsStarted={totalFixationsStarted}");
                sb.AppendLine();

                // Write table header
                if (includeInstanceId)
                {
                    sb.AppendLine("ObjectName\tInstanceID\tFB\tTFF_Seconds\tFD_Seconds\tTFD_Seconds\tFC\tIsCurrentTarget");
                }
                else
                {
                    sb.AppendLine("ObjectName\tFB\tTFF_Seconds\tFD_Seconds\tTFD_Seconds\tFC\tIsCurrentTarget");
                }

                foreach (KeyValuePair<GameObject, BasicMetricsData> pair in metricsByObject)
                {
                    WriteExportLine(sb, pair.Key, pair.Value);
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                Debug.Log($"[GAZE BASIC METRICS] Exported basic metrics report to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GAZE BASIC METRICS] Failed to export basic metrics report: {ex.Message}");
            }
        }

        // Export automatically when the application closes if enabled
        private void OnApplicationQuit()
        {
            // Finalize the last active segment before exporting
            FinalizeCurrentSegment();

            if (exportOnApplicationQuit)
            {
                ExportToTxt();
            }
        }

        // Continue accumulating the current gaze segment
        private void ContinueCurrentSegment(float deltaTime)
        {
            if (currentTarget == null)
            {
                return;
            }

            currentTargetContinuousTime += deltaTime;
            TryStartFixationOnCurrentTarget();
        }

        // Finalize the old segment and begin a new one
        private void StartNewSegment(GameObject newTarget, float deltaTime)
        {
            FinalizeCurrentSegment();

            currentTarget = newTarget;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;

            if (currentTarget == null)
            {
                return;
            }

            currentTargetContinuousTime += deltaTime;
            TryStartFixationOnCurrentTarget();
        }

        // If the fixation threshold is crossed, register the fixation start
        private void TryStartFixationOnCurrentTarget()
        {
            if (currentTarget == null)
            {
                return;
            }

            if (currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (currentTargetContinuousTime >= fixationThreshold)
            {
                StartFixation(currentTarget);
            }
        }

        // If the current segment is already a fixation, keep adding its duration
        private void AccumulateFixationDuration(float deltaTime)
        {
            if (currentTarget == null || !currentSegmentHasBecomeFixation)
            {
                return;
            }

            BasicMetricsData data = GetOrCreateMetrics(currentTarget);
            data.totalFixationDuration += deltaTime;
        }

        // Write periodic summary logs if enabled
        private void TryWritePeriodicSummary()
        {
            if (logPeriodicSummary && summaryLogEveryNFrames > 0 && Time.frameCount % summaryLogEveryNFrames == 0)
            {
                LogSummary();
            }
        }

        // Register the beginning of a valid fixation on the given object
        private void StartFixation(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            BasicMetricsData data = GetOrCreateMetrics(target);

            totalFixationsStarted++;
            currentSegmentHasBecomeFixation = true;

            // TFF and FB are only assigned the first time the object receives a fixation
            if (data.fixationCount == 0)
            {
                data.timeToFirstFixation = Time.time - sessionStartTime;
                data.fixationsBefore = totalFixationsStarted - 1;
            }

            // FC increases every time a new fixation starts on the object
            data.fixationCount++;

            if (logFixationStarts)
            {
                Debug.Log(
                    $"[GAZE BASIC METRICS] Fixation started on '{target.name}' | " +
                    $"FB={data.fixationsBefore} | " +
                    $"TFF={data.timeToFirstFixation.ToString("F3", CultureInfo.InvariantCulture)}s | " +
                    $"FC={data.fixationCount}"
                );
            }
        }

        // Finalize the currently active gaze segment if it had become a fixation
        private void FinalizeCurrentSegment()
        {
            if (currentTarget == null)
            {
                return;
            }

            if (!currentSegmentHasBecomeFixation)
            {
                return;
            }

            BasicMetricsData data = GetOrCreateMetrics(currentTarget);

            // Add the full fixation duration of this segment to the accumulated fixation durations
            data.totalDurationAcrossFixations += currentTargetContinuousTime;
        }

        // Export a single line for one tracked object
        private void WriteExportLine(StringBuilder sb, GameObject target, BasicMetricsData data)
        {
            if (target == null)
            {
                return;
            }

            bool isCurrentTarget = target == currentTarget;

            string tffText = data.timeToFirstFixation >= 0f
                ? data.timeToFirstFixation.ToString("F6", CultureInfo.InvariantCulture)
                : "-1";

            string fdText = data.GetAverageFixationDuration().ToString("F6", CultureInfo.InvariantCulture);
            string tfdText = data.totalFixationDuration.ToString("F6", CultureInfo.InvariantCulture);

            if (includeInstanceId)
            {
                sb.AppendLine(
                    $"{target.name}\t" +
                    $"{target.GetInstanceID()}\t" +
                    $"{data.fixationsBefore}\t" +
                    $"{tffText}\t" +
                    $"{fdText}\t" +
                    $"{tfdText}\t" +
                    $"{data.fixationCount}\t" +
                    $"{(isCurrentTarget ? "1" : "0")}"
                );
            }
            else
            {
                sb.AppendLine(
                    $"{target.name}\t" +
                    $"{data.fixationsBefore}\t" +
                    $"{tffText}\t" +
                    $"{fdText}\t" +
                    $"{tfdText}\t" +
                    $"{data.fixationCount}\t" +
                    $"{(isCurrentTarget ? "1" : "0")}"
                );
            }
        }

        // Returns the existing metrics entry for an object or creates a new one
        private BasicMetricsData GetOrCreateMetrics(GameObject target)
        {
            if (!metricsByObject.ContainsKey(target))
            {
                metricsByObject[target] = new BasicMetricsData();
            }

            return metricsByObject[target];
        }
    }
}