using Attributes;
using Core.Logger;
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
			{
				blur.active = GameSettings.AdvSettings.MotionBlur;
				blur.intensity.value = GameSettings.AdvSettings.MotionBlurIntensity;
				blur.clamp.value = GameSettings.AdvSettings.MotionBlurClamp;
			}

			if (ActiveVolume.profile.TryGet(out Bloom bloom))
				bloom.active = GameSettings.AdvSettings.Bloom;

			Logger.Log("Applied Volume(Post-Processing) Settings");
		}

		#region Console Command

		[ConCommand("r_motionblur_enabled", "Enables or disables motion blur")]
		public static void MotionBlurEnable(string[] args)
		{
			string toggle = args[0].ToLower();

			switch (toggle)
			{
				case "1":
				case "true":
					GameSettings.AdvSettings.MotionBlur = true;
					GameSettings.Save();
					break;
				case "0":
				case "false":
					GameSettings.AdvSettings.MotionBlur = false;
					GameSettings.Save();
					break;
				default:
					Logger.Log("Invalid argument!", LogVerbosity.Error);
					break;
			}
		}

		[ConCommand("r_motionblur_intensity", "Changes the motion blur intensity")]
		public static void MotionBlurIntensity(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if(float.TryParse(stringAmount, out float amount))
			{
				//Motion blur intensity doesn't go over 1.0
				if (amount > 1.0f)
					amount = 1.0f;

				GameSettings.AdvSettings.MotionBlurIntensity = amount;
				GameSettings.Save();

				return;
			}

			Logger.Log("Invalid input!", LogVerbosity.Error);
		}

		[ConCommand("r_motionblur_clamp", "Changes the motion blur clamp")]
		public static void MotionBlurClamp(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if(float.TryParse(stringAmount, out float amount))
			{
				//Motion blur clamp doesn't go over 0.2
				if (amount > 0.2f)
					amount = 0.2f;

				GameSettings.AdvSettings.MotionBlurClamp = amount;
				GameSettings.Save();

				return;
			}

			Logger.Log("Invalid input!", LogVerbosity.Error);
		}

		#endregion
	}
}