using Console;
using Mirror;
using Player.Movement;
using UI;
using UnityEngine;
using Weapons;

namespace Player
{
	public class PlayerInput : NetworkBehaviour
	{
		#region Inspector fields
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

		[Header("Sensitivity")]
		[SerializeField] private float xMouseSensitivity = 100.0f;
		[SerializeField] private float yMouseSensitivity = 100.0f;

		#endregion
		
		private WeaponManager weaponManager;
		private PlayerManager playerManager;
		private AuthCharInput playerInput;

		private float rotationX;
		private float rotationY;

		private float vertical;
		private float horizontal;

		private bool wishToJump;

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
			playerInput = GetComponent<AuthCharInput>();
		}

		private void Update()
		{
			if (!isLocalPlayer) return; //Acts as a backup

			//Pause menu
			if (Input.GetKeyDown(pauseMenuKey))
			{
				if(playerManager.ClientUi.pauseMenu.GetActivePanel() == null)
					playerManager.ClientUi.TogglePauseMenu();
			}

			//Make sure mouse lock/visibility is correct
			HandleMouseLock();

			if (ConsoleSetup.ConsoleUI != null)
			{
				if (ConsoleSetup.ConsoleUI.IsOpen())
				{
					playerManager.ClientUi.ActivatePauseMenu(true);
					return;
				}
			}

			//If the pause menu is open, set player movement direction to 0 and return
			if (ClientUI.IsPauseMenuOpen)
			{
				playerInput.AddInput(0, 0, 0, 0, false);
				return;
			}

			//Scoreboard
			if (Input.GetKeyDown(scoreBoardKey) || Input.GetKeyUp(scoreBoardKey))
				playerManager.ClientUi.ToggleScoreBoard();

			//Don't want to move if the player is dead
			if (!playerManager.IsDead)
			{
				//Player movement rotation
				SetPlayerMouseRotation();

				//Player movement jump
				SetPlayerWishToJump();

				//Player movement direction
				SetPlayerMovementDirection();

				//Send inputs
				playerInput.AddInput(horizontal, vertical, rotationX, rotationY, wishToJump);

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
			{
				rotationX = Input.GetAxisRaw(xMouseAxisName) * yMouseSensitivity;
				rotationY = Input.GetAxisRaw(yMouseAxisName) * xMouseSensitivity;
			}
			else
			{
				rotationX = Input.GetAxis(xMouseAxisName) * xMouseSensitivity;
				rotationY = Input.GetAxis(yMouseAxisName) * yMouseSensitivity;
			}
		}

		private void SetPlayerWishToJump()
		{
			if (ClientUI.IsPauseMenuOpen)
				return;

			wishToJump = Input.GetKey(jumpKey);
		}

		private void SetPlayerMovementDirection()
		{
			if (rawAxis)
			{
				vertical = Input.GetAxisRaw(verticalAxisName);
				horizontal = Input.GetAxisRaw(horizontalAxisName);
			}
			else
			{
				vertical = Input.GetAxis(verticalAxisName);
				horizontal = Input.GetAxis(horizontalAxisName);
			}
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