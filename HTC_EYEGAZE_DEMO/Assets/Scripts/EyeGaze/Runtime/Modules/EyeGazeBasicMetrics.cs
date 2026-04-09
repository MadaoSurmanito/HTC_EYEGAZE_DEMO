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
        [SerializeField] private float fixationThreshold = 0.25f;

        [Header("Metrics Layer Filter")]
        // Only objects in these layers will be used for AOI metrics
        [SerializeField] private LayerMask metricsMask = ~0;

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

        // Current AOI target used for metrics
        private GameObject currentMetricsTarget;

        // Current visual target used for fixation continuity.
        // This may be null when fixation occurs in empty space.
        private GameObject currentVisualTarget;

        // Whether current visual fixation context is using a fallback point in empty space
        private bool currentVisualIsFallback;

        // Continuous time spent looking at the current segment
        private float currentTargetContinuousTime;

        // Whether the current gaze segment has already become a valid fixation
        private bool currentSegmentHasBecomeFixation;

        // Total number of fixations started globally in the current session
        private int totalFixationsStarted;

        // Start time of the current session
        private float sessionStartTime;

        // Last visual fixation point and normal
        private Vector3 currentVisualHitPoint;
        private Vector3 currentVisualHitNormal = Vector3.forward;

        [Serializable]
        public class BasicMetricsData
        {
            public int fixationsBefore = -1;
            public float timeToFirstFixation = -1f;
            public float totalFixationDuration = 0f;
            public int fixationCount = 0;
            public float totalDurationAcrossFixations = 0f;

            public float GetAverageFixationDuration()
            {
                if (fixationCount <= 0)
                {
                    return 0f;
                }

                return totalDurationAcrossFixations / fixationCount;
            }
        }

        [Serializable]
        public class FixationStartedEventData
        {
            // AOI target receiving the fixation, or null if fixation occurred in empty space
            public GameObject target;

            // World point used for scanpath rendering
            public Vector3 worldPoint;

            // Surface or fallback normal used for scanpath rendering
            public Vector3 surfaceNormal;

            // Absolute time when fixation started
            public float fixationStartTime;

            // Time elapsed since the session started
            public float sessionElapsedTime;

            // AOI fixation count for target, or 0 if no AOI target exists
            public int objectFixationCount;

            // Running global fixation index in the session
            public int globalFixationIndex;

            // Whether this fixation corresponds to a fallback point in empty space
            public bool isFallbackFixation;
        }

        public event Action<FixationStartedEventData> FixationStarted;

        public override void Initialize(EyeGazeSystem systemReference)
        {
            base.Initialize(systemReference);
            InitializeState();
        }

        public override void ProcessFrame(EyeGazeFrameData frameData)
        {
            GameObject metricsTarget = IsMetricsLayer(frameData.HitObject)
                ? frameData.HitObject
                : null;

            UpdateCurrentTarget(
                metricsTarget,
                frameData.HitObject,
                frameData.VisualFixationPoint,
                frameData.VisualFixationNormal,
                frameData.IsFallbackFixationPoint,
                frameData.DeltaTime
            );
        }

        public override void HandleTrackingLost(float deltaTime)
        {
            UpdateCurrentTarget(
                null,
                null,
                Vector3.zero,
                Vector3.forward,
                false,
                deltaTime
            );
        }

        public override void ResetModuleState()
        {
            ClearCurrentTarget();
        }

        public void InitializeState()
        {
            metricsByObject.Clear();
            currentMetricsTarget = null;
            currentVisualTarget = null;
            currentVisualIsFallback = false;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
            totalFixationsStarted = 0;
            sessionStartTime = Time.time;
            currentVisualHitPoint = Vector3.zero;
            currentVisualHitNormal = Vector3.forward;
        }

        public void UpdateCurrentTarget(
            GameObject newMetricsTarget,
            GameObject newVisualTarget,
            Vector3 visualPoint,
            Vector3 visualNormal,
            bool isFallbackFixation,
            float deltaTime
        )
        {
            bool sameSegment = IsSameVisualSegment(newVisualTarget, isFallbackFixation);

            if (sameSegment)
            {
                ContinueCurrentSegment(deltaTime, visualPoint, visualNormal);
            }
            else
            {
                StartNewSegment(
                    newMetricsTarget,
                    newVisualTarget,
                    visualPoint,
                    visualNormal,
                    isFallbackFixation,
                    deltaTime
                );
            }

            AccumulateFixationDuration(deltaTime);
            TryWritePeriodicSummary();
        }

        public void ClearCurrentTarget()
        {
            FinalizeCurrentSegment();
            currentMetricsTarget = null;
            currentVisualTarget = null;
            currentVisualIsFallback = false;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
            currentVisualHitPoint = Vector3.zero;
            currentVisualHitNormal = Vector3.forward;
        }

        public BasicMetricsData GetMetrics(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            return metricsByObject.TryGetValue(target, out BasicMetricsData data) ? data : null;
        }

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

                sb.AppendLine("Eye Gaze Basic Metrics Report");
                sb.AppendLine($"ExportedAt={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"FixationThresholdSeconds={fixationThreshold.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"SessionElapsedSeconds={(Time.time - sessionStartTime).ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"CurrentMetricsTarget={(currentMetricsTarget != null ? currentMetricsTarget.name : "<none>")}");
                sb.AppendLine($"CurrentVisualTarget={(currentVisualTarget != null ? currentVisualTarget.name : "<none>")}");
                sb.AppendLine($"CurrentVisualIsFallback={(currentVisualIsFallback ? "1" : "0")}");
                sb.AppendLine($"CurrentTargetContinuousTime={currentTargetContinuousTime.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"TotalFixationsStarted={totalFixationsStarted}");
                sb.AppendLine();

                if (includeInstanceId)
                {
                    sb.AppendLine("ObjectName\tInstanceID\tFB\tTFF_Seconds\tFD_Seconds\tTFD_Seconds\tFC\tIsCurrentMetricsTarget");
                }
                else
                {
                    sb.AppendLine("ObjectName\tFB\tTFF_Seconds\tFD_Seconds\tTFD_Seconds\tFC\tIsCurrentMetricsTarget");
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

        private void OnApplicationQuit()
        {
            FinalizeCurrentSegment();

            if (exportOnApplicationQuit)
            {
                ExportToTxt();
            }
        }

        private void ContinueCurrentSegment(float deltaTime, Vector3 visualPoint, Vector3 visualNormal)
        {
            currentVisualHitPoint = visualPoint;
            currentVisualHitNormal = visualNormal.sqrMagnitude > 0f ? visualNormal.normalized : Vector3.forward;

            currentTargetContinuousTime += deltaTime;
            TryStartFixationOnCurrentTarget();
        }

        private void StartNewSegment(
            GameObject newMetricsTarget,
            GameObject newVisualTarget,
            Vector3 visualPoint,
            Vector3 visualNormal,
            bool isFallbackFixation,
            float deltaTime
        )
        {
            FinalizeCurrentSegment();

            currentMetricsTarget = newMetricsTarget;
            currentVisualTarget = newVisualTarget;
            currentVisualIsFallback = isFallbackFixation;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
            currentVisualHitPoint = visualPoint;
            currentVisualHitNormal = visualNormal.sqrMagnitude > 0f ? visualNormal.normalized : Vector3.forward;

            currentTargetContinuousTime += deltaTime;
            TryStartFixationOnCurrentTarget();
        }

        private void TryStartFixationOnCurrentTarget()
        {
            if (currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (currentTargetContinuousTime >= fixationThreshold)
            {
                StartFixation(currentMetricsTarget);
            }
        }

        private void AccumulateFixationDuration(float deltaTime)
        {
            if (!currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (currentMetricsTarget == null)
            {
                return;
            }

            BasicMetricsData data = GetOrCreateMetrics(currentMetricsTarget);
            data.totalFixationDuration += deltaTime;
        }

        private void TryWritePeriodicSummary()
        {
            if (logPeriodicSummary && summaryLogEveryNFrames > 0 && Time.frameCount % summaryLogEveryNFrames == 0)
            {
                LogSummary();
            }
        }

        private void StartFixation(GameObject metricsTarget)
        {
            totalFixationsStarted++;
            currentSegmentHasBecomeFixation = true;

            BasicMetricsData data = null;

            if (metricsTarget != null)
            {
                data = GetOrCreateMetrics(metricsTarget);

                if (data.fixationCount == 0)
                {
                    data.timeToFirstFixation = Time.time - sessionStartTime;
                    data.fixationsBefore = totalFixationsStarted - 1;
                }

                data.fixationCount++;
            }

            Debug.Log(
                $"[GAZE BASIC METRICS] EVENT FixationStarted -> " +
                $"Target='{(metricsTarget != null ? metricsTarget.name : "<none>")}' | " +
                $"Point={currentVisualHitPoint} | " +
                $"GlobalFixationIndex={totalFixationsStarted} | " +
                $"Fallback={currentVisualIsFallback}"
            );

            FixationStarted?.Invoke(new FixationStartedEventData
            {
                target = metricsTarget,
                worldPoint = currentVisualHitPoint,
                surfaceNormal = currentVisualHitNormal,
                fixationStartTime = Time.time,
                sessionElapsedTime = Time.time - sessionStartTime,
                objectFixationCount = data != null ? data.fixationCount : 0,
                globalFixationIndex = totalFixationsStarted,
                isFallbackFixation = currentVisualIsFallback
            });

            if (logFixationStarts)
            {
                Debug.Log(
                    $"[GAZE BASIC METRICS] Fixation started on '{(metricsTarget != null ? metricsTarget.name : "<none>")}' | " +
                    $"FC={(data != null ? data.fixationCount : 0)}"
                );
            }
        }

        private void FinalizeCurrentSegment()
        {
            if (!currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (currentMetricsTarget == null)
            {
                return;
            }

            BasicMetricsData data = GetOrCreateMetrics(currentMetricsTarget);
            data.totalDurationAcrossFixations += currentTargetContinuousTime;
        }

        private void WriteExportLine(StringBuilder sb, GameObject target, BasicMetricsData data)
        {
            if (target == null)
            {
                return;
            }

            bool isCurrentTarget = target == currentMetricsTarget;

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

        private BasicMetricsData GetOrCreateMetrics(GameObject target)
        {
            if (!metricsByObject.ContainsKey(target))
            {
                metricsByObject[target] = new BasicMetricsData();
            }

            return metricsByObject[target];
        }

        private bool IsMetricsLayer(GameObject target)
        {
            if (target == null)
            {
                return false;
            }

            return (metricsMask.value & (1 << target.layer)) != 0;
        }

        private bool IsSameVisualSegment(GameObject newVisualTarget, bool newIsFallback)
        {
            if (currentVisualIsFallback || newIsFallback)
            {
                return currentVisualIsFallback == newIsFallback;
            }

            return currentVisualTarget == newVisualTarget;
        }
    }
}