// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Console;
using Team_Capture.Settings;
using Team_Capture.Settings.SettingClasses;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Weapons
{
    internal class WeaponSway : MonoBehaviour
    {
        [ConVar("cl_sway_amount", "Sets how much a gun will sway", true)]
        public static float SwayAmount = 0.1f;

        [ConVar("cl_sway_enable", "Whether or not weapons will sway", true)]
        public static bool SwayEnabled = true;

        public float smooth = 3.0f;

        private float axisX;
        private float axisY;
        private Vector3 localPosition;
        
        private Vector2 maxWeaponSway = new(0.25f, 0.2f);
        private float weaponSwayAmount = 1f;

        private void Awake()
        {
            localPosition = transform.localPosition;

            GameSettings.SettingsUpdated += ApplySettings;
            ApplySettings();
        }

        private void Update()
        {
            if (!SwayEnabled)
                return;

            float fx = -axisX * SwayAmount * weaponSwayAmount;
            float fy = -axisY * (SwayAmount * weaponSwayAmount - 0.05f);

            fx = Mathf.Clamp(fx, -maxWeaponSway.x, maxWeaponSway.x);
            fy = Mathf.Clamp(fy, -maxWeaponSway.y, maxWeaponSway.y);

            Vector3 detection = new(localPosition.x + fx, localPosition.y + fy, localPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition, detection, Time.deltaTime * smooth);
        }

        private void OnDestroy()
        {
            GameSettings.SettingsUpdated -= ApplySettings;
        }

        public void SetInput(float x, float y)
        {
            axisX = x;
            axisY = y;
        }

        public void SetWeapon(WeaponBase weapon)
        {
            //Reset
            transform.localPosition = localPosition;

            weaponSwayAmount = weapon.weaponSwayAmountMultiplier;
            maxWeaponSway = weapon.weaponSwayMax;
        }

        private void ApplySettings()
        {
            MultiplayerSettingsClass multiplayerSettings = GameSettings.MultiplayerSettings;
            SwayEnabled = multiplayerSettings.WeaponSwayEnabled;
            SwayAmount = multiplayerSettings.WeaponSwayAmount;
        }
    }
}