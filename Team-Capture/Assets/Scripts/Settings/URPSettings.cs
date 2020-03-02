using Settings.SettingClasses;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Logger = Core.Logger.Logger;

namespace Settings
{
	public class URPSettings
	{
		private static UniversalRenderPipelineAsset urpRenderPipeline;

		private static GraphicSettingsEditor editor;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType
			.AfterSceneLoad)]
		public static void Load()
		{
			urpRenderPipeline = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;

			editor = new GraphicSettingsEditor(urpRenderPipeline);
			GameSettings.SettingsLoaded += ApplyURPSettings;
		}

		private static void ApplyURPSettings()
		{
			editor.SetHdr(GameSettings.URPSettings.Hdr);

			Logger.Log("Applied URP settings.");
		}

	}
}
