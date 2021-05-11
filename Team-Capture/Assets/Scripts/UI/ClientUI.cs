using System;
using Team_Capture.Console;
using Team_Capture.Player;
using Team_Capture.UI.Menus;
using Team_Capture.Weapons;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI
{
	/// <summary>
	///     Controller for the client UI
	/// </summary>
	internal class ClientUI : MonoBehaviour
	{
		/// <summary>
		///     Is the pause menu open
		/// </summary>
		public static bool IsPauseMenuOpen;

		/// <summary>
		///     The hud
		/// </summary>
		[Tooltip("The hud")] public Hud hud;

		/// <summary>
		///     The killfeed
		/// </summary>
		[Tooltip("The killfeed")] public KillFeed killFeed;

		/// <summary>
		///     The pause menu
		/// </summary>
		[Tooltip("The pause menu")] public PauseMenu pauseMenu;

		/// <summary>
		///		The chat
		/// </summary>
		[Tooltip("The chat")] public Chat.Chat chat;

		/// <summary>
		///     The scoreboard gameobject
		/// </summary>
		[Tooltip("The scoreboard gameobject")] public GameObject scoreBoardObject;

		/// <summary>
		///     The <see cref="Team_Capture.Player.PlayerManager" />
		/// </summary>
		[NonSerialized] public PlayerManager PlayerManager;

		/// <summary>
		///     The <see cref="Weapons.WeaponManager" />
		/// </summary>
		[NonSerialized] public WeaponManager WeaponManager;

		/// <summary>
		///     Sets up the UI
		/// </summary>
		/// <param name="playerManager"></param>
		public void SetupUI(PlayerManager playerManager)
		{
			//Reset this
			IsPauseMenuOpen = false;

			hud.Setup(this);

			PlayerManager = playerManager;
			WeaponManager = playerManager.GetComponent<WeaponManager>();

			pauseMenu.gameObject.SetActive(false);

			scoreBoardObject.SetActive(false);
			scoreBoardObject.GetComponent<ScoreBoard.ScoreBoard>().clientPlayer = playerManager;

			pauseMenu.Setup(TogglePauseMenu);

			Logger.Debug("The ClientUI is now ready.");
		}

		/// <summary>
		///     Toggles the pause menu
		/// </summary>
		public void TogglePauseMenu()
		{
			ActivatePauseMenu(!IsPauseMenuOpen);
		}

		/// <summary>
		///     Activate a pause menu
		/// </summary>
		/// <param name="state"></param>
		public void ActivatePauseMenu(bool state)
		{
			if (pauseMenu.gameObject.activeSelf && ConsoleSetup.ConsoleUI != null)
			{
				if(ConsoleSetup.ConsoleUI.IsOpen())
					return;
			}

			IsPauseMenuOpen = state;

			Cursor.visible = IsPauseMenuOpen;
			Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

			pauseMenu.gameObject.SetActive(state);
			killFeed.killFeedItemsHolder.gameObject.SetActive(!state);

			if (state)
			{
				scoreBoardObject.SetActive(false);
				if(chat.IsChatOpen)
					chat.ActivateChat(false);
				
				chat.gameObject.SetActive(false);
			}
			else
			{
				chat.gameObject.SetActive(true);
			}

			if (PlayerManager.IsDead) return;
			ActivateHud(!state);
		}

		/// <summary>
		///     Toggles the score board
		/// </summary>
		public void ToggleScoreBoard()
		{
			scoreBoardObject.SetActive(!scoreBoardObject.activeSelf);
		}

		/// <summary>
		///		Activates the hud
		/// </summary>
		/// <param name="state"></param>
		public void ActivateHud(bool state)
		{
			if(IsPauseMenuOpen && PlayerManager.IsDead)
				return;

			hud.gameObject.SetActive(state);
		}
	}
}