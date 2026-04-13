# Metrics Implementation in Unity

## Overview

The project implements gaze-related measurements through independent runtime modules connected to the main gaze system.

This avoids embedding all metric logic directly into the raycasting code and makes the architecture easier to maintain and extend.

The main modules involved are:

- `EyeGazeSystem`
- `EyeGazeDwellTracker`
- `EyeGazeBasicMetrics`
- `EyeGazeFixationScanpathVisualizer`

## AOIs (Areas of Interest)

In the current implementation, each scene object with a collider can be treated as an Area of Interest.

However, two levels must be distinguished:

- **visual fixation continuity**, which depends on the current visual segment
- **metric registration**, which depends on whether the hit object belongs to the configured metrics layer mask

This allows the system to keep visual continuity even when a fixation is not counted as a valid AOI metric event.

## Dwell-Based Processing

`EyeGazeDwellTracker` measures gaze exposure over time.

It maintains:

- current target
- continuous dwell time on the current target
- total dwell time per object
- number of gaze entries per object

This module is useful for simple exposure analysis and for exporting raw dwell summaries.

## Fixation Definition

In the Unity implementation, a fixation starts when the gaze remains continuously on the same visual segment for at least a configurable threshold time.

This threshold is controlled by:

- `fixationThreshold`

A common example value is:

- `0.25 s`

Once this threshold is reached, the current gaze segment becomes a valid fixation.

## Visual Fixation Emission

The current implementation also supports repeated visual fixation emission while the user continues looking at the same visual segment.

This behavior is controlled by:

- `emitRepeatedVisualFixations`
- `repeatedVisualFixationInterval`

This is different from the fixation threshold:

- `fixationThreshold` decides when a fixation starts
- `repeatedVisualFixationInterval` decides how often additional visual fixation events are emitted while the fixation continues

This is especially useful for scanpath visualization, because it allows the system to generate fixation markers even when the user keeps looking at the same object or empty space.

## Fixation-Based Metrics

`EyeGazeBasicMetrics` computes:

- **TFF**: time from session start until the first fixation on an object
- **FC**: number of valid fixations detected on an object
- **TFD**: total fixation duration accumulated on an object
- **FD**: average fixation duration on an object
- **FB**: number of fixations that occurred before the first fixation on that object

These values are registered only for objects included in the configured metrics mask.

## Visual Scanpath Representation

`EyeGazeFixationScanpathVisualizer` listens to fixation events and creates visual nodes in the scene.

The current implementation supports:

- node creation from fixation events
- merging nearby nodes inside the same context
- scaling nodes according to merged fixation count
- optional scanpath line rendering
- limiting the number of visible nodes
- removing the oldest nodes when the limit is exceeded

This turns fixation events into an interpretable visual scanpath.

## Processing Flow

1. `EyeGazeSystem` reads the latest gaze pose.
2. The system performs the raycast.
3. The system computes the visual fixation point.
4. `EyeGazeDwellTracker` updates dwell accumulators.
5. `EyeGazeBasicMetrics` updates fixation state and metric values.
6. `EyeGazeBasicMetrics` emits fixation events.
7. `EyeGazeFixationScanpathVisualizer` receives those events and updates the scanpath visualization.
8. Reports can be exported to TXT for later analysis.

## Independence from Other Modules

The metric modules do not depend on highlighting or debug visualization.

Therefore:

- metrics can be collected without visual feedback
- debug rays can be enabled or disabled without changing metric computation
- scanpath visualization can be enabled or disabled independently
- the same main system can support different experimental configurations

## Exported Results

The implementation currently supports TXT export for metric-oriented modules.

These outputs can be used for inspection, logging or later offline analysis.
