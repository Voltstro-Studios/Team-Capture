using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Team_Capture.Rope
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    internal class RopeLine : MonoBehaviour
    {
#if UNITY_EDITOR
        
        private const float Dt = 0.02f;
        
        private LineRenderer lineRenderer;
        private Vector3[] currPositions = Array.Empty<Vector3>();
        private Vector3[] prevPositions = Array.Empty<Vector3>();

        /// <summary>
        ///     Simulate this now?
        /// </summary>
        [Tooltip("Simulate this now?")]
        public bool simulate;

        /// <summary>
        ///     The list of <see cref="RopeAnchor"/> points
        /// </summary>
        [Tooltip("The list of rope anchor points")]
        public List<RopeAnchor> anchors = new List<RopeAnchor>();

        private void Start()
        {
            simulate = false;
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            
            CheckRebuildPositionBuffers();
        }

        private void OnEnable()
        {
            RopeLineRunner.Ropes.Add(this);
        }

        private void OnDisable()
        {
            RopeLineRunner.Ropes.Remove(this);
        }

        private void RebuildPositionBuffer(Vector3 startPos, Vector3 endPos, int firstPos, int numSegments)
        {
            Vector3 dir = endPos - startPos;
            for (int i = 0; i < numSegments + 1; i++)
            {
                currPositions[i + firstPos] = startPos + dir * i / numSegments;
                prevPositions[i + firstPos] = currPositions[i + firstPos];
            }
        }

        private void CheckRebuildPositionBuffers()
        {
            RopeAnchor[] children = GetComponentsInChildren<RopeAnchor>();

            //Add new anchors from below this object
            foreach (RopeAnchor a in children)
            {
                if (anchors.Contains(a)) 
                    continue;
                
                //When a new anchor is found, pick closest existing anchor point and insert next to it
                float dist = float.MaxValue;
                int best = 0;
                for (int i = 0; i < anchors.Count; i++)
                {
                    float d = Vector3.Distance(anchors[i].transform.position, a.transform.position);
                    if (!(d < dist)) 
                        continue;
                    
                    dist = d;
                    best = i;
                }

                anchors.Insert(best, a);
            }

            //Remove anchors that were deleted
            anchors.RemoveAll(x => x == null);

            if (anchors.Count < 2)
                return;

            //Segments and length on first anchor is unused
            anchors[0].numSegments = 0;
            anchors[0].length = 0;

            int totalSegments = 0;
            foreach (RopeAnchor a in anchors)
            {
                if (a != anchors[0])
                {
                    if (a.numSegments < 3) 
                        a.numSegments = 3;
                    if (a.length < 0.1f) 
                        a.length = 0.1f;
                }

                totalSegments += a.numSegments;
            }

            if (currPositions.Length == totalSegments + 1)
                return;

            currPositions = new Vector3[totalSegments + 1];
            prevPositions = new Vector3[totalSegments + 1];
            lineRenderer.positionCount = totalSegments + 1;

            int idx = 0;
            for (int i = 1; i < anchors.Count; i++)
            {
                RebuildPositionBuffer(anchors[i - 1].transform.position, anchors[i].transform.position, idx,
                    anchors[i].numSegments);
                idx += anchors[i].numSegments;
            }
        }

        private void Simulate(float length, int firstPos, int numSegments)
        {
            float segmentLength = length / numSegments;

            for (int i = firstPos; i < firstPos + numSegments; i++)
            {
                Vector3 d = currPositions[i + 1] - currPositions[i];
                float dl = d.magnitude;
                if (dl < segmentLength)
                    continue;
                
                float dif = (dl - segmentLength) / dl;
                float b = i == firstPos ? 0.0f : i == firstPos + numSegments - 1 ? 1.0f : 0.5f;
                currPositions[i] += d * b * dif;
                currPositions[i + 1] -= d * (1.0f - b) * dif;
            }
        }

        public void Tick()
        {
            if (this == null || lineRenderer == null)
            {
                EditorApplication.update -= Tick;
                return;
            }

            CheckRebuildPositionBuffers();

            if (anchors.Count < 2)
                return;

            //Fix constraints
            int idx = 0;
            foreach (RopeAnchor a in anchors)
            {
                idx += a.numSegments;
                currPositions[idx] = a.transform.position;
            }

            //Simulate
            idx = 0;
            foreach (RopeAnchor a in anchors)
            {
                if (a.numSegments > 0)
                    Simulate(a.length, idx, a.numSegments);
                idx += a.numSegments;
            }

            //Apply gravity and copy to old pos
            Vector3 down = transform.InverseTransformDirection(Vector3.down);
            for (int i = 0; i < currPositions.Length; i++)
            {
                Vector3 old = currPositions[i];
                currPositions[i] = currPositions[i] + (currPositions[i] - prevPositions[i]) * 0.98f +
                                   10.0f * down * Dt * Dt;
                prevPositions[i] = old;
            }

            lineRenderer.SetPositions(currPositions);
        }
#endif
    }
}
