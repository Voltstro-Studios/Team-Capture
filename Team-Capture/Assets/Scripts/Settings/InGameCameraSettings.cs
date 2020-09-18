using Attributes;
using Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Logger = Core.Logging.Logger;

namespace Settings
{
	[RequireComponent(typeof(UniversalAdditionalCameraData))]
	[RequireComponent(typeof(Camera))]
	public class InGameCameraSettings : MonoBehaviour
	{
		private Camera cameraToChange;
		private UniversalAdditionalCameraData cameraData;

		[SerializeField] private bool allowModifyOfFOV = true;

		private void Start()
		{
			cameraToChange = GetComponent<Camera>();
			cameraData = GetComponent<UniversalAdditionalCameraData>();

			GameSettings.SettingsLoaded += UpdateSettings;

			UpdateSettings();
		}

		private void OnDestroy()
		{
			GameSettings.SettingsLoaded -= UpdateSettings;
		}

		private void UpdateSettings()
		{
			if(Game.IsHeadless) return;

			if(allowModifyOfFOV)
				cameraToChange.fieldOfView = GameSettings.AdvSettings.CameraFOV;

			cameraData.renderPostProcessing = GameSettings.AdvSettings.PostProcessing;
			cameraData.antialiasing = GameSettings.AdvSettings.CameraAntialiasing;
			cameraData.antialiasingQuality = GameSettings.AdvSettings.CameraAntialiasingQuality;
		}

		#region Console Commands

		[ConCommand("r_antialiasing", "Changes the antialiasing mode", 1, 1)]
		public static void AntialiasingMode(string[] args)
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

		[ConCommand("r_antialiasing_quality", "Changes the antialiasing quality", 1, 1)]
		public static void AntialiasingQuality(string[] args)
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

		#endregion
	}
}
