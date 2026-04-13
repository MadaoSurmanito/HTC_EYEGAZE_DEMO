# EyeGazeHighlighter

## Responsibility

`EyeGazeHighlighter` provides visual feedback for the object currently hit by the gaze ray.

It does not read input or perform raycasting by itself.

## Main Behavior

The module reacts to the object supplied by `EyeGazeSystem` and:

- detects when the current hit object changes
- restores the previous object's original appearance
- applies the highlight appearance to the new target when possible

## Input

The module depends on:

- the current hit object
- renderer availability on the target object

## Output

The module modifies object appearance for feedback purposes.

## Notes

This module should remain independent from:

- dwell tracking
- fixation metrics
- scanpath visualization

It is meant only as runtime feedback.
