// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Cinemachine;
using Team_Capture.Helper.Extensions;
using Team_Capture.Input;
using UnityEngine;

namespace Team_Capture.Player
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    internal class PlayerDeathCam : MonoBehaviour
    {
        [SerializeField] private float lookSensitivity = 100f;
        private PlayerManager playerManager;

        private float rotationX;
        private float rotationY;

        private CinemachineVirtualCamera virtualCamera;

        private void Awake()
        {
            virtualCamera = this.GetComponentOrThrow<CinemachineVirtualCamera>();
        }

        internal void Update()
        {
            if (playerManager != null)
            {
                if (playerManager.IsDead)
                    StopTrackingPlayer();

                return;
            }

            Vector2 look = InputReader.ReadPlayerDeathCamLook() * Time.deltaTime * lookSensitivity;
            rotationX -= look.y;
            rotationY += look.x;

            rotationX = Mathf.Clamp(rotationX, -90, 90);

            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        private void OnEnable()
        {
            InputReader.EnablePlayerDeathCamInput();
        }

        private void OnDisable()
        {
            rotationX = 0;
            rotationY = 0;
            InputReader.DisablePlayerDeathCamInput();
        }

        internal void StartTrackingPlayer(PlayerManager playerToTrack)
        {
            playerManager = playerToTrack;
            virtualCamera.m_LookAt = playerManager.transform;
        }

        internal void StopTrackingPlayer()
        {
            playerManager = null;
            virtualCamera.LookAt = null;
        }
    }
}