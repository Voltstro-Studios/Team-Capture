using Player;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class ClientUI : MonoBehaviour
	{
		public static bool IsPauseMenuOpen;

		public Hud hud;

		public KillFeed killFeed;

		public GameObject pauseMenuGameObject;

		public GameObject scoreBoardObject;

		[HideInInspector] public PlayerManager player;

		public ClientUI SetupUi(PlayerManager playerManager)
		{
			IsPauseMenuOpen = false;

			player = playerManager;

			hud.clientUi = this;

			pauseMenuGameObject.SetActive(false);

			scoreBoardObject.SetActive(false);
			scoreBoardObject.GetComponent<ScoreBoard.ScoreBoard>().clientPlayer = playerManager;

			Logger.Log("The ClientUI is now ready.");

			return this;
		}

		public void TogglePauseMenu()
		{
			IsPauseMenuOpen = !IsPauseMenuOpen;

			Cursor.visible = IsPauseMenuOpen;
			Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;

			pauseMenuGameObject.SetActive(IsPauseMenuOpen);
			killFeed.killFeedItemsHolder.gameObject.SetActive(!IsPauseMenuOpen);

			if (player.IsDead) return;
			hud.gameObject.SetActive(!IsPauseMenuOpen);
		}

		public void ToggleScoreBoard()
		{
			scoreBoardObject.SetActive(!scoreBoardObject.activeSelf);
		}
	}
}