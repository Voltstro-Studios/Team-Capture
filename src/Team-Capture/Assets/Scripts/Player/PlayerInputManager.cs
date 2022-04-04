// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Console;
using Team_Capture.Helper.Extensions;
using Team_Capture.Input;
using Team_Capture.Player.Movement;
using Team_Capture.Settings;
using Team_Capture.Settings.SettingClasses;
using Team_Capture.UI;
using Team_Capture.Weapons;
using UnityEngine;

namespace Team_Capture.Player
{
    /// <summary>
    ///     Handles input
    /// </summary>
    [DefaultExecutionOrder(1200)]
    internal sealed class PlayerInputManager : NetworkBehaviour
    {
        [Header("Player Movement")] [SerializeField]
        private float xMouseSensitivity = 100.0f;

        [SerializeField] private float yMouseSensitivity = 100.0f;

        [SerializeField] private bool reverseMouse;

        private PlayerMovementManager playerInput;
        private PlayerManager playerManager;
        private PlayerUIManager uiManager;
        private WeaponManager weaponManager;

        private void Start()
        {
            weaponManager = this.GetComponentOrThrow<WeaponManager>();
            playerManager = this.GetComponentOrThrow<PlayerManager>();
            playerInput = this.GetComponentOrThrow<PlayerMovementManager>();
            uiManager = this.GetComponentOrThrow<PlayerUIManager>();

            GameSettings.SettingsUpdated += UpdateSettings;

            UpdateSettings();
        }

        private void Update()
        {
            //Make sure mouse lock/visibility is correct
            HandleMouseLock();

            if (ConsoleSetup.ConsoleUI != null)
                if (ConsoleSetup.ConsoleUI.IsOpen())
                    uiManager.SetPauseMenu(true);

            //If the pause menu is open, set player movement direction to 0 and return
            if (ClientUI.IsPauseMenuOpen || uiManager.IsChatOpen)
            {
                weaponManager.WeaponSway.SetInput(0, 0);
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
                if (reverseMouse)
                {
                    rotationX = look.y * yMouseSensitivity * Time.fixedDeltaTime;
                    rotationY = look.x * xMouseSensitivity * Time.fixedDeltaTime;
                }
                else
                {
                    rotationX = look.x * xMouseSensitivity * Time.fixedDeltaTime;
                    rotationY = look.y * yMouseSensitivity * Time.fixedDeltaTime;
                }

                //Send inputs
                playerInput.SetInput(horizontal, vertical, rotationX, rotationY, wishToJump);
                weaponManager.WeaponSway.SetInput(rotationX, rotationY);
            }
            else
            {
                wishToJump = false;
                rotationX = 0f;
                rotationY = 0f;
                vertical = 0f;
                horizontal = 0f;
            }
        }

        private void OnDisable()
        {
            InputReader.DisablePlayerInput();
            InputReader.DisableChatInput();

            InputReader.PlayerScoreboard -= OnPlayerScoreBoard;
            InputReader.PlayerSuicide -= OnPlayerSuicidePress;
            InputReader.PlayerJump -= OnPlayerJump;
            InputReader.PlayerPause -= OnPlayerPause;
            InputReader.PlayerWeaponSelection -= OnPlayerWeaponSelection;
            InputReader.PlayerWeaponShoot -= OnPlayerWeaponShoot;
            InputReader.PlayerWeaponReload -= OnPlayerWeaponReload;

            InputReader.ChatSubmit -= OnChatSubmit;
            InputReader.ChatToggle -= OnChatToggle;
        }

        public void Setup()
        {
            //Setup player input
            InputReader.PlayerScoreboard += OnPlayerScoreBoard;
            InputReader.PlayerSuicide += OnPlayerSuicidePress;
            InputReader.PlayerJump += OnPlayerJump;
            InputReader.PlayerPause += OnPlayerPause;
            InputReader.PlayerWeaponSelection += OnPlayerWeaponSelection;
            InputReader.PlayerWeaponShoot += OnPlayerWeaponShoot;
            InputReader.PlayerWeaponReload += OnPlayerWeaponReload;

            InputReader.ChatSubmit += OnChatSubmit;
            InputReader.ChatToggle += OnChatToggle;

            InputReader.EnablePlayerInput();
            InputReader.EnableChatInput();
        }

        #region Input Settings

        private void UpdateSettings()
        {
            MouseSettingsClass mouseSettings = GameSettings.MouseSettings;
            xMouseSensitivity = mouseSettings.MouseSensitivity;
            yMouseSensitivity = mouseSettings.MouseSensitivity;
            reverseMouse = mouseSettings.ReverseMouse;
        }

        #endregion

        #region Inputs to send

        private float rotationX;
        private float rotationY;

        private float vertical;
        private float horizontal;

        private bool wishToJump;

        #endregion

        #region Input Functions

        private void HandleMouseLock()
        {
            if (ClientUI.IsPauseMenuOpen || uiManager.IsChatOpen)
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

        private void OnPlayerScoreBoard()
        {
            if (ClientUI.IsPauseMenuOpen)
                return;

            uiManager.ToggleScoreboard();
        }

        private void OnPlayerSuicidePress()
        {
            if (!playerManager.IsDead)
                playerManager.CmdSuicide();
        }

        private void OnPlayerJump(bool jump)
        {
            if (!playerManager.IsDead)
                wishToJump = jump;
        }

        private void OnPlayerPause()
        {
            uiManager.TogglePauseMenu();
        }

        private void OnPlayerWeaponSelection(float value)
        {
            if (ClientUI.IsPauseMenuOpen || uiManager.IsChatOpen)
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
            weaponManager.CmdSetWeaponIndex(selectedWeaponIndex);
        }

        private void OnPlayerWeaponShoot(bool button)
        {
            if (ClientUI.IsPauseMenuOpen || uiManager.IsChatOpen)
                return;

            weaponManager.CmdShootWeapon(button);
        }

        private void OnPlayerWeaponReload()
        {
            if (ClientUI.IsPauseMenuOpen || uiManager.IsChatOpen)
                return;

            weaponManager.ClientReloadWeapon();
        }

        private void OnChatSubmit()
        {
            uiManager.SubmitChatMessage();
        }

        private void OnChatToggle()
        {
            if(ClientUI.IsPauseMenuOpen)
                return;
            
            uiManager.ToggleChat();
        }

        #endregion
    }
}