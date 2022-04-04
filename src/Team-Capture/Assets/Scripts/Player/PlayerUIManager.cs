// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Helper.Extensions;
using Team_Capture.UI;
using Team_Capture.UI.Chat;
using UnityEngine;

namespace Team_Capture.Player
{
    /// <summary>
    ///     Provides an easy way of calling methods for related functions in the <see cref="ClientUI" />
    ///     <para>This is only created on the client side</para>
    /// </summary>
    [DefaultExecutionOrder(1100)]
    internal sealed class PlayerUIManager : MonoBehaviour
    {
        private ClientUI ui;

        /// <summary>
        ///     Controls for the hud's ammo
        /// </summary>
        internal HudAmmoControls HudAmmoControls => ui.hud.HudAmmoControls;

        /// <summary>
        ///     Sets up <see cref="PlayerUIManager" />
        /// </summary>
        /// <param name="clientUI"></param>
        internal void Setup(ClientUI clientUI)
        {
            ui = clientUI;
            this.GetComponentOrThrow<PlayerManager>().PlayerDamaged += OnPlayerDamaged;
        }

        /// <summary>
        ///     Toggles the pause menu
        /// </summary>
        internal void TogglePauseMenu()
        {
            if (ui.pauseMenu.GetActivePanel().Key == null)
                ui.TogglePauseMenu();
        }

        /// <summary>
        ///     Opens or closes the pause menu
        /// </summary>
        /// <param name="active"></param>
        internal void SetPauseMenu(bool active)
        {
            ui.ActivatePauseMenu(active);
        }

        /// <summary>
        ///     Toggles the scoreboard
        /// </summary>
        internal void ToggleScoreboard()
        {
            ui.ToggleScoreBoard();
        }

        /// <summary>
        ///     Set the <see cref="Hud" />'s <see cref="GameObject" /> active state
        /// </summary>
        /// <param name="active"></param>
        internal void SetHud(bool active)
        {
            ui.ActivateHud(active);
        }

        /// <summary>
        ///     Adds a killfeed item
        /// </summary>
        /// <param name="message"></param>
        internal void AddKillfeedItem(PlayerDiedMessage message)
        {
            ui.killFeed.AddKillfeedItem(message);
        }

        /// <summary>
        ///     Sets the death screen
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="state"></param>
        internal void SetDeathScreen(PlayerManager killer, bool state)
        {
            ui.ActivateDeathScreen(killer, state);
        }

        /// <summary>
        ///     Triggers the UI to update the player's health UI
        /// </summary>
        private void OnPlayerDamaged()
        {
            ui.hud.UpdateHealthUI();
        }

        #region Chat

        /// <summary>
        ///     Is the chat opened?
        /// </summary>
        internal bool IsChatOpen => ui.chat.IsChatOpen;

        /// <summary>
        ///     Adds a message to the chat
        /// </summary>
        /// <param name="message"></param>
        internal void AddChatMessage(ChatMessage message)
        {
            ui.chat.AddMessage(message);
        }

        /// <summary>
        ///     Submits the chat input
        /// </summary>
        internal void SubmitChatMessage()
        {
            ui.chat.Submit();
        }

        /// <summary>
        ///     Toggles the chat
        /// </summary>
        internal void ToggleChat()
        {
            ui.chat.ToggleChat();
        }

        #endregion
    }
}