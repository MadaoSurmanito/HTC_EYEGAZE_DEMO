# EyeGazeAOI

## Responsibility

`EyeGazeAOI` is the semantic marker that defines whether an object should be considered an Area Of Interest in the eye gaze system.

Its purpose is to separate the experimental meaning of an object from its physical collider and from its optional XR interaction components.

## Why It Was Introduced

Previously, AOIs were approximated using scene configuration such as:

- colliders
- layer assignment
- optional `XR Simple Interactable`

That approach was functional but conceptually mixed three different concerns:

- physical detectability
- experimental AOI semantics
- XR interaction

`EyeGazeAOI` was introduced to make AOI definition explicit and easier to maintain.

## Main Role

An object with `EyeGazeAOI` can be treated as a valid semantic target for:

- fixation metrics
- dwell tracking
- highlighting
- scanpath visualization

This depends on the configuration flags of the component and on the consuming module.

## Inspector Fields

### Identification

- `aoiId`: optional stable identifier for analysis and export
- `aoiLabel`: optional human-readable name for logs, debug messages and reports

If left empty, the system can fall back to the GameObject name.

### Usage

- `includeInMetrics`: whether this AOI is valid for fixation-based metrics
- `includeInDwell`: whether this AOI is valid for dwell accumulation
- `allowHighlight`: whether this AOI may be highlighted
- `allowScanpath`: whether this AOI may participate in scanpath visualization

## Recommended Usage

A typical AOI object should contain:

- a collider
- `EyeGazeAOI`

It may also contain:

- `XR Simple Interactable`, if XR interaction is needed

## Important Separation

### Collider

A collider only allows the gaze raycast to hit the object physically.

A collider does not mean that the object should automatically count as an AOI in the experiment.

### EyeGazeAOI

`EyeGazeAOI` defines the semantic role of the object for analysis.

### XR Simple Interactable

`XR Simple Interactable` defines whether the object participates in the XR Interaction Toolkit workflow.

It is optional and independent from AOI semantics.

## Examples

### Experimental AOI without XR interaction

A poster, screen or static object that should be measured but not interacted with:

- collider
- `EyeGazeAOI`

### AOI with XR interaction

A button or interactive object that should be both measured and interactable:

- collider
- `EyeGazeAOI`
- `XR Simple Interactable`

### Non-AOI collider

A wall or floor surface that can still be hit by the raycast but should not count in metrics:

- collider only

## Current Integration

At the current stage, `EyeGazeBasicMetrics` can be configured to require:

- a metrics layer match
- a valid `EyeGazeAOI` component

This allows gradual migration from layer-based AOIs to explicit semantic AOIs.

## Future Role

`EyeGazeAOI` is intended to become the central semantic layer of the eye tracking system.

Future modules should progressively rely on it instead of inferring AOIs from other components.
