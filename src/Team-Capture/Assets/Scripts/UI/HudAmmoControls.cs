// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using TMPro;
using UnityEngine;

namespace Team_Capture.UI
{
    [Serializable]
    public struct HudAmmoControls
    {
        /// <summary>
        ///     The ammo text
        /// </summary>
        [Tooltip("The ammo text")] 
        public TextMeshProUGUI ammoText;

        /// <summary>
        ///     The max ammo text
        /// </summary>
        [Tooltip("The max ammo text")] 
        public TextMeshProUGUI maxAmmoText;

        /// <summary>
        ///     The reload text
        /// </summary>
        [Tooltip("The reload text")] 
        public GameObject reloadTextGameObject;
    }
}