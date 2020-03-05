using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Settings.SettingClasses
{
	public sealed class AdvSettingsClass : Setting
	{
		public MsaaQuality MsaaQuality = MsaaQuality._8x;
		public ShadowCascadesOption ShadowCascades = ShadowCascadesOption.FourCascades;

		public bool PostProcessing = true;
		public bool HDR = true;
		public bool MotionBlur = true;
		public bool Bloom = true;

		[Range(10, 100)] public float ShadowDistance = 45;
	}
}