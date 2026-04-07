using UnityEngine;

namespace EyeGaze.Runtime.Core
{
    // Immutable per-frame data produced by the main eye gaze system and consumed by the modules.
    public readonly struct EyeGazeFrameData
    {
        public readonly bool IsTracked;
        public readonly Vector3 GazeOrigin;
        public readonly Quaternion GazeRotation;
        public readonly Vector3 GazeDirection;
        public readonly Ray GazeRay;
        public readonly bool HasHit;
        public readonly RaycastHit HitInfo;
        public readonly GameObject HitObject;
        public readonly Vector3 RayEndPoint;
        public readonly float DeltaTime;

        public EyeGazeFrameData(
            bool isTracked,
            Vector3 gazeOrigin,
            Quaternion gazeRotation,
            Vector3 gazeDirection,
            Ray gazeRay,
            bool hasHit,
            RaycastHit hitInfo,
            GameObject hitObject,
            Vector3 rayEndPoint,
            float deltaTime)
        {
            IsTracked = isTracked;
            GazeOrigin = gazeOrigin;
            GazeRotation = gazeRotation;
            GazeDirection = gazeDirection;
            GazeRay = gazeRay;
            HasHit = hasHit;
            HitInfo = hitInfo;
            HitObject = hitObject;
            RayEndPoint = rayEndPoint;
            DeltaTime = deltaTime;
        }
    }
}