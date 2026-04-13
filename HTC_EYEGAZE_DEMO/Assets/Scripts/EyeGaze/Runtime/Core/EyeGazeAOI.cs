using UnityEngine;

namespace EyeGaze.Runtime.Core
{
    // Semantic Area Of Interest marker for eye gaze experiments.
    // This component defines whether an object should be considered a valid AOI
    // for metrics, dwell tracking, highlighting or scanpath visualization.
    public class EyeGazeAOI : MonoBehaviour
    {
        [Header("Identification")]
        // Optional stable identifier for exports and analysis
        [SerializeField] private string aoiId = "";

        // Optional human-readable label for debugging and exports
        [SerializeField] private string aoiLabel = "";

        [Header("Usage")]
        // Whether this AOI should be included in fixation-based metrics
        [SerializeField] private bool includeInMetrics = true;

        // Whether this AOI should be included in dwell tracking
        [SerializeField] private bool includeInDwell = true;

        // Whether this AOI can be highlighted by the highlighter module
        [SerializeField] private bool allowHighlight = true;

        // Whether this AOI can be used by scanpath visualization
        [SerializeField] private bool allowScanpath = true;

        public string AoiId => string.IsNullOrWhiteSpace(aoiId) ? gameObject.name : aoiId;
        public string AoiLabel => string.IsNullOrWhiteSpace(aoiLabel) ? gameObject.name : aoiLabel;

        public bool IncludeInMetrics => includeInMetrics;
        public bool IncludeInDwell => includeInDwell;
        public bool AllowHighlight => allowHighlight;
        public bool AllowScanpath => allowScanpath;
    }
}