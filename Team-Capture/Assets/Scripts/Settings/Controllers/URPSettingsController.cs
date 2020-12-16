using Console;
using Core;
using Settings.URPSettings;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Logger = Core.Logging.Logger;

namespace Settings.Controllers
{
	/// <summary>
	///     Handles applying settings to Universal Render Pipeline(URP)
	/// </summary>
	internal static class URPSettingsController
	{
		private static UniversalRenderPipelineAsset urpRenderPipeline;

		private static URPSettingsEditor editor;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Load()
		{
			urpRenderPipeline = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;

			editor = new URPSettingsEditor(urpRenderPipeline);
			GameSettings.SettingsUpdated += ApplyURPSettings;
			ApplyURPSettings();
		}

		private static void ApplyURPSettings()
		{
			if (Game.IsHeadless) return;

			editor.SetHDR(GameSettings.AdvSettings.HDR);
			editor.SetRenderScale(GameSettings.AdvSettings.RenderScale);
			editor.SetShadowDistance(GameSettings.AdvSettings.ShadowDistance);
			editor.SetMsaaQuality((MsaaQuality) GameSettings.AdvSettings.MsaaQuality);
			editor.SetShadowCascades(GameSettings.AdvSettings.ShadowCascades);

			Logger.Info("Applied URP settings.");
		}

		#region Console Commands

		[ConCommand("r_shadow_distance", "Sets the distance of the shadows", CommandRunPermission.ClientOnly, 1, 1,
			true)]
		public static void SetShadowDistanceCommand(string[] args)
		{
			if (int.TryParse(args[0], out int result))
			{
				GameSettings.AdvSettings.ShadowDistance = result;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_shadow_cascades", "Sets the shadow cascades", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void SetShadowCascadesCommand(string[] args)
		{
			if (int.TryParse(args[0], out int result))
			{
				GameSettings.AdvSettings.ShadowCascades = (ShadowCascadesCount) result;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_render_scale", "Sets the render scale", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void SetRenderScaleCommand(string[] args)
		{
			if (float.TryParse(args[0], out float result))
			{
				GameSettings.AdvSettings.RenderScale = result;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_msaa_quality", "Sets the msaa quality", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void SetMsaaQualityCommand(string[] args)
		{
			if (int.TryParse(args[0], out int result))
			{
				GameSettings.AdvSettings.MsaaQuality = (MSAAQuality) result;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		[ConCommand("r_hdr", "Sets HDR", CommandRunPermission.ClientOnly, 1, 1, true)]
		public static void SetHdrCommand(string[] args)
		{
			if (bool.TryParse(args[0], out bool result))
			{
				GameSettings.AdvSettings.HDR = result;
				GameSettings.Save();

				return;
			}

			Logger.Error("Invalid input!");
		}

		#endregion
	}
}