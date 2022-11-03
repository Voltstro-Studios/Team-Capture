// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Cinemachine;
using Team_Capture.Helper.Extensions;
using Team_Capture.Input;
using Team_Capture.Settings;
using Team_Capture.Settings.SettingClasses;
using UnityEngine;

namespace Team_Capture.Player
{
    [RequireComponent(typeof(CinemachineFreeLook))]
    internal class PlayerDeathCam : MonoBehaviour, AxisState.IInputAxisProvider
    {
        [SerializeField] private float xMouseSensitivity = 100f;
        [SerializeField] private float yMouseSensitivity = 100f;
        [SerializeField] private bool reverseMouse;
        
        [SerializeField]
        private CinemachineFreeLook.Orbit[] orbitsLookAround =
        {
            new(5f, 7f),
            new(0f, 7f),
            new(-5f, 7f)
        };

        private PlayerManager playerManager;
        private PlayerManager ourPlayer;

        private CinemachineCollider cameraCollider;
        private CinemachineFreeLook virtualCamera;

        private bool lookAround;

        internal void Setup(PlayerManager localPlayer)
        {
            ourPlayer = localPlayer;
        }

        private void Awake()
        {
            cameraCollider = this.GetComponentOrThrow<CinemachineCollider>();
            cameraCollider.enabled = false;
            
            virtualCamera = this.GetComponentOrThrow<CinemachineFreeLook>();
            virtualCamera.m_Orbits = orbitsLookAround;
            
            GameSettings.SettingsUpdated += UpdateSettings;
            UpdateSettings();
        }

        private void OnEnable()
        {
            InputReader.EnablePlayerDeathCamInput();
        }

        private void OnDisable()
        {
            InputReader.DisablePlayerDeathCamInput();
            StopTrackingPlayer();
        }

        internal void StartTrackingPlayer(PlayerManager playerToTrack)
        {
            playerManager = playerToTrack;
            virtualCamera.m_LookAt = playerManager.transform;
            
            //Its our player, so epic spinnny camera
            if (playerToTrack == ourPlayer)
            {
                virtualCamera.m_Follow = playerManager.transform;
                cameraCollider.enabled = true;
                
                lookAround = true;
            }
        }

        internal void StopTrackingPlayer()
        {
            cameraCollider.enabled = false;
            playerManager = null;
            virtualCamera.m_LookAt = null;
            virtualCamera.m_Follow = null;
            lookAround = false;
        }
        
        private void UpdateSettings()
        {
            MouseSettingsClass mouseSettings = GameSettings.MouseSettings;
            xMouseSensitivity = mouseSettings.MouseSensitivity;
            yMouseSensitivity = mouseSettings.MouseSensitivity;
            reverseMouse = mouseSettings.ReverseMouse;
        }

        public float GetAxisValue(int axis)
        {
            if (!lookAround)
                return 0;
            
            Vector2 look = InputReader.ReadPlayerDeathCamLook() * Time.fixedDeltaTime;
            switch (axis)
            {
                case 0: return reverseMouse ? look.y * yMouseSensitivity : look.x * xMouseSensitivity;
                case 1: return reverseMouse ? look.x * xMouseSensitivity : look.y * yMouseSensitivity;
            }

            return 0;
        }
    }
}