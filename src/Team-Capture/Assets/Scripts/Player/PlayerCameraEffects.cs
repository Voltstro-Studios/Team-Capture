// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Console;
using Team_Capture.Core;
using UnityEngine;

namespace Team_Capture.Player
{
    /// <summary>
    ///     Applies procedural effects such as camera roll and recoil to the camera
    /// </summary>
    internal class PlayerCameraEffects : MonoBehaviour
    {
        #region Camera Roll
        
        /// <summary>
        ///     The max roll angle
        /// </summary>
        [ConVar("cl_rollangle", "Max view roll angle")]
        public static float rollAngle = 2f;

        /// <summary>
        ///     The speed at which the camera rolls
        /// </summary>
        [ConVar("cl_rollspeed", "The speed of the view roll angle")]
        public static float rollSpeed = 10f;

        private Transform baseTransform;
        private Vector3 velocity;
        
        #endregion

        #region Camera Recoil

        private Vector3 currentRecoilRotation;
        private Vector3 cameraRot;
        private float speed;
        private float returnSpeed;

        #endregion

        private bool isServer;

        private void OnEnable() => FixedUpdateManager.OnFixedUpdate += OnFixedUpdate;

        private void OnDisable() => FixedUpdateManager.OnFixedUpdate -= OnFixedUpdate;

        /// <summary>
        ///     Setup <see cref="PlayerCameraEffects"/>
        /// </summary>
        /// <param name="newTransform"></param>
        /// <param name="server"></param>
        internal void Setup(Transform newTransform, bool server)
        {
            baseTransform = newTransform;
            isServer = server;
        }

        /// <summary>
        ///     Sets the velocity
        /// </summary>
        /// <param name="newVelocity"></param>
        internal void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
        }

        internal void OnWeaponChange(float recoilSpeed, float recoilReturnSpeed)
        {
            currentRecoilRotation = Vector3.zero;
            speed = recoilSpeed;
            returnSpeed = recoilReturnSpeed;
        }

        /// <summary>
        ///     Adds recoil to the camera when the weapon fires
        /// </summary>
        /// <param name="recoilAmount"></param>
        internal void OnWeaponFire(Vector3 recoilAmount)
        {
            currentRecoilRotation += -recoilAmount;
        }

        private void OnFixedUpdate()
        {
            currentRecoilRotation = Vector3.Lerp(currentRecoilRotation, Vector3.zero, returnSpeed * Time.fixedDeltaTime);
            cameraRot = Vector3.Slerp(cameraRot, currentRecoilRotation, speed * Time.fixedDeltaTime);
            Vector3 result = cameraRot;
            if(!isServer)
                result.z += CalcRoll();
            
            transform.localRotation = Quaternion.Euler(result);
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