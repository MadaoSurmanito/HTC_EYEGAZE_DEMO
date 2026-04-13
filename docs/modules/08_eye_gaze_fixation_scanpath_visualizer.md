# EyeGazeFixationScanpathVisualizer

## Responsibility

`EyeGazeFixationScanpathVisualizer` turns fixation events into an in-scene visual scanpath representation.

## Dependency

This module depends on `EyeGazeBasicMetrics`, because it listens to the fixation events emitted by that module.

## Main Behavior

When a fixation event is received, the module can:

- create a new fixation node
- merge the fixation into an existing nearby node
- update node position and scale
- draw or update the scanpath line
- limit the number of visible nodes
- remove the oldest nodes when the configured limit is exceeded

## Important Design Choice

The visualizer does not generate fixations by itself.

Instead, it reacts to fixation events generated upstream by `EyeGazeBasicMetrics`.

This means that the quality and frequency of the scanpath depend on the fixation event strategy.

## Repeated Visual Fixations

With repeated visual fixation emission enabled in `EyeGazeBasicMetrics`, the scanpath can generate nodes even if the user keeps looking at the same object or empty space.

This solves the limitation where nodes only appeared when the gaze changed to another object or context.

## Relation with AOIs

The event consumed by this module can now include both:

- the raw target object
- the semantic `EyeGazeAOI`

At the current stage, the module remains primarily visual and does not need to enforce AOI semantics by itself, although it can be extended in the future to do so.

## Inspector Parameters

### Dependencies

- `basicMetrics`: reference to the metrics module that emits fixation events

### Node Creation

- `fixationNodePrefab`: prefab used to represent a fixation
- `nodesParent`: optional parent transform for created nodes

### Placement

- `surfaceOffset`: distance used to offset the node from the surface
- `mergeDistance`: maximum distance used to merge nearby fixations in the same context

### Scale

- `baseNodeScale`: base scale of a fixation node
- `scaleIncreasePerFixation`: scale increase per merged fixation
- `maxNodeScale`: maximum allowed node scale

### Line

- `drawScanpathLine`: enables the scanpath line
- `lineMaterial`: material used by the line renderer
- `lineWidth`: width of the line
- `useWorldSpaceLine`: whether the line uses world space

### Lifecycle

- `clearVisualsOnReset`: clears nodes when the module resets
- `maxVisibleNodes`: maximum number of fixation nodes kept in scene

## Node Limit

The module can keep only a fixed number of visible nodes.

When the limit is exceeded, the oldest nodes are removed first.

This prevents the scene from being overloaded with fixation visuals during long sessions.

## Notes

This module is purely visual.

It should not be responsible for deciding what counts as a fixation. That decision belongs to the metrics layer.
