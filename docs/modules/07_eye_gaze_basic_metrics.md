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

This module supports semantic AOIs through `EyeGazeAOI`.

The metrics module does not rely only on raw hit objects. Instead, it can resolve and validate a semantic AOI component from the hit object or its parents.

This allows the system to distinguish between:

- raw physical hit object
- semantic AOI used for analysis
- optional XR interaction components

## Main Responsibilities

At runtime, the module is responsible for:

- resolving the valid AOI used for metrics
- tracking visual continuity across gaze segments
- deciding when a fixation starts
- accumulating fixation-based metric values
- optionally emitting repeated visual fixation events
- exporting aggregated results

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

### Debug

- `logFixationStarts`: enables logging when a fixation starts
- `logPeriodicSummary`: enables periodic summary logs
- `summaryLogEveryNFrames`: number of frames between periodic summaries

### Export

- `exportOnApplicationQuit`: enables automatic export when the application closes
- `exportToTxt`: enables TXT export
- `exportToCsv`: enables CSV export
- `useCustomOutputDirectory`: enables custom output directory usage
- `customOutputDirectory`: output directory path when enabled
- `outputFileName`: base output file name
- `generateTimestampedFileName`: appends a timestamp to the file name
- `includeInstanceId`: includes Unity instance IDs in the exported report

## Difference Between Threshold and Emission

- `fixationThreshold` decides when a fixation becomes valid
- `repeatedVisualFixationInterval` decides how often new visual fixation events are emitted while the same fixation continues

This distinction is especially useful for scanpath visualization.

## Raw Visual Target vs Semantic AOI

The module distinguishes between:

### Raw visual target

The raw `GameObject` currently used for visual continuity.

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

## Export Formats

The module supports two export formats:

### TXT

TXT export is intended mainly for human-readable inspection and debugging.

It includes:

- session metadata
- current runtime state
- a tabular section with one row per AOI

### CSV

CSV export is intended for structured analysis in tools such as:

- Excel
- Google Sheets
- LibreOffice Calc
- Python
- R

The CSV uses semicolon separators for better compatibility with spreadsheet environments that use Spanish locale settings.

## Internal File Organization

The module is internally split into multiple partial class files for maintainability.

Recommended structure:

```text
Assets/Scripts/EyeGaze/Runtime/Modules/BasicMetrics/
├── EyeGazeBasicMetrics.cs
├── EyeGazeBasicMetrics.Types.cs
├── EyeGazeBasicMetrics.State.cs
├── EyeGazeBasicMetrics.Logic.cs
├── EyeGazeBasicMetrics.AOI.cs
└── EyeGazeBasicMetrics.Export.cs
EyeGazeBasicMetrics.cs

Contains:

serialized configuration fields
public lifecycle methods
public metric query methods
event declaration
EyeGazeBasicMetrics.Types.cs

Contains:

BasicMetricsData
FixationStartedEventData
EyeGazeBasicMetrics.State.cs

Contains:

runtime state fields
state initialization
state clearing
metric container creation
EyeGazeBasicMetrics.Logic.cs

Contains:

segment tracking
fixation start logic
repeated visual fixation emission
duration accumulation
fixation finalization
EyeGazeBasicMetrics.AOI.cs

Contains:

semantic AOI resolution
layer-mask filtering helpers
EyeGazeBasicMetrics.Export.cs

Contains:

debug summary output
TXT export
CSV export
export row formatting
CSV escaping utilities
Notes

At the current stage, this module still combines two roles:

computing fixation metrics
emitting visual fixation events for visualization modules

This is practical and works well, but in a future refactor these concerns could be split into separate event types.
```
