// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Console;
using Team_Capture.Core;
using Team_Capture.Settings.SettingClasses;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Settings.Controllers
{
	/// <summary>
	///     Controller for Unity's <see cref="Volume" /> for URP
	/// </summary>
	[RequireComponent(typeof(Volume))]
	internal class VolumeSettingsController : SingletonMonoBehaviour<VolumeSettingsController>
	{
		/// <summary>
		///     The active <see cref="Volume" />
		/// </summary>
		public static Volume ActiveVolume;

		protected override void SingletonAwakened()
		{
			if (Game.IsHeadless || Game.IsGameQuitting)
			{
				Destroy(gameObject);
				return;
			}

			ActiveVolume = GetComponent<Volume>();

			GameSettings.SettingsUpdated += ApplyVolumeSettings;
			ApplyVolumeSettings();
		}

		protected override void SingletonStarted()
		{
		}

		protected override void SingletonDestroyed()
		{
		}

		protected override void NotifyInstanceRepeated()
		{
			Destroy(gameObject);
		}

		private void ApplyVolumeSettings()
		{
			//Get the advance settings
			AdvSettingsClass settings = GameSettings.AdvSettings;
			ActiveVolume.enabled = settings.PostProcessing;

			//No point in apply anything if we aren't using Post-Processing
			if (!settings.PostProcessing)
				return;

			//Motion Blur
			if (ActiveVolume.profile.TryGet(out MotionBlur blur))
			{
				blur.active = settings.MotionBlur;
				blur.intensity.value = settings.MotionBlurIntensity;
				blur.clamp.value = settings.MotionBlurClamp;
			}

			//Bloom
			if (ActiveVolume.profile.TryGet(out Bloom bloom))
			{
				bloom.active = settings.Bloom;
				bloom.threshold.value = settings.BloomThreshold;
				bloom.intensity.value = settings.BloomIntensity;
			}

			//Vignette
			if (ActiveVolume.profile.TryGet(out Vignette vignette))
			{
				vignette.active = settings.Vignette;
				vignette.intensity.value = settings.VignetteIntensity;
				vignette.smoothness.value = settings.VignetteSmoothness;
			}

			Logger.Debug("Applied Volume(Post-Processing) settings");
		}

		#region Console Command

		#region Motion Blur

		[ConCommand("r_motionblur_enabled", "Enables or disables motion blur", CommandRunPermission.ClientOnly, 1, 1,
			true)]
		public static void MotionBlurEnableCommand(string[] args)
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
					Logger.Error("Invalid argument!");
					break;
			}
		}

		[ConCommand("r_motionblur_intensity", "Changes the motion blur intensity", CommandRunPermission.ClientOnly, 1,
			1, true)]
		public static void MotionBlurIntensityCommand(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if (float.TryParse(stringAmount, out float amount))
			{
				//Motion blur intensity doesn't go over 1.0
				if (amount > 1.0f)
					amount = 1.0f;

				GameSettings.AdvSettings.MotionBlurIntensity = amount;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_motionblur_clamp", "Changes the motion blur clamp", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void MotionBlurClampCommand(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if (float.TryParse(stringAmount, out float amount))
			{
				//Motion blur clamp doesn't go over 0.2
				if (amount > 0.2f)
					amount = 0.2f;

				GameSettings.AdvSettings.MotionBlurClamp = amount;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		#endregion

		#region Bloom

		[ConCommand("r_bloom_enabled", "Enables or disables bloom", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void BloomEnableCommand(string[] args)
		{
			string toggle = args[0].ToLower();

			switch (toggle)
			{
				case "1":
				case "true":
					GameSettings.AdvSettings.Bloom = true;
					GameSettings.Save();
					break;
				case "0":
				case "false":
					GameSettings.AdvSettings.Bloom = false;
					GameSettings.Save();
					break;
				default:
					Logger.Error("Invalid argument!");
					break;
			}
		}

		[ConCommand("r_bloom_threshold", "Changes the bloom threshold", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void BloomThresholdCommand(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if (float.TryParse(stringAmount, out float amount))
			{
				GameSettings.AdvSettings.BloomThreshold = amount;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_bloom_intensity", "Changes the bloom intensity", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void BloomIntensityCommand(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if (float.TryParse(stringAmount, out float amount))
			{
				GameSettings.AdvSettings.BloomIntensity = amount;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		#endregion

		#region Vignette

		[ConCommand("r_vignette_enabled", "Enables or disables vignette", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void VignetteEnableCommand(string[] args)
		{
			string toggle = args[0].ToLower();

			switch (toggle)
			{
				case "1":
				case "true":
					GameSettings.AdvSettings.Vignette = true;
					GameSettings.Save();
					break;
				case "0":
				case "false":
					GameSettings.AdvSettings.Vignette = false;
					GameSettings.Save();
					break;
				default:
					Logger.Error("Invalid argument!");
					break;
			}
		}

		[ConCommand("r_vignette_intensity", "Changes the vignette intensity", CommandRunPermission.ClientOnly, 1, 1,
			true)]
		public static void VignetteIntensityCommand(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if (float.TryParse(stringAmount, out float amount))
			{
				//Vignette intensity doesn't go over 1.0
				if (amount > 1.0f)
					amount = 1.0f;

				GameSettings.AdvSettings.VignetteIntensity = amount;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_vignette_smoothness", "Changes the vignette smoothness", CommandRunPermission.ClientOnly, 1, 1,
			true)]
		public static void VignetteSmoothnessCommand(string[] args)
		{
			string stringAmount = args[0].ToLower();

			if (float.TryParse(stringAmount, out float amount))
			{
				//Vignette smoothness doesn't go over 1.0
				if (amount > 1.0f)
					amount = 1.0f;

				GameSettings.AdvSettings.VignetteSmoothness = amount;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		#endregion

		#endregion
	}
}