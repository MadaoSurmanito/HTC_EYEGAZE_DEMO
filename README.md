# Eye Gaze Interaction Demo in Unity for HTC VIVE

## Description

Research-oriented Unity demo focused on eye gaze interaction in VR using HTC VIVE headsets and OpenXR.

## Current features

- Eye gaze raycasting
- Object highlighting through gaze
- Dwell time tracking
- Export of gaze dwell reports to TXT
- Experimental support for gaze-based interaction metrics

## Tech stack

- Unity
- OpenXR
- HTC VIVE / SteamVR runtime
- C#

## Project goals

- Validate eye gaze tracking in VR
- Measure gaze-based interaction metrics
- Study user visual behaviour in immersive environments

## Repository structure

- Assets/: Unity project assets
- Packages/: Unity packages
- ProjectSettings/: Unity project settings
- docs/: technical notes and research documentation
- results/: exported reports and experiment summaries

## How to run

1. Clone the repository:

   ````bash
   git clone https://github.com/MadaoSurmanito/HTC_EYEGAZE_DEMO.git```

   ````

2. Open the project in Unity Hub:

- Click on "Add project"
- Select the cloned folder

3. Open the main scene:

- Navigate to Assets/Scenes/ (or your scene folder)
- Open the demo scene

4. Ensure VR setup:

- OpenXR enabled in Project Settings
- SteamVR running as runtime
- Headset connected and recognized

5. Press Play in Unity or build the project to run in VR
