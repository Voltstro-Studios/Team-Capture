using System;
using Mirror;
using Settings.SettingClasses;
using Team_Capture.Console;
using Team_Capture.Player.Movement;
using Team_Capture.Settings;
using Team_Capture.UI;
using Team_Capture.Weapons;
using UnityEngine;

namespace Team_Capture.Player
{
	/// <summary>
	///     Handles input
	/// </summary>
	internal sealed class PlayerInput : NetworkBehaviour
	{
		private PlayerMovementInput playerInput;
		private PlayerManager playerManager;
		private PlayerWeaponShoot weaponShoot;
		private PlayerUIManager uiManager;

		private WeaponManager weaponManager;

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
			weaponShoot = GetComponent<PlayerWeaponShoot>();
			playerInput = GetComponent<PlayerMovementInput>();
			uiManager = GetComponent<PlayerUIManager>();

			GameSettings.SettingsUpdated += UpdateSettings;

			UpdateSettings();
		}

		public void Setup(InputReader reader)
		{
			InputReader = reader;

			InputReader.PlayerScoreboard += () => uiManager.ToggleScoreboard();
			InputReader.PlayerSuicide += OnPlayerSuicidePress;
			InputReader.PlayerJump += OnPlayerJump;
			InputReader.PlayerPause += () => uiManager.TogglePauseMenu();
			InputReader.PlayerWeaponSelection += OnPlayerWeaponSelection;
			InputReader.PlayerWeaponShoot += OnPlayerWeaponShoot;
			InputReader.PlayerWeaponReload += OnPlayerWeaponReload;

			InputReader.EnablePlayerInput();
		}

		private void OnDisable()
		{
			InputReader.DisablePlayerInput();
		}

		private void Update()
		{
			if (!isLocalPlayer) return; //Acts as a backup

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

			//Don't want to move if the player is dead
			if (!playerManager.IsDead)
			{
				//Movement
				Vector2 movement = InputReader.ReadPlayerMove();
				horizontal = movement.x;
				vertical = movement.y;

				//Look
				Vector2 look = InputReader.ReadPlayerLook();
				rotationX = look.x * xMouseSensitivity * Time.deltaTime;
				rotationY = look.y * yMouseSensitivity * Time.deltaTime;

				//Send inputs
				playerInput.SetInput(horizontal, vertical, rotationX, rotationY, wishToJump);
				weaponManager.WeaponSway.SetInput(rotationX, rotationY);

				//Weapon selection, we do this last
				//SetSelectedWeaponIndex();
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

		[Header("Inputs")]
		[SerializeField] private string mouseScrollWheel = "Mouse ScrollWheel";

		[NonSerialized] public InputReader InputReader;

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

		private void OnPlayerSuicidePress()
		{
			if(!playerManager.IsDead)
				playerManager.CmdSuicide();
		}

		private void OnPlayerJump(bool jump)
		{
			if (!playerManager.IsDead)
				wishToJump = jump;
		}

		private void OnPlayerWeaponSelection(float value)
		{
			if(ClientUI.IsPauseMenuOpen)
				return;

			int selectedWeaponIndex = weaponManager.SelectedWeaponIndex;
			int weaponHolderChildCount = weaponManager.WeaponHolderSpotChildCount - 1;

			if (value > 0f)
			{
				if (selectedWeaponIndex >= weaponHolderChildCount)
					selectedWeaponIndex = 0;
				else
					selectedWeaponIndex++;
			}

			if (value < 0f)
			{
				if (selectedWeaponIndex <= 0)
					selectedWeaponIndex = weaponHolderChildCount;
				else
					selectedWeaponIndex--;
			}

			if (selectedWeaponIndex == weaponManager.SelectedWeaponIndex) return;
			weaponManager.CmdSetWeapon(selectedWeaponIndex);
		}

		private void OnPlayerWeaponShoot(bool button)
		{
			weaponShoot.ShootWeapon(button);
		}

		private void OnPlayerWeaponReload()
		{
			if(ClientUI.IsPauseMenuOpen)
				return;

			weaponManager.ClientReloadWeapon();
		}

		#endregion
	}
}