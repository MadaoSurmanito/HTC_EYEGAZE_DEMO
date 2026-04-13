using System;
using System.Collections.Generic;
using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    // This helper module computes basic eye tracking metrics for each valid AOI hit by the gaze raycast.
    // Implemented metrics:
    // - FB  (Fixations Before)
    // - TFF (Time to First Fixation)
    // - FD  (Average Fixation Duration)
    // - TFD (Total Fixation Duration)
    // - FC  (Fixation Count)
    public partial class EyeGazeBasicMetrics : EyeGazeModuleBase
    {
        // ---------------------------------------------------------------------
        // FIXATION
        // ---------------------------------------------------------------------

        [Header("Fixation")]
        // Minimum continuous gaze time required to consider that a fixation has started
        [SerializeField] private float fixationThreshold = 0.25f;

        // ---------------------------------------------------------------------
        // VISUAL FIXATION EMISSION
        // ---------------------------------------------------------------------

        [Header("Visual Fixation Emission")]
        // If enabled, the module keeps emitting visual fixation events
        // while the user remains on the same visual segment
        [SerializeField] private bool emitRepeatedVisualFixations = true;

        // Time interval between repeated visual fixation event emissions
        [SerializeField] private float repeatedVisualFixationInterval = 0.25f;

        // ---------------------------------------------------------------------
        // AOI FILTERING
        // ---------------------------------------------------------------------

        [Header("AOI Filtering")]
        // If enabled, the object must belong to the metrics layer mask
        [SerializeField] private bool requireMetricsLayerMask = true;

        // Only objects in these layers will be considered for AOI metrics
        [SerializeField] private LayerMask metricsMask = ~0;

        // If enabled, a valid EyeGazeAOI component is required for metrics
        [SerializeField] private bool requireAOIComponent = true;

        // If enabled, the AOI component can be searched in parents too
        [SerializeField] private bool searchAOIInParents = true;

        // ---------------------------------------------------------------------
        // DEBUG
        // ---------------------------------------------------------------------

        [Header("Debug")]
        // Enables or disables logging when a fixation starts
        [SerializeField] private bool logFixationStarts = false;

        // Enables or disables periodic summary logs
        [SerializeField] private bool logPeriodicSummary = false;

        // Number of frames between each summary log when periodic summaries are enabled
        [SerializeField] private int summaryLogEveryNFrames = 300;

        // ---------------------------------------------------------------------
        // EXPORT
        // ---------------------------------------------------------------------

        [Header("Export")]
        // Enables automatic export when the application closes
        [SerializeField] private bool exportOnApplicationQuit = true;

        // If true, exports a TXT report
        [SerializeField] private bool exportToTxt = true;

        // If true, exports a CSV report
        [SerializeField] private bool exportToCsv = true;

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

        // ---------------------------------------------------------------------
        // EVENTS
        // ---------------------------------------------------------------------

        // Event fired when a fixation starts or when a repeated visual fixation is emitted
        public event Action<FixationStartedEventData> FixationStarted;

        // ---------------------------------------------------------------------
        // LIFECYCLE
        // ---------------------------------------------------------------------

        // Called once by the main system during module initialization
        public override void Initialize(EyeGazeSystem systemReference)
        {
            base.Initialize(systemReference);
            InitializeState();
        }

        // Called every frame when valid gaze data is available
        public override void ProcessFrame(EyeGazeFrameData frameData)
        {
            // Resolve the semantic AOI that should be used for metrics
            EyeGazeAOI metricsAOI = ResolveValidMetricsAOI(frameData.HitObject);

            // Update the internal fixation/segment state using both
            // the semantic AOI and the raw visual target information
            UpdateCurrentTarget(
                metricsAOI,
                frameData.HitObject,
                frameData.VisualFixationPoint,
                frameData.VisualFixationNormal,
                frameData.IsFallbackFixationPoint,
                frameData.DeltaTime
            );
        }

        // Called when tracking is lost or invalid gaze data must be handled
        public override void HandleTrackingLost(float deltaTime)
        {
            // Treat tracking loss as a null target and reset the current visual context
            UpdateCurrentTarget(
                null,
                null,
                Vector3.zero,
                Vector3.forward,
                false,
                deltaTime
            );
        }

        // Called when the main system is disabled and transient state should be cleared
        public override void ResetModuleState()
        {
            ClearCurrentTarget();
        }

        // ---------------------------------------------------------------------
        // PUBLIC QUERY METHODS
        // ---------------------------------------------------------------------

        // Returns the metrics data associated with the given AOI, if available
        public BasicMetricsData GetMetrics(EyeGazeAOI aoi)
        {
            if (aoi == null)
            {
                return null;
            }

            return metricsByAOI.TryGetValue(aoi, out BasicMetricsData data) ? data : null;
        }

        // Returns a deep-copy snapshot of the current metrics dictionary
        // This prevents external code from modifying the internal runtime data
        public Dictionary<EyeGazeAOI, BasicMetricsData> GetMetricsSnapshot()
        {
            Dictionary<EyeGazeAOI, BasicMetricsData> snapshot = new();

            foreach (KeyValuePair<EyeGazeAOI, BasicMetricsData> pair in metricsByAOI)
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
    }
}