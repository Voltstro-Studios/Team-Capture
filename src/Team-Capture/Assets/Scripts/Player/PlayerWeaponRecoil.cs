// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Core;
using UnityEngine;

namespace Team_Capture.Player
{
    internal class PlayerWeaponRecoil : MonoBehaviour
    {
        private float speed;
        private float returnSpeed;
        
        private Vector3 rotationRecoil;
        private Vector3 rotation;
        
        private void OnEnable() => FixedUpdateManager.OnFixedUpdate += OnFixedUpdate;

        private void OnDisable() => FixedUpdateManager.OnFixedUpdate -= OnFixedUpdate;

        private void OnFixedUpdate()
        {
            rotationRecoil = Vector3.Lerp(rotationRecoil, Vector3.zero, returnSpeed * Time.fixedDeltaTime);

            rotation = Vector3.Slerp(rotation, rotationRecoil, speed * Time.fixedDeltaTime);
            transform.localRotation = Quaternion.Euler(rotation);
        }

        internal void OnWeaponChange(float recoilSpeed, float recoilReturnSpeed)
        {
            speed = recoilSpeed;
            returnSpeed = recoilReturnSpeed;
        }

        internal void OnWeaponFire(Vector3 rotationRecoilAmount)
        {
            rotationRecoil += -rotationRecoilAmount;
        }
    }
}