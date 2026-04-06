# Metrics Implementation in Unity

## AOI (Areas of Interest)

Each object with a collider is treated as an Area of Interest.

## Fixation Definition

A fixation is defined as:
A gaze maintained on an object for a minimum time threshold (e.g., 150 ms)

## Metrics Calculation

- TFF: time from start until first fixation
- FC: number of fixations per object
- TFD: total time spent looking at an object
- FD: average fixation duration

## Base System

- Gaze-based raycasting
- Object detection
- Time accumulation per object