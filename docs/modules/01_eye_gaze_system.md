# EyeGazeSystem

## Responsibility

`EyeGazeSystem` is the central runtime module of the architecture.

It is responsible for:

- reading eye gaze input through Unity Input System
- validating whether tracking is available
- generating the gaze ray
- performing physics raycasting
- computing visual fixation data
- building shared frame data
- invoking the optional runtime modules

## Main Responsibilities

### Input acquisition

The system reads:

- gaze position
- gaze rotation
- tracking state

### Ray generation

When tracking is valid, the system generates a gaze ray using the latest valid gaze pose.

### Physics interaction

The system performs a raycast using:

- `maxDistance`
- `hitMask`

### Visual fixation generation

The system distinguishes between:

- physical hit data
- visual fixation data

This allows visualization to remain stable even when no object is hit or when distant hits should be visually clamped.

## Inspector Parameters

### Raycast

- `maxDistance`: maximum raycast distance
- `hitMask`: layers allowed for gaze ray interaction

### Fallback Visual Fixation

- `fallbackFixationDistance`: distance used when the gaze hits nothing
- `clampVisualFixationDistance`: enables visual clamping
- `maxVisualFixationDistance`: maximum allowed rendered fixation distance when clamping is enabled

### References

- `referenceCamera`: camera used as reference, usually the HMD or main XR camera

### Optional Modules

- `moduleBehaviours`: runtime modules driven by the system

## Runtime Output

The module produces an `EyeGazeFrameData` object for each valid frame.

## Notes

This module should remain focused on orchestration and shared gaze data generation.

Metric logic, highlighting and visualization should stay in separate modules.