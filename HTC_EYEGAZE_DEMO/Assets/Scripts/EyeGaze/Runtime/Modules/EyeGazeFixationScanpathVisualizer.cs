using System;
using System.Collections.Generic;
using EyeGaze.Runtime.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace EyeGaze.Runtime.Modules
{
    public class EyeGazeFixationScanpathVisualizer : EyeGazeModuleBase
    {
        [Header("Dependencies")]
        [SerializeField] private EyeGazeBasicMetrics basicMetrics;

        [Header("Node Creation")]
        [SerializeField] private GameObject fixationNodePrefab;
        [SerializeField] private Transform nodesParent;

        [Header("Placement")]
        [SerializeField] private float surfaceOffset = 0.01f;
        [SerializeField] private float mergeDistance = 0.08f;

        [Header("Distance Clamp")]
        // Maximum visual distance from camera for rendered fixation nodes.
        // Detection still works beyond this distance; only the rendered node position is clamped.
        [SerializeField] private bool clampRenderedDistance = true;
        [SerializeField] private float maxRenderedDistance = 3.0f;

        [Header("Scale")]
        [SerializeField] private float baseNodeScale = 0.12f;
        [SerializeField] private float scaleIncreasePerFixation = 0.03f;
        [SerializeField] private float maxNodeScale = 0.25f;

        [Header("Line")]
        [SerializeField] private bool drawScanpathLine = true;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private float lineWidth = 0.005f;
        [SerializeField] private bool useWorldSpaceLine = true;

        [Header("Lifecycle")]
        [SerializeField] private bool clearVisualsOnReset = false;

        // Maximum number of fixation nodes kept in scene.
        // When exceeded, the oldest nodes are removed first.
        [SerializeField] private int maxVisibleNodes = 10;

        [Serializable]
        private class FixationNodeData
        {
            public GameObject targetObject;
            public Vector3 worldPosition;
            public Vector3 surfaceNormal;
            public int mergedFixationCount;
            public GameObject visualObject;
            public Transform visualTransform;
            public bool isFallbackNode;
        }

        private readonly List<FixationNodeData> nodes = new();
        private LineRenderer lineRenderer;

        public override void Initialize(EyeGazeSystem systemReference)
        {
            base.Initialize(systemReference);

            Debug.Log("[GAZE SCANPATH VISUALIZER] Initialize() called");

            if (basicMetrics == null)
            {
                Debug.LogWarning("[GAZE SCANPATH VISUALIZER] basicMetrics is NULL before subscription.");
            }
            else
            {
                Debug.Log("[GAZE SCANPATH VISUALIZER] basicMetrics reference is assigned.");
            }

            EnsureLineRenderer();

            if (basicMetrics != null)
            {
                basicMetrics.FixationStarted += OnFixationStarted;
                Debug.Log("[GAZE SCANPATH VISUALIZER] Subscribed to FixationStarted");
            }
            else
            {
                Debug.LogWarning("[GAZE SCANPATH VISUALIZER] Missing EyeGazeBasicMetrics reference.");
            }
        }

        public override void ProcessFrame(EyeGazeFrameData frameData)
        {
        }

        public override void HandleTrackingLost(float deltaTime)
        {
        }

        public override void ResetModuleState()
        {
            if (clearVisualsOnReset)
            {
                ClearAllVisuals();
            }
        }

        private void OnDestroy()
        {
            if (basicMetrics != null)
            {
                basicMetrics.FixationStarted -= OnFixationStarted;
            }
        }

        private void OnFixationStarted(EyeGazeBasicMetrics.FixationStartedEventData fixationData)
        {
            if (fixationData == null)
            {
                Debug.LogWarning("[GAZE SCANPATH VISUALIZER] Received null fixationData.");
                return;
            }

            Debug.Log(
                $"[GAZE SCANPATH VISUALIZER] Received fixation -> " +
                $"Target='{(fixationData.target != null ? fixationData.target.name : "<none>")}' | " +
                $"Point={fixationData.worldPoint} | " +
                $"Fallback={fixationData.isFallbackFixation}"
            );

            Vector3 clampedWorldPoint = GetRenderedWorldPoint(fixationData.worldPoint);

            FixationNodeData mergedNode = FindMergeCandidate(
                fixationData.target,
                clampedWorldPoint,
                fixationData.isFallbackFixation
            );

            if (mergedNode != null)
            {
                MergeIntoExistingNode(
                    mergedNode,
                    clampedWorldPoint,
                    fixationData.surfaceNormal
                );
            }
            else
            {
                CreateNewNode(fixationData, clampedWorldPoint);
            }

            EnforceMaxVisibleNodes();
            UpdateLineRenderer();
        }

        private Vector3 GetRenderedWorldPoint(Vector3 sourceWorldPoint)
        {
            if (!clampRenderedDistance)
            {
                return sourceWorldPoint;
            }

            if (system == null || system.ReferenceCamera == null)
            {
                return sourceWorldPoint;
            }

            Transform cameraTransform = system.ReferenceCamera.transform;
            Vector3 origin = cameraTransform.position;
            Vector3 direction = sourceWorldPoint - origin;

            float distance = direction.magnitude;

            if (distance <= maxRenderedDistance || distance <= Mathf.Epsilon)
            {
                return sourceWorldPoint;
            }

            return origin + (direction.normalized * maxRenderedDistance);
        }

        private FixationNodeData FindMergeCandidate(
            GameObject targetObject,
            Vector3 worldPoint,
            bool isFallbackFixation
        )
        {
            FixationNodeData bestCandidate = null;
            float bestDistance = float.MaxValue;

            foreach (FixationNodeData node in nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (node.isFallbackNode != isFallbackFixation)
                {
                    continue;
                }

                bool sameContext =
                    (node.targetObject != null && targetObject != null && node.targetObject == targetObject) ||
                    (node.targetObject == null && targetObject == null);

                if (!sameContext)
                {
                    continue;
                }

                float distance = Vector3.Distance(node.worldPosition, worldPoint);

                if (distance <= mergeDistance && distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCandidate = node;
                }
            }

            return bestCandidate;
        }

        private void MergeIntoExistingNode(
            FixationNodeData node,
            Vector3 worldPoint,
            Vector3 surfaceNormal
        )
        {
            node.mergedFixationCount++;
            node.worldPosition = Vector3.Lerp(node.worldPosition, worldPoint, 0.35f);

            if (surfaceNormal.sqrMagnitude > 0f)
            {
                node.surfaceNormal = surfaceNormal.normalized;
            }

            UpdateNodeTransform(node);
            UpdateNodeScale(node);
        }

        private void CreateNewNode(
            EyeGazeBasicMetrics.FixationStartedEventData fixationData,
            Vector3 clampedWorldPoint
        )
        {
            GameObject nodeObject = CreateNodeVisualObject();

            FixationNodeData node = new FixationNodeData
            {
                targetObject = fixationData.target,
                worldPosition = clampedWorldPoint,
                surfaceNormal = fixationData.surfaceNormal.sqrMagnitude > 0f
                    ? fixationData.surfaceNormal.normalized
                    : Vector3.forward,
                mergedFixationCount = 1,
                visualObject = nodeObject,
                visualTransform = nodeObject != null ? nodeObject.transform : null,
                isFallbackNode = fixationData.isFallbackFixation
            };

            nodes.Add(node);

            UpdateNodeTransform(node);
            UpdateNodeScale(node);
        }

        private void EnforceMaxVisibleNodes()
        {
            if (maxVisibleNodes <= 0)
            {
                return;
            }

            while (nodes.Count > maxVisibleNodes)
            {
                RemoveOldestNode();
            }
        }

        private void RemoveOldestNode()
        {
            if (nodes.Count == 0)
            {
                return;
            }

            FixationNodeData oldestNode = nodes[0];

            if (oldestNode?.visualObject != null)
            {
                Destroy(oldestNode.visualObject);
            }

            nodes.RemoveAt(0);
        }

        private GameObject CreateNodeVisualObject()
        {
            GameObject nodeObject;

            if (fixationNodePrefab != null)
            {
                Transform parent = nodesParent != null ? nodesParent : transform;
                nodeObject = Instantiate(fixationNodePrefab, parent);
            }
            else
            {
                nodeObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                nodeObject.name = "FixationNode";
                Transform parent = nodesParent != null ? nodesParent : transform;
                nodeObject.transform.SetParent(parent, true);

                Collider createdCollider = nodeObject.GetComponent<Collider>();
                if (createdCollider != null)
                {
                    Destroy(createdCollider);
                }
            }

            nodeObject.name = $"FixationNode_{nodes.Count + 1}";

            Collider collider = nodeObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer = nodeObject.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return nodeObject;
        }

        private void UpdateNodeTransform(FixationNodeData node)
        {
            if (node == null || node.visualTransform == null)
            {
                return;
            }

            Vector3 normal = node.surfaceNormal.sqrMagnitude > 0f
                ? node.surfaceNormal.normalized
                : Vector3.forward;

            node.visualTransform.position = node.worldPosition + (normal * surfaceOffset);
            node.visualTransform.rotation = Quaternion.LookRotation(-normal);
        }

        private void UpdateNodeScale(FixationNodeData node)
        {
            if (node == null || node.visualTransform == null)
            {
                return;
            }

            float scale = baseNodeScale + ((node.mergedFixationCount - 1) * scaleIncreasePerFixation);
            scale = Mathf.Min(scale, maxNodeScale);

            node.visualTransform.localScale = new Vector3(scale, scale, scale);
        }

        private void EnsureLineRenderer()
        {
            if (!drawScanpathLine)
            {
                return;
            }

            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.useWorldSpace = useWorldSpaceLine;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 0;
            lineRenderer.alignment = LineAlignment.View;

            if (lineMaterial != null)
            {
                lineRenderer.material = lineMaterial;
            }
        }

        private void UpdateLineRenderer()
        {
            if (!drawScanpathLine || lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = nodes.Count;

            for (int i = 0; i < nodes.Count; i++)
            {
                FixationNodeData node = nodes[i];
                Vector3 normal = node.surfaceNormal.sqrMagnitude > 0f
                    ? node.surfaceNormal.normalized
                    : Vector3.forward;

                lineRenderer.SetPosition(i, node.worldPosition + (normal * surfaceOffset));
            }
        }

        public void ClearAllVisuals()
        {
            foreach (FixationNodeData node in nodes)
            {
                if (node?.visualObject != null)
                {
                    Destroy(node.visualObject);
                }
            }

            nodes.Clear();

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
        }
    }
}