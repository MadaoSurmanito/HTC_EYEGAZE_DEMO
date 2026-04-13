using EyeGaze.Runtime.Core;
using UnityEngine;

namespace EyeGaze.Runtime.Modules
{
    public partial class EyeGazeBasicMetrics
    {
        // Resolves the semantic AOI that should be used for metrics from a raw hit object
        private EyeGazeAOI ResolveValidMetricsAOI(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            // Reject the object if it does not pass the optional layer-mask filter
            if (requireMetricsLayerMask && !IsMetricsLayer(target))
            {
                return null;
            }

            // Resolve the EyeGazeAOI either on the object itself or in its parents
            EyeGazeAOI aoi = searchAOIInParents
                ? target.GetComponentInParent<EyeGazeAOI>()
                : target.GetComponent<EyeGazeAOI>();

            // Reject the target if an AOI component is required and none was found
            if (requireAOIComponent && aoi == null)
            {
                return null;
            }

            // Reject the AOI if it explicitly disables metrics usage
            if (aoi != null && !aoi.IncludeInMetrics)
            {
                return null;
            }

            return aoi;
        }

        // Returns whether the target object belongs to the configured metrics layer mask
        private bool IsMetricsLayer(GameObject target)
        {
            if (target == null)
            {
                return false;
            }

            return (metricsMask.value & (1 << target.layer)) != 0;
        }
    }
}