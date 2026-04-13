# EyeGazeDwellTracker

## Responsibility

`EyeGazeDwellTracker` performs dwell-based accumulation over time for gaze targets.

## Main Behavior

The module tracks:

- current target
- continuous dwell time on the current target
- total dwell time per object
- number of gaze entries per object

## Use Cases

This module is useful for:

- simple attention analysis
- object exposure measurement
- exporting raw dwell summaries
- comparing scene elements by viewing time

## AOI Model

Each object with a collider can be treated as an Area of Interest for dwell tracking.

## Export

This module can export dwell results to TXT for later inspection.

## Notes

Dwell is different from fixation.

Dwell is based on accumulated looking time, while fixation introduces a minimum threshold and a segment validation step.