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

In the current implementation, each scene object with a collider can be physically detected by the gaze raycast, but only objects resolved as valid semantic AOIs should be counted in the analysis.

The current semantic AOI mechanism is based on `EyeGazeAOI`.

This allows the system to distinguish between:

- physical hit detection
- visual fixation continuity
- semantic AOI registration for metrics

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

- **TFF**: time from session start until the first fixation on an AOI
- **FC**: number of valid fixations detected on an AOI
- **TFD**: total fixation duration accumulated on an AOI
- **FD**: average fixation duration on an AOI
- **FB**: number of fixations that occurred before the first fixation on that AOI

These values are registered only for valid AOIs after AOI resolution and filtering.

## Export Formats

The project currently supports two export formats for fixation-based metrics:

### TXT

TXT export is intended for debug-oriented reading and manual inspection.

### CSV

CSV export is intended for structured analysis and spreadsheet workflows.

CSV is especially useful for:

- Excel-based analysis
- comparison between AOIs
- filtering and sorting
- later processing in Python or R

The current implementation uses semicolon separators for spreadsheet compatibility in Spanish regional settings.

## Internal Module Structure

`EyeGazeBasicMetrics` has been refactored internally into multiple partial class files.

This improves:

- readability
- maintainability
- separation of concerns
- future extensibility

The internal split separates:

- configuration and public entry points
- runtime state
- fixation logic
- AOI resolution
- export utilities
- event/data types

## Visual Scanpath Representation

`EyeGazeFixationScanpathVisualizer` listens to fixation events and creates visual nodes in the scene.

The current implementation supports:

- node creation from fixation events
- repeated visual fixation sampling
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
4. `EyeGazeBasicMetrics` resolves the valid semantic AOI.
5. The module updates segment continuity and fixation state.
6. The module accumulates fixation metrics.
7. The module emits fixation events.
8. `EyeGazeFixationScanpathVisualizer` receives those events and updates the scanpath visualization.
9. Results can be exported to TXT or CSV for later analysis.

## Independence from Other Modules

The metric modules do not depend on highlighting or debug visualization.

Therefore:

- metrics can be collected without visual feedback
- debug rays can be enabled or disabled without changing metric computation
- scanpath visualization can be enabled or disabled independently
- the same main system can support different experimental configurations

```

```
