using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    public partial class EyeGazeBasicMetrics
    {
        // Updates the fixation state using the current semantic AOI,
        // the raw visual target and the current visual fixation point data
        public void UpdateCurrentTarget(
            EyeGazeAOI newMetricsAOI,
            GameObject newVisualTarget,
            Vector3 visualPoint,
            Vector3 visualNormal,
            bool isFallbackFixation,
            float deltaTime
        )
        {
            // Determine whether the new gaze data still belongs to the same visual segment
            bool sameSegment = IsSameVisualSegment(newVisualTarget, isFallbackFixation);

            if (sameSegment)
            {
                // Continue accumulating time on the current segment
                ContinueCurrentSegment(deltaTime, visualPoint, visualNormal);
            }
            else
            {
                // Finalize the old segment and start a new one
                StartNewSegment(
                    newMetricsAOI,
                    newVisualTarget,
                    visualPoint,
                    visualNormal,
                    isFallbackFixation,
                    deltaTime
                );
            }

            // If the current segment is already a valid fixation, accumulate duration
            AccumulateFixationDuration(deltaTime);

            // Emit repeated visual fixation events if that mode is enabled
            TryEmitRepeatedVisualFixation(deltaTime);

            // Write periodic debug summaries if configured
            TryWritePeriodicSummary();
        }

        // Continues the current segment by updating the latest visual point/normal
        // and increasing the continuous gaze time
        private void ContinueCurrentSegment(float deltaTime, Vector3 visualPoint, Vector3 visualNormal)
        {
            currentVisualHitPoint = visualPoint;
            currentVisualHitNormal = visualNormal.sqrMagnitude > 0f
                ? visualNormal.normalized
                : Vector3.forward;

            currentTargetContinuousTime += deltaTime;
            TryStartFixationOnCurrentTarget();
        }

        // Starts a new segment after finalizing the current one
        private void StartNewSegment(
            EyeGazeAOI newMetricsAOI,
            GameObject newVisualTarget,
            Vector3 visualPoint,
            Vector3 visualNormal,
            bool isFallbackFixation,
            float deltaTime
        )
        {
            // Finalize the previous segment before replacing state
            FinalizeCurrentSegment();

            // Store the new segment context
            currentMetricsAOI = newMetricsAOI;
            currentVisualTarget = newVisualTarget;
            currentVisualIsFallback = isFallbackFixation;
            currentTargetContinuousTime = 0f;
            currentSegmentHasBecomeFixation = false;
            timeSinceLastRepeatedVisualFixation = 0f;
            currentVisualHitPoint = visualPoint;
            currentVisualHitNormal = visualNormal.sqrMagnitude > 0f
                ? visualNormal.normalized
                : Vector3.forward;

            // Count the current frame immediately as part of the new segment
            currentTargetContinuousTime += deltaTime;
            TryStartFixationOnCurrentTarget();
        }

        // Checks whether the current segment has already reached the fixation threshold
        private void TryStartFixationOnCurrentTarget()
        {
            if (currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (currentTargetContinuousTime >= fixationThreshold)
            {
                StartFixation(currentMetricsAOI);
                timeSinceLastRepeatedVisualFixation = 0f;
            }
        }

        // Emits additional visual fixation events while the user remains in the same valid fixation
        private void TryEmitRepeatedVisualFixation(float deltaTime)
        {
            if (!emitRepeatedVisualFixations)
            {
                return;
            }

            if (!currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (repeatedVisualFixationInterval <= 0f)
            {
                return;
            }

            timeSinceLastRepeatedVisualFixation += deltaTime;

            if (timeSinceLastRepeatedVisualFixation < repeatedVisualFixationInterval)
            {
                return;
            }

            // Use a while loop so long frames still emit the correct number of repeated events
            while (timeSinceLastRepeatedVisualFixation >= repeatedVisualFixationInterval)
            {
                timeSinceLastRepeatedVisualFixation -= repeatedVisualFixationInterval;
                EmitVisualFixationEventOnly();
            }
        }

        // Emits a visual fixation event without increasing fixation count
        // This is used to feed visual systems such as scanpath rendering
        private void EmitVisualFixationEventOnly()
        {
            BasicMetricsData data = null;

            if (currentMetricsAOI != null)
            {
                data = GetOrCreateMetrics(currentMetricsAOI);
            }

            FixationStarted?.Invoke(new FixationStartedEventData
            {
                target = currentVisualTarget,
                aoi = currentMetricsAOI,
                worldPoint = currentVisualHitPoint,
                surfaceNormal = currentVisualHitNormal,
                fixationStartTime = Time.time,
                sessionElapsedTime = Time.time - sessionStartTime,
                objectFixationCount = data != null ? data.fixationCount : 0,
                globalFixationIndex = totalFixationsStarted,
                isFallbackFixation = currentVisualIsFallback
            });
        }

        // Accumulates fixation duration frame by frame only while the current segment is a valid fixation
        private void AccumulateFixationDuration(float deltaTime)
        {
            if (!currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (currentMetricsAOI == null)
            {
                return;
            }

            BasicMetricsData data = GetOrCreateMetrics(currentMetricsAOI);
            data.totalFixationDuration += deltaTime;
        }

        // Writes a periodic summary to the console if enabled
        private void TryWritePeriodicSummary()
        {
            if (logPeriodicSummary && summaryLogEveryNFrames > 0 && Time.frameCount % summaryLogEveryNFrames == 0)
            {
                LogSummary();
            }
        }

        // Converts the current segment into a valid fixation
        // Updates AOI metrics and emits the fixation event
        private void StartFixation(EyeGazeAOI metricsAOI)
        {
            totalFixationsStarted++;
            currentSegmentHasBecomeFixation = true;

            BasicMetricsData data = null;

            if (metricsAOI != null)
            {
                data = GetOrCreateMetrics(metricsAOI);

                // Register first-fixation-only values the first time this AOI is fixated
                if (data.fixationCount == 0)
                {
                    data.timeToFirstFixation = Time.time - sessionStartTime;
                    data.fixationsBefore = totalFixationsStarted - 1;
                }

                data.fixationCount++;
            }

            Debug.Log(
                $"[GAZE BASIC METRICS] EVENT FixationStarted -> " +
                $"AOI='{(metricsAOI != null ? metricsAOI.AoiLabel : "<none>")}' | " +
                $"Point={currentVisualHitPoint} | " +
                $"GlobalFixationIndex={totalFixationsStarted} | " +
                $"Fallback={currentVisualIsFallback}"
            );

            FixationStarted?.Invoke(new FixationStartedEventData
            {
                target = currentVisualTarget,
                aoi = metricsAOI,
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
                    $"[GAZE BASIC METRICS] Fixation started on '{(metricsAOI != null ? metricsAOI.AoiLabel : "<none>")}' | " +
                    $"FC={(data != null ? data.fixationCount : 0)}"
                );
            }
        }

        // Finalizes the current segment by adding its total segment duration
        // to the AOI aggregated fixation duration accumulator
        private void FinalizeCurrentSegment()
        {
            if (!currentSegmentHasBecomeFixation)
            {
                return;
            }

            if (currentMetricsAOI == null)
            {
                return;
            }

            BasicMetricsData data = GetOrCreateMetrics(currentMetricsAOI);
            data.totalDurationAcrossFixations += currentTargetContinuousTime;
        }

        // Returns whether the new gaze data still belongs to the same visual segment
        // Fallback segments are compared by fallback context instead of by target object
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