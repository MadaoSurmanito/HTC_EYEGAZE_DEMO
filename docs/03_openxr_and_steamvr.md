# OpenXR and Runtime

## OpenXR

OpenXR is used as a standard interface to access VR hardware in an abstracted way from within Unity.

## Runtime

SteamVR is used as the active runtime.

## Relevant Features

- Eye tracking support
- Device access through OpenXR profiles such as EyeGaze
- Compatibility with Unity XR workflows
- Integration with Unity Input System

## Role in the Project

Within this project, OpenXR and the runtime provide the low-level eye gaze data that is later consumed by the `EyeGazeSystem`. The Unity-side architecture does not directly implement hardware access logic beyond input reading, but instead builds a modular processing pipeline on top of the gaze data exposed by the runtime.

## Considerations

- The runtime must be correctly configured
- Eye tracking support depends on hardware-specific capabilities
- Unity must be configured to use OpenXR correctly
- If the runtime does not expose valid gaze tracking data, the higher-level modules will not receive valid frame input
