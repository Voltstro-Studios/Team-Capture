using Attributes;
using Settings.URPSettings;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Settings.SettingClasses
{
	public sealed class AdvSettingsClass : Setting
	{
		[SettingsMenuFormat("MSAA")] public MSAAQuality MsaaQuality = MSAAQuality.X4;
		
		[SettingsMenuFormat("Post-Processing")] public bool PostProcessing = true;
		public bool HDR = true;

		//Motion blur settings
		public bool MotionBlur = true;
		[SettingsDontShow] public float MotionBlurIntensity = 0.06f;
		[SettingsDontShow] public float MotionBlurClamp = 0.05f;

		//Bloom settings
		public bool Bloom = true;
		[SettingsDontShow] public float BloomThreshold = 1.17f;
		[SettingsDontShow] public float BloomIntensity = 0.02f;

		//Vignette settings
		public bool Vignette = true;
		[SettingsDontShow] public float VignetteIntensity = 0.39f;
		[SettingsDontShow] public float VignetteSmoothness = 0.2f;

		//Shadow settings
		[SettingsMenuFormat("Shadow Distance")] [Range(10, 100)] public float ShadowDistance = 45;
		[SettingsMenuFormat("Cascades")] public ShadowCascadesOption ShadowCascades = ShadowCascadesOption.FourCascades;

		//Camera Settings
		[SettingsMenuFormat("FOV")] [Range(50, 100)] public int CameraFOV = 90;
		[SettingsMenuFormat("Antialiasing")] public AntialiasingMode CameraAntialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
		[SettingsDontShow] public AntialiasingQuality CameraAntialiasingQuality = AntialiasingQuality.High;
	}
}