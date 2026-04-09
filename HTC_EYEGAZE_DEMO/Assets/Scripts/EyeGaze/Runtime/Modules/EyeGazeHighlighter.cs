using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    // This helper module is responsible only for highlighting objects being looked at.
    public class EyeGazeHighlighter : EyeGazeModuleBase
    {
        [Header("Highlighting")]
        // Color to use for highlighting
        [SerializeField] private Color highlightColor = Color.yellow;

        [Header("Layer Filter")]
        // Only objects in these layers will be highlighted
        [SerializeField] private LayerMask highlightMask = ~0;

        // Reference to the currently highlighted object's renderer
        private Renderer currentRenderer;

        // Store the original color of the currently highlighted object
        private Color originalColor;
        private bool hasOriginalColor;

        // Called every frame when valid gaze data is available.
        public override void ProcessFrame(EyeGazeFrameData frameData)
        {
            GameObject highlightableTarget = IsHighlightableLayer(frameData.HitObject)
                ? frameData.HitObject
                : null;

            SetHighlightedTarget(highlightableTarget);
        }

        // Called when tracking is lost or invalid gaze data must be handled.
        public override void HandleTrackingLost(float deltaTime)
        {
            ClearHighlight();
        }

        // Called when the main system is disabled and the module should clear transient state.
        public override void ResetModuleState()
        {
            ClearHighlight();
        }

        // Assign a new target to be highlighted
        public void SetHighlightedTarget(GameObject newTarget)
        {
            Renderer newRenderer = EyeGazeUtils.GetRendererFromGameObject(newTarget);

            if (newRenderer == currentRenderer)
            {
                return;
            }

            ClearHighlight();
            TryApplyHighlight(newRenderer);
        }

        // Restore original color and clear highlight
        public void ClearHighlight()
        {
            RestorePreviousHighlight();
            currentRenderer = null;
            hasOriginalColor = false;
        }

        // Try to apply the highlight effect to a renderer
        private void TryApplyHighlight(Renderer targetRenderer)
        {
            if (!EyeGazeUtils.CanHighlightRenderer(targetRenderer))
            {
                return;
            }

            currentRenderer = targetRenderer;
            originalColor = currentRenderer.material.color;
            hasOriginalColor = true;
            currentRenderer.material.color = highlightColor;
        }

        // Restore the original color of the previously highlighted renderer
        private void RestorePreviousHighlight()
        {
            if (currentRenderer != null && hasOriginalColor && EyeGazeUtils.CanHighlightRenderer(currentRenderer))
            {
                currentRenderer.material.color = originalColor;
            }
        }

        // Returns true only if the object belongs to the allowed highlight layers
        private bool IsHighlightableLayer(GameObject target)
        {
            if (target == null)
            {
                return false;
            }

            return (highlightMask.value & (1 << target.layer)) != 0;
        }
    }
}