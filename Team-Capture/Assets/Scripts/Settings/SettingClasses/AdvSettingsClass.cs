using Team_Capture.Settings.URPSettings;
using Team_Capture.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Team_Capture.Settings.SettingClasses
{
	internal sealed class AdvSettingsClass : Setting
	{
		[SettingsPropertyFormatName("Settings_AdvMSAA")] 
		public MSAAQuality MsaaQuality = MSAAQuality.X4;
		
		[SettingsPropertyFormatName("Settings_AdvPostProcessing")] 
		public bool PostProcessing = true;

		[SettingsPropertyFormatName("Settings_AdvHDR")]
		public bool HDR = true;

		//Motion blur settings
		[SettingsPropertyFormatName("Settings_AdvMotionBlur")]
		public bool MotionBlur = true;
		[SettingsDontShow] public float MotionBlurIntensity = 0.06f;
		[SettingsDontShow] public float MotionBlurClamp = 0.05f;

		//Bloom settings
		[SettingsPropertyFormatName("Settings_AdvBloom")]
		public bool Bloom = true;
		[SettingsDontShow] public float BloomThreshold = 1.17f;
		[SettingsDontShow] public float BloomIntensity = 0.02f;

		//Vignette settings
		[SettingsPropertyFormatName("Settings_AdvVignette")]
		public bool Vignette = true;
		[SettingsDontShow] public float VignetteIntensity = 0.39f;
		[SettingsDontShow] public float VignetteSmoothness = 0.2f;

		[SettingsPropertyFormatName("Settings_AdvRenderScale")]
		[Range(0, 2)]
		public float RenderScale = 1.0f;

		//Shadow settings
		[SettingsPropertyFormatName("Settings_AdvShadowDistance")] 
		[Range(10, 100)] public int ShadowDistance = 45;

		[SettingsPropertyFormatName("Settings_AdvCascades")] 
		public ShadowCascadesCount ShadowCascades = ShadowCascadesCount.FourCascades;

		//Camera Settings
		[SettingsPropertyFormatName("Settings_AdvFOV")] 
		[Range(50, 100)] public int CameraFOV = 90;

		[SettingsPropertyFormatName("Settings_AdvAntialiasing")] 
		public AntialiasingMode CameraAntialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

		[SettingsDontShow] public AntialiasingQuality CameraAntialiasingQuality = AntialiasingQuality.High;
	}
}