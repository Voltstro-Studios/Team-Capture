// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

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
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    internal class CameraSettingsController : MonoBehaviour
    {
        private CinemachineVirtualCamera cameraToChange;

        private void Start()
        {
            if (Game.IsHeadless)
            {
                Destroy(this);
                return;
            }

            cameraToChange = GetComponent<CinemachineVirtualCamera>();

            GameSettings.SettingsUpdated += UpdateSettings;
            UpdateSettings();
        }

        private void OnDestroy()
        {
            GameSettings.SettingsUpdated -= UpdateSettings;
        }

        private void UpdateSettings()
        {
            cameraToChange.m_Lens.FieldOfView = GameSettings.AdvSettings.CameraFOV;
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