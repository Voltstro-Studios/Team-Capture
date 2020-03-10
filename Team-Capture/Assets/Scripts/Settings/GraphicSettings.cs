using Attributes;
using Core.Logger;
using Settings.SettingClasses;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace Settings
{
	public class GraphicSettings
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Load()
		{
			GameSettings.SettingsLoaded += ApplySettings;
			ApplySettings();
		}

		private static void ApplySettings()
		{
			VideoSettingsClass settings = GameSettings.VideoSettings;
			Screen.SetResolution(settings.Resolution.width, settings.Resolution.height, settings.ScreenMode, settings.Resolution.refreshRate);

			Logger.Log("Applied video settings");
		}

		[ConCommand("r_resolution", "Set the resolution (width x height)", 2, 2)]
		public static void SetResolution(string[] args)
		{
			if (int.TryParse(args[0], out int widthRes))
			{
				if (int.TryParse(args[1], out int heightRes))
				{
					GameSettings.VideoSettings.Resolution = new Resolution
					{
						height = heightRes,
						width = widthRes,
						refreshRate = Screen.currentResolution.refreshRate
					};

					GameSettings.Save();

					return;
				}
			}

			Logger.Log("Invalid input!", LogVerbosity.Error);
		}

		[ConCommand("r_screenmode", "Sets the screen mode", 1, 1)]
		public static void SetScreenMode(string[] args)
		{
			if (int.TryParse(args[0], out int screenModeIndex))
			{
				FullScreenMode screenMode = (FullScreenMode) screenModeIndex;

				GameSettings.VideoSettings.ScreenMode = screenMode;
				GameSettings.Save();

				return;
			}

			Logger.Log("Invalid input!", LogVerbosity.Error);
		}
	}
}
