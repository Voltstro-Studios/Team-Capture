// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using TMPro;
using UnityEngine;

namespace Team_Capture.UI.Elements.Settings
{
    /// <summary>
    ///     A dropdown for the settings menu
    /// </summary>
    internal class SettingDropdown : MonoBehaviour
    {
        /// <summary>
        ///     The dropdown itself
        /// </summary>
        public TMP_Dropdown dropdown;

        /// <summary>
        ///     The settings name text
        /// </summary>
        public TextMeshProUGUI settingsName;
    }
}