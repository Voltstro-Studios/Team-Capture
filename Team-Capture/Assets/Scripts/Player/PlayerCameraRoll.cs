using Team_Capture.Helper.Extensions;
using UnityEngine;

namespace Team_Capture.Player
{
    internal class PlayerCameraRoll : MonoBehaviour
    {
        [SerializeField] private float rollAngle = 2f;
        [SerializeField] private float rollSpeed = 10f;

        public Transform baseTransform;
        
        private Vector3 velocity;
        
        internal void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
        }

        private void Update()
        {
            transform.localRotation = Quaternion.Euler(0, 0, CalcRoll());
        }

        private float CalcRoll()
        {
            //Get amount of lateral movement
            float side = Vector3.Dot(velocity, baseTransform.TransformDirection(Vector3.right));

            //Right or left side?
            float sign = side < 0 ? 1 : -1;
            side = Mathf.Abs(side);

            if (side < 1f)
                return 0;

            float value = rollAngle;

            //Hit 100% of rollAngle at rollSpeed. Below that get linear approx.
            if (side < rollSpeed)
            {
                side = side * value / rollSpeed;
            }
            else
            {
                side = value;
            }

            //Scale by right/left sign
            return side * sign;
        }
    }
}
