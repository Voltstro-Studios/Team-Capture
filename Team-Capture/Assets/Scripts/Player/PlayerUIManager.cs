using Core.Networking.Messages;
using UI;
using UnityEngine;
using Weapons;

namespace Player
{
	public sealed class PlayerUIManager : MonoBehaviour
	{
		private ClientUI ui;

		public void Setup(ClientUI clientUI)
		{
			ui = clientUI;
			GetComponent<PlayerManager>().PlayerDamaged += OnPlayerDamaged;
			GetComponent<WeaponManager>().WeaponUpdated += OnWeaponUpdated;
		}

		/// <summary>
		/// Toggles the pause menu
		/// </summary>
		internal void TogglePauseMenu()
		{
			if(ui.pauseMenu.GetActivePanel() == null)
				ui.TogglePauseMenu();
		}

		/// <summary>
		/// Opens or closes the pause menu
		/// </summary>
		/// <param name="active"></param>
		internal void SetPauseMenu(bool active)
		{
			ui.ActivatePauseMenu(active);
		}

		/// <summary>
		/// Toggles the scoreboard
		/// </summary>
		internal void ToggleScoreboard()
		{
			ui.ToggleScoreBoard();
		}

		/// <summary>
		/// Set the <see cref="Hud"/>'s <see cref="GameObject"/> active state
		/// </summary>
		/// <param name="active"></param>
		internal void SetHud(bool active)
		{
			ui.hud.gameObject.SetActive(active);
		}

		/// <summary>
		/// Adds a killfeed item
		/// </summary>
		/// <param name="message"></param>
		internal void AddKillfeedItem(PlayerDiedMessage message)
		{
			ui.killFeed.AddFeedBackItem(message);
		}

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