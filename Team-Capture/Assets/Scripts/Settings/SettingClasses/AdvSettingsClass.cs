using UnityEngine.Rendering.Universal;

namespace Settings.SettingClasses
{
	public sealed class AdvSettingsClass : Setting
	{
		public MsaaQuality MsaaQuality = MsaaQuality._8x;

		public float ShadowDistance = 45;
		public ShadowCascadesOption ShadowCascades = ShadowCascadesOption.FourCascades;

		public bool HDR = true;
		public bool PostProcessing = true;
		public bool MotionBlur = true;
		public bool Bloom = true;
	}
}
