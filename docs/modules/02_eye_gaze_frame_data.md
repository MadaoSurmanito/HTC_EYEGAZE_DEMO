# EyeGazeFrameData

## Responsibility

`EyeGazeFrameData` is the shared per-frame data container used by runtime modules.

Its purpose is to centralize all gaze-related information for the current frame and avoid duplicating raycast logic in multiple modules.

## Typical Contents

The structure includes data such as:

- tracking validity
- gaze origin
- gaze rotation
- gaze direction
- gaze ray
- hit result
- hit information
- hit object
- hit point
- hit normal
- ray end point
- delta time
- physical hit availability
- visual fixation point
- visual fixation normal
- fallback fixation flag

## Why It Matters

This structure is important because different modules need different views of the same frame:

- the highlighter needs the current hit object
- the debug visualizer needs ray and alignment data
- the dwell tracker needs the hit object and delta time
- the metrics module needs both the metric target and the visual fixation point
- the scanpath visualizer benefits indirectly from the fixation events derived from this data

## Design Benefit

Using a single frame data object makes the architecture:

- cleaner
- easier to maintain
- easier to extend
- less error-prone
