using Team_Capture.Settings.URPSettings;
using Team_Capture.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Team_Capture.Settings.SettingClasses
{
	internal sealed class AdvSettingsClass : Setting
	{
		[SettingsPropertyDisplayText("Settings_AdvMSAA")] 
		public MSAAQuality MsaaQuality = MSAAQuality.X4;
		
		[SettingsPropertyDisplayText("Settings_AdvPostProcessing")] 
		public bool PostProcessing = true;

		[SettingsPropertyDisplayText("Settings_AdvHDR")]
		public bool HDR = true;

		//Motion blur settings
		[SettingsPropertyDisplayText("Settings_AdvMotionBlur")]
		public bool MotionBlur = true;
		[SettingsDontShow] public float MotionBlurIntensity = 0.06f;
		[SettingsDontShow] public float MotionBlurClamp = 0.05f;

		//Bloom settings
		[SettingsPropertyDisplayText("Settings_AdvBloom")]
		public bool Bloom = true;
		[SettingsDontShow] public float BloomThreshold = 1.17f;
		[SettingsDontShow] public float BloomIntensity = 0.02f;

		//Vignette settings
		[SettingsPropertyDisplayText("Settings_AdvVignette")]
		public bool Vignette = true;
		[SettingsDontShow] public float VignetteIntensity = 0.39f;
		[SettingsDontShow] public float VignetteSmoothness = 0.2f;

		[SettingsPropertyDisplayText("Settings_AdvRenderScale")]
		[Range(0, 2)]
		public float RenderScale = 1.0f;

		//Shadow settings
		[SettingsPropertyDisplayText("Settings_AdvShadowDistance")] 
		[Range(10, 100)] public int ShadowDistance = 45;

		[SettingsPropertyDisplayText("Settings_AdvCascades")] 
		public ShadowCascadesCount ShadowCascades = ShadowCascadesCount.FourCascades;

		//Camera Settings
		[SettingsPropertyDisplayText("Settings_AdvFOV")] 
		[Range(50, 100)] public int CameraFOV = 90;

		[SettingsPropertyDisplayText("Settings_AdvAntialiasing")] 
		public AntialiasingMode CameraAntialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

		[SettingsDontShow] public AntialiasingQuality CameraAntialiasingQuality = AntialiasingQuality.High;
	}
}