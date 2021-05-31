// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Console;
using Team_Capture.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Settings.Controllers
{
	/// <summary>
	///     Handles controlling the <see cref="Camera" />'s settings.
	/// </summary>
	[RequireComponent(typeof(UniversalAdditionalCameraData))]
	[RequireComponent(typeof(Camera))]
	internal class CameraSettingsController : MonoBehaviour
	{
		[SerializeField] private bool allowModifyOfFOV = true;
		private UniversalAdditionalCameraData cameraData;
		private Camera cameraToChange;

		private void Start()
		{
			cameraToChange = GetComponent<Camera>();
			cameraData = GetComponent<UniversalAdditionalCameraData>();

			GameSettings.SettingsUpdated += UpdateSettings;

			UpdateSettings();
		}

		private void OnDestroy()
		{
			GameSettings.SettingsUpdated -= UpdateSettings;
		}

		private void UpdateSettings()
		{
			if (Game.IsHeadless) return;

			if (allowModifyOfFOV)
				cameraToChange.fieldOfView = GameSettings.AdvSettings.CameraFOV;

			cameraData.renderPostProcessing = GameSettings.AdvSettings.PostProcessing;
			cameraData.antialiasing = GameSettings.AdvSettings.CameraAntialiasing;
			cameraData.antialiasingQuality = GameSettings.AdvSettings.CameraAntialiasingQuality;
		}

		#region Console Commands

		[ConCommand("r_antialiasing", "Changes the antialiasing mode", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void AntialiasingModeCommand(string[] args)
		{
			if (int.TryParse(args[0], out int modeIndex))
			{
				AntialiasingMode antialiasingMode = (AntialiasingMode) modeIndex;

				GameSettings.AdvSettings.CameraAntialiasing = antialiasingMode;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_antialiasing_quality", "Changes the antialiasing quality", CommandRunPermission.ClientOnly, 1, 1,
			true)]
		public static void AntialiasingQualityCommand(string[] args)
		{
			if (int.TryParse(args[0], out int qualityIndex))
			{
				AntialiasingQuality antialiasingQuality = (AntialiasingQuality) qualityIndex;

				GameSettings.AdvSettings.CameraAntialiasingQuality = antialiasingQuality;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
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

		#endregion
	}
}