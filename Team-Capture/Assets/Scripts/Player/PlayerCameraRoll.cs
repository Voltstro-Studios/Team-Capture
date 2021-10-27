using Team_Capture.Console;
using Team_Capture.Core;
using UnityEngine;

namespace Team_Capture.Player
{
    internal class PlayerCameraRoll : MonoBehaviour
    {
        [ConVar("cl_rollangle", "Max view roll angle")]
        public static float rollAngle = 2f;

        [ConVar("cl_rollspeed", "The speed of the view roll angle")]
        public static float rollSpeed = 10f;

        private Transform baseTransform;
        private Vector3 velocity;

        private void OnEnable()
        {
            FixedUpdateManager.OnFixedUpdate += OnFixedUpdate;
        }

        private void OnDisable()
        {
            FixedUpdateManager.OnFixedUpdate -= OnFixedUpdate;
        }

        /// <summary>
        ///     Sets the base transform of the player
        /// </summary>
        /// <param name="newTransform"></param>
        internal void SetBaseTransform(Transform newTransform)
        {
            baseTransform = newTransform;
        }

        /// <summary>
        ///     Sets the velocity
        /// </summary>
        /// <param name="newVelocity"></param>
        internal void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
        }

        private void OnFixedUpdate()
        {
            transform.localRotation = Quaternion.Euler(0, 0, CalcRoll());
        }

        //Yes, more code stol-- borrowed from the Source Engine

        /// <summary>
        ///     Compute roll angle for a particular lateral velocity
        /// </summary>
        /// <returns></returns>
        private float CalcRoll()
        {
            //Get amount of lateral movement
            float side = Vector3.Dot(velocity * Time.fixedDeltaTime * 45f,
                baseTransform.TransformDirection(Vector3.right));

            //Right or left side?
            float sign = side < 0 ? 1 : -1;
            side = Mathf.Abs(side);

            if (side < 1f)
                return 0;

            float value = rollAngle;

            //Hit 100% of rollAngle at rollSpeed. Below that get linear approx.
            if (side < rollSpeed)
                side = side * value / rollSpeed;
            else
                side = value;

            //Scale by right/left sign
            return side * sign;
        }
    }
}