# Eye Gaze Integration in Unity

## Overview

The eye gaze system is implemented as a modular architecture in Unity. A central module reads eye tracking input, performs gaze-based raycasting and distributes the resulting frame data to a set of optional processing modules.

This design replaces a more tightly coupled approach and allows each feature to be enabled, disabled or extended independently.

## Main Architecture

### EyeGazeSystem

`EyeGazeSystem` is the main module of the architecture.

Its responsibilities are:

- Read eye gaze position, rotation and tracking state through Unity Input System
- Validate whether gaze tracking is currently available
- Build a gaze ray from the latest valid pose
- Perform raycasting against the configured layer mask
- Generate the per-frame gaze data structure
- Forward the frame data to all active modules
- Notify modules when tracking is lost
- Reset module state when the system is disabled

This module acts as the coordinator of the entire gaze pipeline.

### EyeGazeFrameData

`EyeGazeFrameData` represents the per-frame data shared with all modules.

It contains information such as:

- Whether tracking is valid
- Gaze origin
- Gaze rotation
- Gaze direction
- Gaze ray
- Whether the ray hit an object
- Hit information
- Hit object
- Ray end point
- Delta time

Using a shared frame structure avoids duplicating raycasting logic across modules and keeps the architecture consistent.

### IEyeGazeModule

`IEyeGazeModule` defines the common contract implemented by the helper modules.

Each module can:

- Initialize itself from the main system
- Process valid frame data
- React when tracking is lost
- Reset its transient state

This interface ensures that all modules can be managed uniformly by `EyeGazeSystem`.

### EyeGazeModuleBase

`EyeGazeModuleBase` is an optional abstract base class that reduces boilerplate in the module implementations.

It stores the reference to the main system and provides default implementations for the methods that do not always require custom logic.

## Support Module

### EyeGazeUtils

`EyeGazeUtils` contains common utility functions shared by the modules, including:

- Output directory resolution
- Export file name generation
- LineRenderer configuration
- Renderer retrieval from GameObjects
- Renderer validation for highlight operations

This utility layer helps avoid code duplication and improves readability.

## Optional Processing Modules

### EyeGazeHighlighter

`EyeGazeHighlighter` is responsible only for visual highlighting.

Its role is to:

- Detect changes in the currently hit object
- Restore the previous object's original material color
- Apply the highlight color to the new target when possible

This module does not know how gaze is read or how the raycast is performed. It only reacts to the object supplied by the main system.

### EyeGazeDebugVisualizer

`EyeGazeDebugVisualizer` is responsible only for debugging support.

Its functions include:

- Drawing the gaze ray
- Drawing the reference camera forward ray
- Drawing the offset between camera position and gaze origin
- Writing periodic diagnostic logs

This module is useful for alignment checks, calibration validation and development debugging.

### EyeGazeDwellTracker

`EyeGazeDwellTracker` is responsible only for dwell-based analysis.

Its functions include:

- Measuring accumulated dwell time per object
- Measuring continuous dwell time on the current object
- Counting gaze entries per object
- Logging periodic summaries
- Exporting dwell reports to TXT

This module treats each object with a collider as a valid Area of Interest.

### EyeGazeBasicMetrics

`EyeGazeBasicMetrics` is responsible only for fixation-based metrics.

Its functions include:

- Detecting fixations using a configurable minimum threshold
- Registering first fixation timing per object
- Counting fixations
- Measuring total fixation duration
- Measuring average fixation duration
- Exporting metric reports to TXT

It supports the following basic metrics:

- FB (Fixations Before)
- TFF (Time to First Fixation)
- FD (Average Fixation Duration)
- TFD (Total Fixation Duration)
- FC (Fixation Count)

## Modularity and Independence

A key property of the system is that the processing modules are optional and independent.

This means:

- Highlighting can be disabled without affecting dwell tracking
- Debug visualization can be disabled without affecting metrics
- Dwell tracking and fixation metrics can run simultaneously or separately
- New modules can be added later without modifying the architectural role of the existing ones

## Typical Execution Flow

1. Unity updates the scene frame
2. `EyeGazeSystem` reads gaze input
3. If tracking is valid, it computes the gaze ray
4. The system performs a raycast
5. A frame data object is built
6. Active modules process the frame according to their responsibilities
7. If tracking is lost, modules are notified so they can clear or preserve their state as needed

## Benefits of the Refactor

The modular refactor improves the project in several ways:

- Better separation of responsibilities
- Easier maintenance
- Better readability
- Easier debugging
- Easier experimentation
- Easier extension with future gaze-based features
