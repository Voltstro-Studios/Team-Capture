// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Team_Capture.Console;
using Team_Capture.Settings;
using Team_Capture.Settings.SettingClasses;
using UnityEngine;
using UnityEngine.Rendering;

namespace Team_Capture.Weapons
{
    internal class WeaponGraphics : MonoBehaviour
    {
        [ConVar("cl_muzzleflashlighting", "Whether or not the muzzle flash will have lighting",
            nameof(InvokeLightingChange), true)]
        public static bool MuzzleFlashLighting = true;

        public ParticleSystem muzzleFlash;
        public Transform bulletTracerPosition;

        [SerializeField] private MeshRenderer[] meshRenderers;

        private void OnEnable()
        {
            LightingChange += ChangeLighting;
            GameSettings.SettingsUpdated += ApplySettings;
            ApplySettings();
            ChangeLighting();
        }

        private void OnDisable()
        {
            LightingChange -= ChangeLighting;
            GameSettings.SettingsUpdated -= ApplySettings;
        }

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

        private void ApplySettings()
        {
            MultiplayerSettingsClass multiplayerSettings = GameSettings.MultiplayerSettings;
            MuzzleFlashLighting = multiplayerSettings.WeaponMuzzleFlashLighting;
            ChangeLighting();
        }

        internal void DisableMeshRenderersShadows()
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}