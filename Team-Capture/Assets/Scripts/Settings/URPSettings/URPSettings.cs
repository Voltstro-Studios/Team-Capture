using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Logger = Core.Logger.Logger;

namespace Settings.URPSettings
{
	public class URPSettings
	{
		private static UniversalRenderPipelineAsset urpRenderPipeline;

		private static GraphicSettingsEditor editor;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Load()
		{
			urpRenderPipeline = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;

			editor = new GraphicSettingsEditor(urpRenderPipeline);
			GameSettings.SettingsLoaded += ApplyURPSettings;
			ApplyURPSettings();
		}

		private static void ApplyURPSettings()
		{
			editor.SetHdr(GameSettings.AdvSettings.HDR);
			editor.SetShadowDistance(GameSettings.AdvSettings.ShadowDistance);
			editor.SetAntiAliasing(GameSettings.AdvSettings.MsaaQuality);
			editor.SetShadowCascades(GameSettings.AdvSettings.ShadowCascades);

			Logger.Log("Applied URP settings.");
		}
	}
}