using Mirror;
using Player.Movement;
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

		[Header("Sensitivity")]
		[SerializeField] private float xMouseSensitivity = 100.0f;
		[SerializeField] private float yMouseSensitivity = 100.0f;

		private WeaponManager weaponManager;
		private PlayerManager playerManager;
		private PlayerMovement playerMovement;

		private float rotationX;
		private float rotationY;

		private float vertical;
		private float horizontal;

		private bool wishToJump;

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
			{
				if(playerManager.ClientUi.pauseMenu.GetActivePanel() == null)
					playerManager.ClientUi.TogglePauseMenu();
			}

			//Make sure mouse lock/visibility is correct
			HandleMouseLock();

			if (ConsoleGUI.Instance != null)
			{
				if (ConsoleGUI.Instance.IsOpen())
				{
					playerManager.ClientUi.ActivatePauseMenu(true);
					return;
				}
			}

			//If the pause menu is open, set player movement direction to 0 and return
			if (ClientUI.IsPauseMenuOpen)
			{
				playerMovement.AddInput(new PlayerInputs
				{
					Horizontal = 0,
					Vertical = 0,
					RotationX = 0,
					RotationY = 0,
					WishToJump = false
				});
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
				playerMovement.AddInput(new PlayerInputs
				{
					Horizontal = horizontal,
					Vertical = vertical,
					RotationX = rotationX,
					RotationY = rotationY,
					WishToJump = wishToJump
				});

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
				rotationX -= Input.GetAxisRaw(yMouseAxisName) * yMouseSensitivity * 0.02f;
				rotationY += Input.GetAxisRaw(xMouseAxisName) * xMouseSensitivity * 0.02f;
			}
			else
			{
				rotationX -= Input.GetAxis(xMouseAxisName) * xMouseSensitivity * 0.02f;
				rotationY += Input.GetAxis(yMouseAxisName) * yMouseSensitivity * 0.02f;
			}
		}

		private void SetPlayerWishToJump()
		{
			if (ClientUI.IsPauseMenuOpen)
				return;

			if (holdJumpToBhop)
			{
				wishToJump = Input.GetKey(jumpKey);
				return;
			}

			if (Input.GetKeyDown(jumpKey) && !wishToJump)
				wishToJump = true;
			if (Input.GetKeyUp(jumpKey))
				wishToJump = false;
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