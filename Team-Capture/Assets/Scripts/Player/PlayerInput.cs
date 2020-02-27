using Mirror;
using UI;
using UnityEngine;
using Weapons;

namespace Player
{
	public class PlayerInput : NetworkBehaviour
	{
		[Header("Keycodes")]
		[SerializeField] private KeyCode pauseMenuKey = KeyCode.Escape;
		[SerializeField] private KeyCode scoreBoardKey = KeyCode.Tab;
		[SerializeField] private KeyCode suicideKey = KeyCode.P;
		[SerializeField] private KeyCode jumpKey = KeyCode.Space;

		[Header("Player Movement")] 
		[SerializeField] private bool rawAxis = true;
		[SerializeField] private bool rawMouseAxis = true;
		[SerializeField] private string verticalAxisName = "Vertical";
		[SerializeField] private string horizontalAxisName = "Horizontal";
		[SerializeField] private string yMouseAxisName = "Mouse Y";
		[SerializeField] private string xMouseAxisName = "Mouse X";
		[SerializeField] private bool holdJumpToBhop = true;

		private WeaponManager weaponManager;
		private PlayerManager playerManager;
		private PlayerMovement playerMovement;

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
			playerMovement = GetComponent<PlayerMovement>();
		}

		private void Update()
		{
			if (!isLocalPlayer) return; //Acts as a backup

			//Pause menu
			if (Input.GetKeyDown(pauseMenuKey))
				playerManager.clientUi.TogglePauseMenu();

			//Make sure mouse lock/visibility is correct
			HandleMouseLock();

			if (ConsoleGUI.Instance != null)
			{
				if (ConsoleGUI.Instance.IsOpen())
				{
					playerManager.clientUi.ActivatePauseMenu(true);
					return;
				}
			}

			//If the pause menu is open, set player movement direction to 0 and return
			if (ClientUI.IsPauseMenuOpen)
			{
				playerMovement.SetMovementDir(0, 0);
				playerMovement.SetWishJump(false);
				return;
			}

			//Scoreboard
			if (Input.GetKeyDown(scoreBoardKey) || Input.GetKeyUp(scoreBoardKey))
				playerManager.clientUi.ToggleScoreBoard();

			//Don't want to move if the player is dead
			if (!playerManager.IsDead)
			{
				//Player movement rotation
				SetPlayerMouseRotation();

				//Player movement jump
				SetPlayerWishToJump();

				//Player movement direction
				SetPlayerMovementDirection();

				//Good ol' suicide button
				if (Input.GetKeyDown(suicideKey))
					playerManager.CmdSuicide();

				//Weapon selection, we do this last
				SetSelectedWeaponIndex();
			}
		}

		private static void HandleMouseLock()
		{
			if (ClientUI.IsPauseMenuOpen)
			{
				if (!Cursor.visible)
					Cursor.visible = true;

				if (Cursor.lockState != CursorLockMode.None)
					Cursor.lockState = CursorLockMode.None;

				return;
			}
			
			if (Cursor.visible)
				Cursor.visible = false;

			if (Cursor.lockState != CursorLockMode.Locked)
				Cursor.lockState = CursorLockMode.Locked;
		}

		private void SetPlayerMouseRotation()
		{
			if(rawMouseAxis)
				playerMovement.SetMouseRotation(Input.GetAxisRaw(yMouseAxisName), Input.GetAxisRaw(xMouseAxisName));
			else
				playerMovement.SetMouseRotation(Input.GetAxis(yMouseAxisName), Input.GetAxis(xMouseAxisName));
		}

		private void SetPlayerWishToJump()
		{
			if (ClientUI.IsPauseMenuOpen)
				return;

			if (holdJumpToBhop)
			{
				playerMovement.SetWishJump(Input.GetKey(jumpKey));
				return;
			}

			if (Input.GetKeyDown(jumpKey) && !playerMovement.WishToJump)
				playerMovement.SetWishJump(true);
			if (Input.GetKeyUp(jumpKey))
				playerMovement.SetWishJump(false);
		}

		private void SetPlayerMovementDirection()
		{
			if(rawAxis)
				playerMovement.SetMovementDir(Input.GetAxisRaw(verticalAxisName), Input.GetAxisRaw(horizontalAxisName));
			else
				playerMovement.SetMovementDir(Input.GetAxis(verticalAxisName), Input.GetAxis(horizontalAxisName));
		}

		private void SetSelectedWeaponIndex()
		{
			int selectedWeaponIndex = weaponManager.SelectedWeaponIndex;
			int weaponHolderChildCount = weaponManager.WeaponHolderSpotChildCount - 1;

			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				if (selectedWeaponIndex >= weaponHolderChildCount)
					selectedWeaponIndex = 0;
				else
					selectedWeaponIndex++;
			}

			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				if (selectedWeaponIndex <= 0)
					selectedWeaponIndex = weaponHolderChildCount;
				else
					selectedWeaponIndex--;
			}

			if (selectedWeaponIndex == weaponManager.SelectedWeaponIndex) return;
			weaponManager.CmdSetWeapon(selectedWeaponIndex);
			//playerManager.clientUi.hud.UpdateAmmoUi(weaponManager);
		}
	}
}