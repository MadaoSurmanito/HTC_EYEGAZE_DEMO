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

        // Reference to the currently highlighted object's renderer
        private Renderer currentRenderer;

        // Store the original color of the currently highlighted object
        private Color originalColor;
        private bool hasOriginalColor;

        // Called every frame when valid gaze data is available.
        public override void ProcessFrame(EyeGazeFrameData frameData)
        {
            SetHighlightedTarget(frameData.HitObject);
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
    }
}