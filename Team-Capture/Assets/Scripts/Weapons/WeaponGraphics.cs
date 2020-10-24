using System;
using Console;
using Settings;
using Settings.SettingClasses;
using UnityEngine;

namespace Weapons
{
	internal class WeaponGraphics : MonoBehaviour
	{
		[ConVar("cl_muzzleflashlighting", "Whether or not the muzzle flash will have lighting", nameof(InvokeLightingChange), true)]
		public static bool MuzzleFlashLighting = true;

		public static event Action LightingChange;

		public static void InvokeLightingChange()
		{
			LightingChange?.Invoke();
		}

		public void ChangeLighting()
		{
			ParticleSystem.LightsModule lighting = muzzleFlash.lights;
			lighting.enabled = MuzzleFlashLighting;
		}

		public void OnEnable()
		{
			LightingChange += ChangeLighting;
			GameSettings.SettingsUpdated += ApplySettings;
			ApplySettings();
			ChangeLighting();
		}

		public void OnDisable()
		{
			LightingChange -= ChangeLighting;
			GameSettings.SettingsUpdated -= ApplySettings;
		}

		private void ApplySettings()
		{
			MultiplayerSettingsClass multiplayerSettings = GameSettings.MultiplayerSettings;
			MuzzleFlashLighting = multiplayerSettings.WeaponMuzzleFlashLighting;
			ChangeLighting();
		}

		public ParticleSystem muzzleFlash;
		public Transform bulletTracerPosition;
	}
}