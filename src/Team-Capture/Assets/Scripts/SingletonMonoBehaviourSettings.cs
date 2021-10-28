// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture
{
    /// <summary>
    ///     A <see cref="SingletonMonoBehaviour{T}" /> but with settings.
    /// </summary>
    public abstract class SingletonMonoBehaviourSettings<TBaseType, TSettingsType> : SingletonMonoBehaviour<TBaseType>
        where TBaseType : SingletonMonoBehaviour<TBaseType>
        where TSettingsType : ScriptableObject
    {
        /// <summary>
        ///     Loaded <see cref="TSettingsType" />
        /// </summary>
        [NonSerialized] protected TSettingsType Settings;

        /// <summary>
        ///     Addressable path to where the settings are
        /// </summary>
        protected abstract string SettingsPath { get; }

        protected override void SingletonAwakened()
        {
            if (string.IsNullOrEmpty(SettingsPath))
            {
                Logger.Error("Settings path cannot be empty or null!");
                Destroy(this);
                return;
            }

            try
            {
                Settings = Addressables.LoadAssetAsync<TSettingsType>(SettingsPath).WaitForCompletion();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load settings!");
            }

            Logger.Debug("Loaded settings from '{Path}'.", SettingsPath);
        }

        protected override void SingletonDestroyed()
        {
            Destroy(Settings);
        }
    }
}