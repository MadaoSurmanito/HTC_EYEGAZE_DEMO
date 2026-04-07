# Eye Gaze Interaction Demo in Unity for HTC VIVE

## Description

Research-oriented Unity demo focused on eye gaze interaction in virtual reality using HTC VIVE headsets, OpenXR and C#.

The project has been refactored into a modular architecture in which a central system reads eye gaze data, performs gaze-based raycasting, and delegates processing to independent optional modules. This design improves maintainability, extensibility and experimental flexibility.

## Current Features

- Eye gaze acquisition through Unity Input System and OpenXR
- Gaze-based raycasting against scene objects
- Modular architecture with independent optional components
- Gaze-based object highlighting
- Debug visualization of gaze rays and gaze origin
- Dwell time tracking per object
- Basic fixation-based eye tracking metrics
- Export of dwell and metric reports to TXT

## Architecture Overview

The system is organized around the following components:

- **EyeGazeSystem**  
  Main orchestration module. Reads gaze input, validates tracking, performs raycasting and distributes frame data to the active modules.

- **EyeGazeUtils**  
  Shared utility module containing reusable helper functions for file export, renderer validation and common support tasks.

- **EyeGazeHighlighter**  
  Optional module that applies visual highlighting to the object currently hit by the gaze ray.

- **EyeGazeDebugVisualizer**  
  Optional module that visualizes gaze rays, camera direction and offset information for debugging and calibration purposes.

- **EyeGazeDwellTracker**  
  Optional module that measures dwell time and gaze entries for each object.

- **EyeGazeBasicMetrics**  
  Optional module that computes basic fixation-based metrics such as FB, TFF, FD, TFD and FC.

## Design Goals

- Validate eye gaze tracking in immersive environments
- Support modular experimental development
- Measure visual interaction metrics in Unity
- Allow enabling or disabling processing modules independently
- Facilitate future extension with new analysis or interaction modules

## Tech Stack

- Unity
- C#
- Unity Input System
- OpenXR
- SteamVR runtime
- HTC VIVE headset with eye tracking

## Repository Structure

- `Assets/` → Unity project assets
- `Packages/` → Unity packages
- `ProjectSettings/` → Unity project settings
- `docs/` → technical notes and research documentation
- `results/` → exported reports and experiment summaries

## Main Documentation

- `01_hardware_setup.md`
- `02_unity_vr_setup.md`
- `03_openxr_and_steamvr.md`
- `04_eye_gaze_integration.md`
- `05_metrics_theory.md`
- `06_metrics_in_unity.md`

## How to Run

1. Clone the repository.

```git clone https://github.com/MadaoSurmanito/HTC_EYEGAZE_DEMO.git```

2. Open the project in Unity Hub.
3. Open the main demo scene.
4. Ensure that:
   - OpenXR is enabled in Project Settings
   - SteamVR is the active runtime
   - The headset is connected and recognized
   - Eye tracking is properly calibrated
5. Press Play in Unity or build the project to run it in VR.

## Notes
- Connection and streaming stability may affect tracking quality.
- Proper calibration is required for reliable gaze data.
- Each interactive object should provide a collider to be treated as an Area of Interest.