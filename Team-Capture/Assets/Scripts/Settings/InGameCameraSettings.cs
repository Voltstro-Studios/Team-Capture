using UnityEngine;
using UnityEngine.Rendering.Universal;

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
			if(allowModifyOfFOV)
				cameraToChange.fieldOfView = GameSettings.AdvSettings.CameraFOV;

			cameraData.renderPostProcessing = GameSettings.AdvSettings.PostProcessing;
			cameraData.antialiasing = GameSettings.AdvSettings.CameraAntialiasing;
			cameraData.antialiasingQuality = GameSettings.AdvSettings.CameraAntialiasingQuality;
		}
	}
}
