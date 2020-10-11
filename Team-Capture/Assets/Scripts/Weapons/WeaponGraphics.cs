using System;
using Console;
using UnityEngine;

namespace Weapons
{
	public class WeaponGraphics : MonoBehaviour
	{
		[ConVar("cl_muzzleflashlighting", "Whether or not the muzzle flash will have lighting", nameof(InvokeLightingChange))]
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
			ChangeLighting();
		}

		public void OnDisable()
		{
			LightingChange -= ChangeLighting;
		}

		public ParticleSystem muzzleFlash;
		public Transform bulletTracerPosition;
	}
}