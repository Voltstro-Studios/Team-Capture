// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using TMPro;
using UnityEngine;

namespace Team_Capture.UI
{
    /// <summary>
    ///     Hud, or 'Heads Up Display', it displays your health and ammo as well as other stuff
    /// </summary>
    internal class Hud : MonoBehaviour
    {
        /// <summary>
        ///     Controls for ammo
        /// </summary>
        [Tooltip("Controls for ammo")] [SerializeField]
        private HudAmmoControls hudAmmoControls;

        /// <summary>
        ///     The health text
        /// </summary>
        [Tooltip("The health text")] [SerializeField]
        private TextMeshProUGUI healthText;

        private ClientUI clientUI;

        /// <summary>
        ///     Controls for ammo
        /// </summary>
        internal HudAmmoControls HudAmmoControls => hudAmmoControls;

        /// <summary>
        ///     Sets up the <see cref="Hud" />
        /// </summary>
        /// <param name="ui"></param>
        internal void Setup(ClientUI ui)
        {
            clientUI = ui;
        }

        /// <summary>
        ///     Updates the health text
        /// </summary>
        internal void UpdateHealthUI()
        {
            healthText.text = clientUI.PlayerManager.Health.ToString();
        }
    }
}