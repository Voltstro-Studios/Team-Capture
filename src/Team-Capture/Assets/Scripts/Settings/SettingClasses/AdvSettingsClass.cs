// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Settings.URPSettings;
using Team_Capture.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Team_Capture.Settings.SettingClasses
{
	internal sealed class AdvSettingsClass : Setting
	{
		[SettingsPropertyDisplayText("Text", "Settings_Adv_MSAA")] 
		public MSAAQuality MsaaQuality = MSAAQuality.X4;
		
		[SettingsPropertyDisplayText("Text", "Settings_Adv_PostProcessing")] 
		public bool PostProcessing = true;

		[SettingsPropertyDisplayText("Text", "Settings_Adv_HDR")]
		public bool HDR = true;

		//Motion blur settings
		[SettingsPropertyDisplayText("Text", "Settings_Adv_MotionBlur")]
		public bool MotionBlur = true;
		[SettingsDontShow] public float MotionBlurIntensity = 0.06f;
		[SettingsDontShow] public float MotionBlurClamp = 0.05f;

		//Bloom settings
		[SettingsPropertyDisplayText("Text", "Settings_Adv_Bloom")]
		public bool Bloom = true;
		[SettingsDontShow] public float BloomThreshold = 1.17f;
		[SettingsDontShow] public float BloomIntensity = 0.02f;

		//Vignette settings
		[SettingsPropertyDisplayText("Text", "Settings_Adv_Vignette")]
		public bool Vignette = true;
		[SettingsDontShow] public float VignetteIntensity = 0.39f;
		[SettingsDontShow] public float VignetteSmoothness = 0.2f;

		[SettingsPropertyDisplayText("Text", "Settings_Adv_RenderScale")]
		[Range(0, 2)]
		public float RenderScale = 1.0f;

		//Shadow settings
		[SettingsPropertyDisplayText("Text", "Settings_Adv_ShadowDistance")] 
		[Range(10, 100)] public int ShadowDistance = 45;

		[SettingsPropertyDisplayText("Text", "Settings_Adv_Cascades")] 
		public ShadowCascadesCount ShadowCascades = ShadowCascadesCount.FourCascades;

		//Camera Settings
		[SettingsPropertyDisplayText("Text", "Settings_Adv_FOV")] 
		[Range(50, 100)] public int CameraFOV = 90;

		[SettingsPropertyDisplayText("Text", "Settings_Adv_Antialiasing")] 
		public AntialiasingMode CameraAntialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

		[SettingsDontShow] public AntialiasingQuality CameraAntialiasingQuality = AntialiasingQuality.High;
	}
}