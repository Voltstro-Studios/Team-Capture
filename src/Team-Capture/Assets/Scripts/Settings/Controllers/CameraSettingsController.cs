// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Cinemachine;
using Team_Capture.Console;
using Team_Capture.Core;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Settings.Controllers
{
    /// <summary>
    ///     Handles controlling the <see cref="Camera" />'s settings.
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCameraBase))]
    internal class CameraSettingsController : MonoBehaviour
    {
        private CinemachineVirtualCameraBase cameraToChange;

        private void Start()
        {
            if (Game.IsHeadless)
            {
                Destroy(this);
                return;
            }

            cameraToChange = GetComponent<CinemachineVirtualCameraBase>();

            GameSettings.SettingsUpdated += UpdateSettings;
            UpdateSettings();
        }

        private void OnDestroy()
        {
            GameSettings.SettingsUpdated -= UpdateSettings;
        }

        private void UpdateSettings()
        {
            //Not the most pretty solution, but it works
            switch (cameraToChange)
            {
                case CinemachineFreeLook freeLook:
                    freeLook.m_Lens.FieldOfView = GameSettings.AdvSettings.CameraFOV;
                    break;
                case CinemachineVirtualCamera virtualCamera:
                    virtualCamera.m_Lens.FieldOfView = GameSettings.AdvSettings.CameraFOV;
                    break;
                default:
                    throw new IndexOutOfRangeException(
                        "I don't support this Cinemachine camera type! Please add me to the code!");
            }
        }

        [ConCommand("cl_fov", "FOV of the camera", CommandRunPermission.ClientOnly, 1, 1, true)]
        public static void CameraFovCommand(string[] args)
        {
            if (int.TryParse(args[0], out int cameraFov))
            {
                GameSettings.AdvSettings.CameraFOV = cameraFov;
                GameSettings.Save();

                return;
            }

            Logger.Error("Invalid input!");
        }
    }
}