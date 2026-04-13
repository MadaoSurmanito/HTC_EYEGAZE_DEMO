# Eye Gaze Integration in Unity

## Overview

The eye gaze system is implemented as a modular architecture in Unity.

A central runtime module reads the eye tracking input, performs the gaze raycast and distributes the resulting frame data to a set of optional processing modules. This design allows each feature to be enabled, disabled or extended independently.

The current architecture separates:

- gaze input acquisition
- raycast and visual fixation point generation
- debug visualization
- highlight feedback
- dwell-based accumulation
- fixation-based metrics
- scanpath visualization
- semantic AOI definition

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

### EyeGazeFrameData

`EyeGazeFrameData` is the shared per-frame container used by all runtime modules.

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

### IEyeGazeModule and EyeGazeModuleBase

`IEyeGazeModule` defines the common contract implemented by the optional runtime modules.

`EyeGazeModuleBase` is a convenience base class that stores the reference to the main system and reduces boilerplate in module implementations.

## AOI Model

The project now separates **semantic AOIs** from **XR interactables**.

### Collider

A collider is required for the gaze raycast to hit an object physically.

A collider only means that the object can be detected by the raycast. It does not automatically mean that the object should count as an experimental AOI.

### EyeGazeAOI

`EyeGazeAOI` is the semantic component that marks an object as an Area Of Interest for eye tracking analysis.

This component defines whether an object should be considered valid for:

- fixation-based metrics
- dwell tracking
- highlighting
- scanpath visualization

This makes AOI definition explicit and independent from interaction systems.

### XR Simple Interactable

`XR Simple Interactable` is now considered optional and independent from AOI semantics.

It should only be used when an object must also participate in XR interaction workflows such as hover, focus or selection.

An object can therefore be:

- an AOI without being an XR interactable
- an XR interactable without being an AOI
- both at the same time

## Visual Fixation Model

The system distinguishes between two related but different concepts.

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

Applies visual highlighting to the current gaze target.

### EyeGazeDebugVisualizer

Draws debug rays and alignment information for calibration and development.

### EyeGazeDwellTracker

Tracks dwell time and gaze entries over valid targets.

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

## Benefits of the Current Architecture

The modular design improves the project in several ways:

- better separation of responsibilities
- easier maintenance
- easier debugging
- easier experimentation
- easier extension with future gaze-based modules
- clearer distinction between physical detection, experimental semantics and XR interaction
