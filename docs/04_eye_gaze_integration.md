# Eye Gaze Integration in Unity

## Overview

The eye gaze system is implemented as a modular architecture in Unity.

A central module reads eye tracking input, performs gaze-based raycasting and distributes the resulting frame data to a set of optional processing modules. This design allows each feature to be enabled, disabled or extended independently.

The current architecture separates:

- gaze input acquisition
- raycast and visual fixation point generation
- debug visualization
- highlight feedback
- dwell-based accumulation
- fixation-based metrics
- scanpath visualization

## Main Architecture

### EyeGazeSystem

`EyeGazeSystem` is the main orchestration module.

Its responsibilities are:

- read eye gaze position, rotation and tracking state through Unity Input System
- validate whether gaze tracking is currently available
- build a gaze ray from the latest valid pose
- perform raycasting against the configured layer mask
- generate a shared `EyeGazeFrameData` object
- compute both the physical hit and the visual fixation point
- optionally clamp the visual fixation distance without losing object detection
- forward frame data to all active modules
- notify modules when tracking is lost
- reset module state when the system is disabled

This module acts as the coordinator of the entire eye gaze pipeline.

### EyeGazeFrameData

`EyeGazeFrameData` represents the per-frame data shared with all modules.

It contains information such as:

- tracking validity
- gaze origin
- gaze rotation
- gaze direction
- gaze ray
- hit result
- hit object
- hit point
- hit normal
- ray end point
- delta time
- physical hit availability
- visual fixation point
- visual fixation normal
- whether the visual fixation point is a fallback point

Using a shared frame structure avoids duplicating raycasting logic across modules and keeps the architecture consistent.

### IEyeGazeModule

`IEyeGazeModule` defines the common contract implemented by all optional helper modules.

Each module can:

- initialize itself from the main system
- process valid frame data
- react when tracking is lost
- reset its transient state

### EyeGazeModuleBase

`EyeGazeModuleBase` is an optional abstract base class that reduces boilerplate in module implementations.

It stores the reference to the main system and provides default implementations for methods that do not always require custom logic.

## Visual Fixation Model

The system distinguishes between two related but different concepts:

### Physical hit

A physical hit occurs when the gaze ray intersects a collider inside the configured raycast distance.

This determines:

- the real hit object
- the real hit point
- the real hit normal

### Visual fixation point

The visual fixation point is the point used by visualization-oriented modules such as scanpath rendering.

It may come from:

- the real hit point, when the ray hits a collider
- a fallback point placed forward from the gaze origin, when the ray does not hit anything
- a clamped version of a distant hit, when visual fixation distance clamping is enabled

This distinction is important because an object can still be detected by the raycast while the visualized fixation marker is rendered closer to the user.

## Optional Processing Modules

### EyeGazeHighlighter

Applies visual highlighting to the object currently hit by the gaze ray.

### EyeGazeDebugVisualizer

Draws debug rays and alignment information for calibration and development.

### EyeGazeDwellTracker

Tracks dwell time and gaze entries per object.

### EyeGazeBasicMetrics

Computes fixation-based metrics such as FB, TFF, FD, TFD and FC.

It also emits fixation events that can be consumed by other modules.

### EyeGazeFixationScanpathVisualizer

Creates visual fixation markers in the scene and optionally connects them with a scanpath line.

This module listens to fixation events emitted by `EyeGazeBasicMetrics`.

## Typical Execution Flow

1. Unity updates the scene frame.
2. `EyeGazeSystem` reads the latest gaze pose.
3. If tracking is valid, it constructs the gaze ray.
4. The system performs the raycast.
5. The system computes the visual fixation point.
6. A shared `EyeGazeFrameData` instance is built.
7. Active modules process the frame according to their responsibilities.
8. If tracking is lost, modules are notified.

## Benefits of the Architecture

The modular design improves the project in several ways:

- better separation of responsibilities
- easier maintenance
- easier debugging
- easier experimentation
- easier extension with future gaze-based modules
- clearer distinction between data acquisition, metric computation and visualization
