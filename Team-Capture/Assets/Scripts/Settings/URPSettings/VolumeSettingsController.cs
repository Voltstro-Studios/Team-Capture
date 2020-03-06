using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Logger = Core.Logger.Logger;

namespace Settings.URPSettings
{
	[RequireComponent(typeof(Volume))]
	public class VolumeSettingsController : MonoBehaviour
	{
		private static VolumeSettingsController instance;

		public static Volume ActiveVolume;

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);

			ActiveVolume = GetComponent<Volume>();

			GameSettings.SettingsLoaded += ApplyVolumeSettings;
			ApplyVolumeSettings();
		}

		private void ApplyVolumeSettings()
		{
			ActiveVolume.enabled = GameSettings.AdvSettings.PostProcessing;

			if (ActiveVolume.profile.TryGet(out MotionBlur blur))
				blur.active = GameSettings.AdvSettings.MotionBlur;

			if (ActiveVolume.profile.TryGet(out Bloom bloom))
				bloom.active = GameSettings.AdvSettings.Bloom;

			Logger.Log("Applied Volume(Post-Processing) Settings");
		}

		private void Start()
		{
			Logger.Log("Volume settings are ready!");
		}
	}
}