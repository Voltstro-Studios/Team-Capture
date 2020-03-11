using UnityEngine;

namespace Settings
{
	[RequireComponent(typeof(Camera))]
	public class InGameCameraSettings : MonoBehaviour
	{
		private Camera cameraToChange;

		private void Start()
		{
			cameraToChange = GetComponent<Camera>();
			GameSettings.SettingsLoaded += UpdateSettings;

			UpdateSettings();
		}

		private void OnDestroy()
		{
			GameSettings.SettingsLoaded -= UpdateSettings;
		}

		private void UpdateSettings()
		{
			cameraToChange.fieldOfView = GameSettings.AdvSettings.FOV;
		}
	}
}
