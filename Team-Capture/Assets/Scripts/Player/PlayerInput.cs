using Console;
using Mirror;
using Player.Movement;
using Settings;
using Settings.SettingClasses;
using UI;
using UnityEngine;
using Weapons;

namespace Player
{
	/// <summary>
	///     Handles input
	/// </summary>
	internal sealed class PlayerInput : NetworkBehaviour
	{
		private PlayerMovementInput playerInput;
		private PlayerManager playerManager;
		private PlayerUIManager uiManager;

		private WeaponManager weaponManager;

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
			playerInput = GetComponent<PlayerMovementInput>();
			uiManager = GetComponent<PlayerUIManager>();

			GameSettings.SettingsUpdated += UpdateSettings;

			UpdateSettings();
		}

		private void Update()
		{
			if (!isLocalPlayer) return; //Acts as a backup

			//Pause menu
			if (Input.GetKeyDown(pauseMenuKey))
				uiManager.TogglePauseMenu();

			//Make sure mouse lock/visibility is correct
			HandleMouseLock();

			if (ConsoleSetup.ConsoleUI != null)
				if (ConsoleSetup.ConsoleUI.IsOpen())
				{
					uiManager.SetPauseMenu(true);
					return;
				}

			//If the pause menu is open, set player movement direction to 0 and return
			if (ClientUI.IsPauseMenuOpen)
			{
				playerInput.SetInput(0, 0, 0, 0, false);
				return;
			}

			//Scoreboard
			if (Input.GetKeyDown(scoreBoardKey) || Input.GetKeyUp(scoreBoardKey))
				uiManager.ToggleScoreboard();

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
				playerInput.SetInput(horizontal, vertical, rotationX, rotationY, wishToJump);
				weaponManager.WeaponSway.SetInput(rotationX, rotationY);

				//Good ol' suicide button
				if (Input.GetKeyDown(suicideKey))
					playerManager.CmdSuicide();

				//Weapon selection, we do this last
				SetSelectedWeaponIndex();
			}
		}

		#region Input Settings

		private void UpdateSettings()
		{
			MouseSettingsClass mouseSettings = GameSettings.MouseSettings;
			xMouseSensitivity = mouseSettings.MouseSensitivity;
			yMouseSensitivity = mouseSettings.MouseSensitivity;
			rawMouseAxis = mouseSettings.RawAxis;
			reverseMouse = mouseSettings.ReverseMouse;
		}

		#endregion

		#region Inspector fields

		[Header("Inputs")] [SerializeField] private KeyCode pauseMenuKey = KeyCode.Escape;

		[SerializeField] private KeyCode scoreBoardKey = KeyCode.Tab;
		[SerializeField] private KeyCode suicideKey = KeyCode.P;
		[SerializeField] private KeyCode jumpKey = KeyCode.Space;
		[SerializeField] private string verticalAxisName = "Vertical";
		[SerializeField] private string horizontalAxisName = "Horizontal";
		[SerializeField] private string yMouseAxisName = "Mouse Y";
		[SerializeField] private string xMouseAxisName = "Mouse X";
		[SerializeField] private string mouseScrollWheel = "Mouse ScrollWheel";

		[Header("Player Movement")] [SerializeField]
		private bool rawAxis = true;

		[SerializeField] private bool rawMouseAxis = true;

		[SerializeField] private float xMouseSensitivity = 100.0f;
		[SerializeField] private float yMouseSensitivity = 100.0f;

		[SerializeField] private bool reverseMouse;

		#endregion

		#region Inputs to send

		private float rotationX;
		private float rotationY;

		private float vertical;
		private float horizontal;

		private bool wishToJump;

		#endregion

		#region Input Functions

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
			if (rawMouseAxis)
			{
				if (reverseMouse)
				{
					rotationX = Input.GetAxisRaw(yMouseAxisName) * yMouseSensitivity * Time.deltaTime;
					rotationY = Input.GetAxisRaw(xMouseAxisName) * xMouseSensitivity * Time.deltaTime;
				}
				else
				{
					rotationX = Input.GetAxisRaw(xMouseAxisName) * xMouseSensitivity * Time.deltaTime;
					rotationY = Input.GetAxisRaw(yMouseAxisName) * yMouseSensitivity * Time.deltaTime;
				}
			}
			else
			{
				if (reverseMouse)
				{
					rotationX = Input.GetAxis(yMouseAxisName) * yMouseSensitivity;
					rotationY = Input.GetAxis(xMouseAxisName) * xMouseSensitivity;
				}
				else
				{
					rotationX = Input.GetAxis(xMouseAxisName) * xMouseSensitivity;
					rotationY = Input.GetAxis(yMouseAxisName) * yMouseSensitivity;
				}
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

			if (Input.GetAxis(mouseScrollWheel) > 0f)
			{
				if (selectedWeaponIndex >= weaponHolderChildCount)
					selectedWeaponIndex = 0;
				else
					selectedWeaponIndex++;
			}

			if (Input.GetAxis(mouseScrollWheel) < 0f)
			{
				if (selectedWeaponIndex <= 0)
					selectedWeaponIndex = weaponHolderChildCount;
				else
					selectedWeaponIndex--;
			}

			if (selectedWeaponIndex == weaponManager.SelectedWeaponIndex) return;
			weaponManager.CmdSetWeapon(selectedWeaponIndex);
		}

		#endregion
	}
}