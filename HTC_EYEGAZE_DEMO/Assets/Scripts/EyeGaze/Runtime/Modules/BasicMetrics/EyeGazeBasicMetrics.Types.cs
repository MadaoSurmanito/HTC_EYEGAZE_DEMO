using System;
using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    public partial class EyeGazeBasicMetrics
    {
        // Stores the aggregated fixation metrics for a single AOI
        [Serializable]
        public class BasicMetricsData
        {
            // Number of fixations that happened before the first fixation on this AOI
            public int fixationsBefore = -1;

            // Time elapsed until the first fixation on this AOI
            public float timeToFirstFixation = -1f;

            // Total fixation time accumulated frame by frame on this AOI
            public float totalFixationDuration = 0f;

            // Number of fixations detected on this AOI
            public int fixationCount = 0;

            // Sum of finalized fixation segment durations on this AOI
            public float totalDurationAcrossFixations = 0f;

            // Returns the average duration across all finalized fixations
            public float GetAverageFixationDuration()
            {
                if (fixationCount <= 0)
                {
                    return 0f;
                }

                return totalDurationAcrossFixations / fixationCount;
            }
        }

        // Event payload emitted when a fixation starts or when a repeated visual fixation is emitted
        [Serializable]
        public class FixationStartedEventData
        {
            // Raw target object receiving the fixation, if any
            public GameObject target;

            // Semantic AOI receiving the fixation, if any
            public EyeGazeAOI aoi;

            // World point used for scanpath rendering
            public Vector3 worldPoint;

            // Surface or fallback normal used for scanpath rendering
            public Vector3 surfaceNormal;

            // Absolute time when fixation started
            public float fixationStartTime;

            // Time elapsed since the session started
            public float sessionElapsedTime;

            // AOI fixation count for target, or 0 if no valid AOI exists
            public int objectFixationCount;

            // Running global fixation index in the session
            public int globalFixationIndex;

            // Whether this fixation corresponds to a fallback point in empty space
            public bool isFallbackFixation;
        }
    }
}