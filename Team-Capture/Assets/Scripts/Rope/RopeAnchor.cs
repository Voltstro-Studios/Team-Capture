using UnityEngine;

namespace Team_Capture.Rope
{
    internal class RopeAnchor : MonoBehaviour
    {
        public int numSegments = 5;
        public float length = 5;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(transform.position, Vector3.one * 0.05f);
        }
#endif
    }
}
