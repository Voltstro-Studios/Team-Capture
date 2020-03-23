using Player;
using UnityEngine;
using Weapons;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class ClientUI : MonoBehaviour
	{
		public static bool IsPauseMenuOpen;

		internal PlayerManager PlayerManager;
		internal WeaponManager WeaponManager;

		public Hud hud;
		public KillFeed killFeed;
		public MainMenuController pauseMenu;
		public GameObject scoreBoardObject;

		public ClientUI SetupUI(PlayerManager playerManager)
		{
			IsPauseMenuOpen = false;

			hud.Setup(this);

			PlayerManager = playerManager;
			WeaponManager = playerManager.GetComponent<WeaponManager>();

			pauseMenu.gameObject.SetActive(false);

			scoreBoardObject.SetActive(false);
			scoreBoardObject.GetComponent<ScoreBoard.ScoreBoard>().clientPlayer = playerManager;

			Logger.Log("The ClientUI is now ready.");

			return this;
		}

		public void TogglePauseMenu()
		{
			ActivatePauseMenu(!IsPauseMenuOpen);
		}

		public void ActivatePauseMenu(bool state)
		{
			IsPauseMenuOpen = state;

			Cursor.visible = IsPauseMenuOpen;
			Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

			pauseMenu.gameObject.SetActive(state);
			killFeed.killFeedItemsHolder.gameObject.SetActive(!state);

			if (PlayerManager.IsDead) return;
			hud.gameObject.SetActive(!state);
		}

		public void ToggleScoreBoard()
		{
			scoreBoardObject.SetActive(!scoreBoardObject.activeSelf);
		}
	}
}