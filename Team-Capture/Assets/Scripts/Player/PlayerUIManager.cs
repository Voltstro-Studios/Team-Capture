// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.UI;
using Team_Capture.UI.Chat;
using Team_Capture.Weapons;
using UnityEngine;

namespace Team_Capture.Player
{
	/// <summary>
	///     Provides an easy way of calling methods for related functions in the <see cref="ClientUI" />
	///     <para>This is only created on the client side</para>
	/// </summary>
	internal sealed class PlayerUIManager : MonoBehaviour
	{
		private ClientUI ui;

		/// <summary>
		///     Sets up <see cref="PlayerUIManager" />
		/// </summary>
		/// <param name="clientUI"></param>
		public void Setup(ClientUI clientUI)
		{
			ui = clientUI;
			GetComponent<PlayerManager>().PlayerDamaged += OnPlayerDamaged;
			GetComponent<WeaponManager>().WeaponUpdated += OnWeaponUpdated;
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

		internal void SetDeathScreen(PlayerManager killer, bool state)
		{
			ui.ActivateDeathScreen(killer, state);
		}

		#region Chat

		/// <summary>
		///		Is the chat opened
		/// </summary>
		internal bool IsChatOpen => ui.chat.IsChatOpen;

		/// <summary>
		///		Adds a message to the chat
		/// </summary>
		/// <param name="message"></param>
		internal void AddChatMessage(ChatMessage message)
		{
			ui.chat.AddMessage(message);
		}

		/// <summary>
		///		Submits the chat input
		/// </summary>
		internal void SubmitChatMessage()
		{
			ui.chat.Submit();
		}

		/// <summary>
		///		Toggles the chat
		/// </summary>
		internal void ToggleChat()
		{
			ui.chat.ToggleChat();
		}

		#endregion

		private void OnPlayerDamaged()
		{
			ui.hud.UpdateHealthUI();
		}

		private void OnWeaponUpdated(NetworkedWeapon weapon)
		{
			ui.hud.UpdateAmmoUI(weapon);
		}
	}
}