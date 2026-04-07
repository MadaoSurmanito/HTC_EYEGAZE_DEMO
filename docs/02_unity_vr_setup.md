# Unity VR Setup

## Development Environment

- Engine: Unity
- Language: C#
- Input System: Unity Input System

## XR Configuration

- OpenXR as the main backend
- XR Origin configured in the scene
- Main Camera linked to the HMD

## Objective

Set up a minimal VR environment capable of:

- Rendering in VR
- Reading eye tracking data
- Supporting gaze-based interaction experiments

## Project Requirements in Unity

The Unity project must provide:

- A scene configured for VR rendering
- A valid XR Origin
- A Main Camera associated with the headset
- Colliders on scene objects intended to act as gaze targets
- A GameObject containing the `EyeGazeSystem` component
- Optional eye gaze modules attached and linked from the main system

## Main Runtime Components

The gaze interaction architecture is built around:

- `EyeGazeSystem` as the main controller
- `EyeGazeHighlighter` as an optional visual feedback module
- `EyeGazeDebugVisualizer` as an optional debugging module
- `EyeGazeDwellTracker` as an optional dwell analysis module
- `EyeGazeBasicMetrics` as an optional fixation metrics module

## Scene Preparation

To test the system correctly:

- Add visible objects with colliders
- Ensure the objects are placed in layers included in the gaze raycast mask
- Assign the reference camera if it is not resolved automatically
- Configure the desired modules depending on the experiment

## Result

Once configured, Unity provides a VR environment in which gaze input can be read, interpreted and processed by independent modules.
