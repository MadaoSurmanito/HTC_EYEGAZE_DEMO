# EyeGazeBasicMetrics

## Responsibility

`EyeGazeBasicMetrics` computes fixation-based metrics from the gaze stream.

It also emits fixation events that can be reused by other modules, such as the scanpath visualizer.

## Main Metrics

The module computes:

- **FB**: Fixations Before
- **TFF**: Time to First Fixation
- **FD**: Average Fixation Duration
- **TFD**: Total Fixation Duration
- **FC**: Fixation Count

## Core Concepts

### Metrics target

The module can filter AOIs through a metrics layer mask.

Only objects inside this mask are counted for object-based metrics.

### Visual target

Fixation continuity is evaluated over the current visual segment.

This may differ from the metrics target, especially when visual fallback fixation is active.

## Inspector Parameters

### Fixation

- `fixationThreshold`: minimum continuous time required before a fixation starts

### Visual Fixation Emission

- `emitRepeatedVisualFixations`: enables repeated visual fixation event emission while the same segment continues
- `repeatedVisualFixationInterval`: interval used to emit additional visual fixation events after fixation start

### Metrics Layer Filter

- `metricsMask`: defines which layers are valid for AOI metrics

## Difference Between Threshold and Emission

- `fixationThreshold` decides when a fixation becomes valid
- `repeatedVisualFixationInterval` decides how often new visual fixation events are emitted while the same fixation continues

This distinction is especially useful for scanpath visualization.

## Event Emission

The module emits `FixationStarted` event data containing information such as:

- target object
- world point
- surface normal
- fixation start time
- session elapsed time
- object fixation count
- global fixation index
- fallback fixation flag

## Export

The module supports TXT export of per-object metric values.

## Notes

This module currently mixes two roles:

- computing metrics
- emitting visual fixation events for visualization modules

This is practical and works well, but in a future refactor these concerns could be split into separate event types.