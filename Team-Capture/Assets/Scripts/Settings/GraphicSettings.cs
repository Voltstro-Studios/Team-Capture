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
		}

		private static void ApplySettings()
		{
			VideoSettingsClass settings = GameSettings.VideoSettings;
			Screen.SetResolution(settings.Resolution.width, settings.Resolution.height, settings.ScreenMode, settings.Resolution.refreshRate);

			Logger.Log("Applied video settings");
		}
	}
}
