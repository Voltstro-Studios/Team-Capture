using Console;
using Core;
using Settings.SettingClasses;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Settings.Controllers
{
	/// <summary>
	/// Handles controlling Unity's quality settings
	/// </summary>
	public static class QualitySettingsController
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Load()
		{
			GameSettings.SettingsUpdated += ApplySettings;
			ApplySettings();
		}

		private static void ApplySettings()
		{
			if(Game.IsHeadless) return;

			VideoSettingsClass settings = GameSettings.VideoSettings;
			Screen.SetResolution(settings.Resolution.width, settings.Resolution.height, settings.ScreenMode, settings.Resolution.refreshRate);
			QualitySettings.masterTextureLimit = (int) settings.TextureQuality;
			QualitySettings.vSyncCount = (int) settings.VSync;

			Logger.Debug("Applied Video settings");
		}

		#region Video Console Commands

		[ConCommand("r_resolution", "Set the resolution (width x height)", CommandRunPermission.ClientOnly,2, 2, true)]
		public static void SetResolutionCommand(string[] args)
		{
			if (int.TryParse(args[0], out int widthRes))
			{
				if (int.TryParse(args[1], out int heightRes))
				{
					GameSettings.VideoSettings.Resolution.width = widthRes;
					GameSettings.VideoSettings.Resolution.height = heightRes;

					GameSettings.Save();

					return;
				}
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_refreshrate", "Sets the refresh rate", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void SetRefreshRateCommand(string[] args)
		{
			if (int.TryParse(args[0], out int refreshRate))
			{
				GameSettings.VideoSettings.Resolution.refreshRate = refreshRate;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_screenmode", "Sets the screen mode", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void SetScreenModeCommand(string[] args)
		{
			if (int.TryParse(args[0], out int screenModeIndex))
			{
				FullScreenMode screenMode = (FullScreenMode) screenModeIndex;

				GameSettings.VideoSettings.ScreenMode = screenMode;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		#endregion
	}
}