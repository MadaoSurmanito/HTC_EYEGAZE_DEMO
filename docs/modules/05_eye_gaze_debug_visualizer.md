# EyeGazeDebugVisualizer

## Responsibility

`EyeGazeDebugVisualizer` helps inspect gaze behavior, alignment and calibration during development.

## Main Behavior

Its responsibilities may include:

- drawing the gaze ray
- drawing the reference camera forward ray
- drawing the offset between camera position and gaze origin
- logging periodic diagnostic information

## Why It Is Useful

This module is valuable for:

- debugging eye gaze alignment
- checking whether the ray points where expected
- validating runtime calibration
- understanding the difference between gaze origin and camera origin

## Notes

This module is intended for development and debugging.

It should not affect the logic of dwell tracking, metrics or scanpath generation.