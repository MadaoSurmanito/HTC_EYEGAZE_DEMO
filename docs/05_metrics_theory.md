# Eye Tracking Metrics

## Basic Concepts

### Fixations

A fixation is a period during which gaze remains stable on a point or object for a sufficient amount of time to be considered meaningful for analysis.

### Saccades

Saccades are rapid eye movements between fixations. In this project they are not measured directly as a primary exported metric, but they conceptually separate successive fixation events.

## Areas of Interest

The analysis is performed over scene objects that act as Areas of Interest (AOIs). In the Unity implementation, each relevant object with a collider may be treated as an AOI.

## Main Metrics

### FB (Fixations Before)

Number of fixations that occurred before the first fixation on a given target.

### TFF (Time to First Fixation)

Time elapsed from the beginning of the session until the first fixation on a given target.

### FD (Fixation Duration)

Average duration of the fixations registered on a target.

### TFD (Total Fixation Duration)

Total accumulated fixation duration on a target.

### FC (Fixation Count)

Total number of fixations registered on a target.

## Interpretation

- Lower TFF may indicate that an element is visually accessible or prominent
- Higher TFD may indicate stronger or more sustained visual attention
- Higher FC may indicate repeated visual returns to the same object
- FB can help characterize the relative visual order in which elements are explored

## Experimental Role

These metrics are used to study user visual behaviour in immersive environments and to support the analysis of gaze-guided interaction in VR.
