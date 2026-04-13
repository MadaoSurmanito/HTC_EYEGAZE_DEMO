using System.Collections.Generic;
using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    public partial class EyeGazeBasicMetrics
    {
        // ---------------------------------------------------------------------
        // RUNTIME STATE
        // ---------------------------------------------------------------------

        // Stores all metrics per AOI
        private readonly Dictionary<EyeGazeAOI, BasicMetricsData> metricsByAOI = new();

        // Current AOI target used for metrics
        private EyeGazeAOI currentMetricsAOI;

        // Current raw visual target used for fixation continuity
        // This may be null when fixation occurs in empty space
        private GameObject currentVisualTarget;

        // Whether current visual fixation context is using a fallback point in empty space
        private bool currentVisualIsFallback;

        // Continuous time spent looking at the current segment
        private float currentTargetContinuousTime;

        // Whether the current gaze segment has already become a valid fixation
        private bool currentSegmentHasBecomeFixation;

        // Timer used to emit repeated visual fixation events while staying in the same segment
        private float timeSinceLastRepeatedVisualFixation;

        // Total number of fixations started globally in the current session
        private int totalFixationsStarted;

        // Start time of the current session
        private float sessionStartTime;

        // Last visual fixation point used by the current segment
        private Vector3 currentVisualHitPoint;

        // Last visual fixation normal used by the current segment
        private Vector3 currentVisualHitNormal = Vector3.forward;

        // ---------------------------------------------------------------------
        // STATE MANAGEMENT
        // ---------------------------------------------------------------------

        // Initializes or resets the full internal runtime state
        public void InitializeState()
        {
            metricsByAOI.Clear();
            currentMetricsAOI = null;
            currentVisualTarget = null;
            currentVisualIsFallback = false;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
            timeSinceLastRepeatedVisualFixation = 0f;
            totalFixationsStarted = 0;
            sessionStartTime = Time.time;
            currentVisualHitPoint = Vector3.zero;
            currentVisualHitNormal = Vector3.forward;
        }

        // Clears the current active target/segment state
        // Finalizes the current segment first so accumulated data is not lost
        public void ClearCurrentTarget()
        {
            FinalizeCurrentSegment();
            currentMetricsAOI = null;
            currentVisualTarget = null;
            currentVisualIsFallback = false;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
            timeSinceLastRepeatedVisualFixation = 0f;
            currentVisualHitPoint = Vector3.zero;
            currentVisualHitNormal = Vector3.forward;
        }

        // Returns the existing metrics container for an AOI or creates one if missing
        private BasicMetricsData GetOrCreateMetrics(EyeGazeAOI aoi)
        {
            if (!metricsByAOI.ContainsKey(aoi))
            {
                metricsByAOI[aoi] = new BasicMetricsData();
            }

            return metricsByAOI[aoi];
        }
    }
}