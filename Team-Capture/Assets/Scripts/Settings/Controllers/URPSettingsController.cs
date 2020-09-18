using Core;
using Settings.URPSettings;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Logger = Core.Logging.Logger;

namespace Settings.Controllers
{
	/// <summary>
	/// Handles applying settings to Universal Render Pipeline(URP)
	/// </summary>
	public static class URPSettingsController
	{
		private static UniversalRenderPipelineAsset urpRenderPipeline;

		private static URPSettingsEditor editor;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Load()
		{
			urpRenderPipeline = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;

			editor = new URPSettingsEditor(urpRenderPipeline);
			GameSettings.SettingsLoaded += ApplyURPSettings;
			ApplyURPSettings();
		}

		private static void ApplyURPSettings()
		{
			if(Game.IsHeadless) return;

			editor.SetHDR(GameSettings.AdvSettings.HDR);
			editor.SetRenderScale(GameSettings.AdvSettings.RenderScale);
			editor.SetShadowDistance(GameSettings.AdvSettings.ShadowDistance);
			editor.SetMsaaQuality((MsaaQuality) GameSettings.AdvSettings.MsaaQuality);
			editor.SetShadowCascades(GameSettings.AdvSettings.ShadowCascades);

			Logger.Info("Applied URP settings.");
		}
	}
}