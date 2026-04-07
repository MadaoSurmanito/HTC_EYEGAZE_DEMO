# Metrics Implementation in Unity

## AOI (Areas of Interest)

Each scene object with a collider can be treated as an Area of Interest.

The gaze ray produced by the main system is used to determine whether the user is currently looking at an AOI.

## Base Architecture

The metrics are not computed directly inside the raycasting logic. Instead, they are implemented as independent modules connected to the main gaze system.

The base architecture is composed of:

- `EyeGazeSystem` for input reading and raycasting
- `EyeGazeDwellTracker` for dwell-based accumulation
- `EyeGazeBasicMetrics` for fixation-based metrics

This separation keeps the metric logic independent from visualization or debugging features.

## Dwell-Based Processing

`EyeGazeDwellTracker` maintains:

- Current target
- Continuous dwell time on the current target
- Total dwell time per object
- Number of gaze entries per object

This module is useful for simple gaze exposure analysis and for exporting raw dwell summaries.

## Fixation Definition

In the Unity implementation, a fixation is defined as a continuous gaze maintained on the same object for at least a configurable threshold time.

A common threshold example is:

- 150 ms

Once this threshold is reached, the current gaze segment is considered a valid fixation.

## Fixation-Based Metrics

`EyeGazeBasicMetrics` computes:

- **TFF**: time from session start until the first fixation on an object
- **FC**: number of valid fixations detected on an object
- **TFD**: total fixation duration accumulated on an object
- **FD**: average fixation duration on an object
- **FB**: number of fixations that occurred before the first fixation on that object

## Processing Logic

The general processing flow is:

1. `EyeGazeSystem` reads the latest gaze pose
2. The system performs a raycast
3. The hit object is propagated to the metrics modules
4. `EyeGazeDwellTracker` updates dwell accumulators
5. `EyeGazeBasicMetrics` updates fixation state and metric values
6. Reports can be exported to TXT for later analysis

## Independence from Other Modules

The metric modules do not depend on highlighting or debug visualization.

Therefore:

- Metrics can be collected without visual feedback
- Debug rays can be enabled or disabled without changing metric computation
- The same main system can support different experimental configurations

## Exported Results

The implementation supports exporting TXT reports containing per-object values, which can later be used for inspection, analysis or experimental logging.