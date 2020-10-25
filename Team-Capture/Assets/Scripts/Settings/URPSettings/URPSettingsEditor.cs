using System;
using UnityEngine.Rendering.Universal;

namespace Settings.URPSettings
{
	/// <summary>
	/// An editor for URP
	/// </summary>
	internal class URPSettingsEditor
	{
		private readonly UniversalRenderPipelineAsset urpPipelineAsset;

		/// <summary>
		/// Allows editing of a URP assets, but it is easier
		/// </summary>
		public URPSettingsEditor(UniversalRenderPipelineAsset pipelineAsset)
		{
			urpPipelineAsset = pipelineAsset;
		}

		#region Quality

		/// <summary>
		/// Enable or disable HDR
		/// </summary>
		/// <param name="active"></param>
		public void SetHDR(bool active)
		{
			urpPipelineAsset.supportsHDR = active;
		}

		/// <summary>
		/// Set what MSAA quality to use
		/// </summary>
		/// <param name="msaaSampleCount"></param>
		public void SetMsaaQuality(MsaaQuality msaaSampleCount)
		{
			urpPipelineAsset.msaaSampleCount = (int) msaaSampleCount;
		}

		/// <summary>
		/// Set the render scale
		/// </summary>
		/// <param name="renderScale"></param>
		public void SetRenderScale(float renderScale)
		{
			if(renderScale < 0)
				throw new ArgumentOutOfRangeException(nameof(renderScale), "The render scale cannot be smaller then 0!");
			if(renderScale > 2)
				throw new ArgumentOutOfRangeException(nameof(renderScale), "The render scale cannot be greater then 2!");

			urpPipelineAsset.renderScale = renderScale;
		}

		#endregion

		#region Shadows

		/// <summary>
		/// Sets how far the shadows should draw
		/// </summary>
		/// <param name="distance"></param>
		public void SetShadowDistance(float distance)
		{
			urpPipelineAsset.shadowDistance = distance;
		}

		/// <summary>
		/// Set what shadow cascade option to use
		/// </summary>
		/// <param name="cascadesOption"></param>
		public void SetShadowCascades(ShadowCascadesOption cascadesOption)
		{
			urpPipelineAsset.shadowCascadeOption = cascadesOption;
		}

		#endregion
	}
}