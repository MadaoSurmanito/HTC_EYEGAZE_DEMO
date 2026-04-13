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

## Current AOI Model

This module now supports semantic AOIs through `EyeGazeAOI`.

The metrics module no longer needs to rely only on raw GameObjects. It can resolve and validate a semantic AOI component from the hit object.

This allows the system to distinguish between:

- raw physical hit object
- semantic AOI used for analysis
- optional XR interaction components

## Inspector Parameters

### Fixation

- `fixationThreshold`: minimum continuous time required before a fixation starts

### Visual Fixation Emission

- `emitRepeatedVisualFixations`: enables repeated visual fixation event emission while the same segment continues
- `repeatedVisualFixationInterval`: interval used to emit additional visual fixation events after fixation start

### AOI Filtering

- `requireMetricsLayerMask`: if enabled, the hit object must belong to the configured metrics layer mask
- `metricsMask`: defines which layers are valid for metric candidates
- `requireAOIComponent`: if enabled, a valid `EyeGazeAOI` component is required
- `searchAOIInParents`: if enabled, the module can resolve `EyeGazeAOI` in the object hierarchy

## Difference Between Threshold and Emission

- `fixationThreshold` decides when a fixation becomes valid
- `repeatedVisualFixationInterval` decides how often new visual fixation events are emitted while the same fixation continues

This distinction is especially useful for scanpath visualization.

## Raw Visual Target vs Semantic AOI

The module distinguishes between:

### Raw visual target

The raw GameObject currently used for visual continuity.

This target is useful to decide whether the current visual segment is still the same.

### Semantic AOI

The resolved `EyeGazeAOI` used for metric registration.

This AOI is the real experimental target used for FB, TFF, FD, TFD and FC.

## Event Emission

The module emits `FixationStarted` event data containing:

- raw target object
- semantic AOI
- world point
- surface normal
- fixation start time
- session elapsed time
- AOI fixation count
- global fixation index
- fallback fixation flag

This keeps compatibility with visualization modules while also exposing semantic AOI information.

## Export

The module supports TXT export of per-AOI metric values.

The export can include:

- AOI label
- AOI identifier
- instance ID
- FB
- TFF
- FD
- TFD
- FC
- whether the AOI is the current one

## Notes

At the current stage, this module still combines two roles:

- computing fixation metrics
- emitting visual fixation events for visualization modules

This is practical and works well, but in a future refactor these concerns could be split into separate event types.
