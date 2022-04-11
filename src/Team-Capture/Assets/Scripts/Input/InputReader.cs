// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Team_Capture.Input
{
    //Ohhh how much I love inputs... no, inputs can go fuck them selves. 
    //
    //Unity old input system was alright, apart from you had to call Input.GetButton/Key every update, and that cross-platform was a fucking pain.
    //The new input system uses events, which is 100% times better, (you can also do .ReadValue, which is handy for stuff like mouses)
    //but its fucking dog shit to setup and use, and has no fucking XML docs on it, so you have to refer to the online docs constantly.
    //I hope Unity can prove me wrong in the future, and fix their shit up. (Unlikely ROLF) Wasn't the point of Unity to be easy to use?

    /// <summary>
    ///     Handles reading inputs
    /// </summary>
    public static class InputReader
    {
        private static GameInput gameInput;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            if (gameInput == null)
            {
                gameInput = new GameInput();

                //Console
                gameInput.Console.ToggleConsole.performed += OnToggleConsole;
                gameInput.Console.AutoComplete.performed += OnAutoComplete;
                gameInput.Console.HistoryUp.performed += OnHistoryUp;
                gameInput.Console.HistoryDown.performed += OnHistoryDown;
                gameInput.Console.SubmitInput.performed += OnSubmitInput;

                //MenuController
                gameInput.MenuController.Close.performed += OnMenuClose;

                //StartVideo
                gameInput.StartVideo.Skip.performed += OnStartVideoSkip;

                //Debug menu
                gameInput.DebugMenu.ToggleMenu.performed += OnDebugMenuToggle;

                //Player
                gameInput.Player.ScoreBoard.performed += OnPlayerScoreBoard;
                gameInput.Player.Suicide.performed += OnPlayerSuicide;
                gameInput.Player.Jump.performed += OnPlayerJump;
                gameInput.Player.WeaponSelection.performed += OnPlayerWeaponSelection;
                gameInput.Player.ShootWeapon.performed += OnPlayerWeaponShoot;
                gameInput.Player.ReloadWeapon.performed += OnPlayerReloadWeapon;
                
                //Player UI
                gameInput.PlayerUI.Pause.performed += OnPlayerPause;

                //Chat
                gameInput.Chat.SubmitChat.performed += OnChatSubmit;
                gameInput.Chat.ToggleChat.performed += OnChatToggle;

                Application.quitting += ShutdownInput;
            }
        }

        private static void ShutdownInput()
        {
            gameInput.Disable();

            //Console
            gameInput.Console.ToggleConsole.performed -= OnToggleConsole;
            gameInput.Console.AutoComplete.performed -= OnAutoComplete;
            gameInput.Console.HistoryUp.performed -= OnHistoryUp;
            gameInput.Console.HistoryDown.performed -= OnHistoryDown;
            gameInput.Console.SubmitInput.performed -= OnSubmitInput;

            //MenuController
            gameInput.MenuController.Close.performed -= OnMenuClose;

            //StartVideo
            gameInput.StartVideo.Skip.performed -= OnStartVideoSkip;

            //Debug menu
            gameInput.DebugMenu.ToggleMenu.performed -= OnDebugMenuToggle;

            //Player
            gameInput.Player.ScoreBoard.performed -= OnPlayerScoreBoard;
            gameInput.Player.Suicide.performed -= OnPlayerSuicide;
            gameInput.Player.Jump.performed -= OnPlayerJump;
            gameInput.Player.WeaponSelection.performed -= OnPlayerWeaponSelection;
            gameInput.Player.ShootWeapon.performed -= OnPlayerWeaponShoot;
            gameInput.Player.ReloadWeapon.performed -= OnPlayerReloadWeapon;
            
            //Player UI
            gameInput.PlayerUI.Pause.performed -= OnPlayerPause;

            //Chat
            gameInput.Chat.SubmitChat.performed -= OnSubmitInput;
            gameInput.Chat.ToggleChat.performed -= OnChatToggle;

            gameInput.Dispose();

            Application.quitting -= ShutdownInput;
        }

        #region Console

        public static event Action ConsoleToggle;
        public static event Action ConsoleAutoComplete;
        public static event Action ConsoleHistoryUp;
        public static event Action ConsoleHistoryDown;
        public static event Action ConsoleSubmitInput;

        private static void OnToggleConsole(InputAction.CallbackContext context)
        {
            ConsoleToggle?.Invoke();
        }

        private static void OnAutoComplete(InputAction.CallbackContext context)
        {
            ConsoleAutoComplete?.Invoke();
        }

        private static void OnHistoryUp(InputAction.CallbackContext context)
        {
            ConsoleHistoryUp?.Invoke();
        }

        private static void OnHistoryDown(InputAction.CallbackContext context)
        {
            ConsoleHistoryDown?.Invoke();
        }

        private static void OnSubmitInput(InputAction.CallbackContext context)
        {
            ConsoleSubmitInput?.Invoke();
        }

        public static void EnableConsoleInput()
        {
            gameInput.Console.Enable();
        }

        public static void DisableConsoleInput()
        {
            gameInput.Console.Disable();
        }

        #endregion

        #region MenuController

        public static event Action MenuClose;

        private static void OnMenuClose(InputAction.CallbackContext context)
        {
            MenuClose?.Invoke();
        }

        public static void EnableMenuControllerInput()
        {
            gameInput.MenuController.Enable();
        }

        public static void DisableMenuControllerInput()
        {
            gameInput.MenuController.Disable();
        }

        #endregion

        #region StartVideo

        public static event Action StartVideoSkip;

        private static void OnStartVideoSkip(InputAction.CallbackContext context)
        {
            StartVideoSkip?.Invoke();
        }

        public static void EnableStartVideoInput()
        {
            gameInput.StartVideo.Enable();
        }

        public static void DisableStartVideoInput()
        {
            gameInput.StartVideo.Disable();
        }

        #endregion

        #region DebugMenu

        public static event Action DebugMenuToggle;

        private static void OnDebugMenuToggle(InputAction.CallbackContext context)
        {
            DebugMenuToggle?.Invoke();
        }

        public static void EnableDebugMenuInput()
        {
            gameInput.DebugMenu.Enable();
        }

        public static void DisableDebugMenuInput()
        {
            gameInput.DebugMenu.Disable();
        }

        #endregion

        #region Player

        public static event Action PlayerScoreboard;
        public static event Action PlayerSuicide;
        public static event Action<bool> PlayerJump;
        public static event Action<float> PlayerWeaponSelection;
        public static event Action<bool> PlayerWeaponShoot;
        public static event Action PlayerWeaponReload;

        private static void OnPlayerScoreBoard(InputAction.CallbackContext context)
        {
            PlayerScoreboard?.Invoke();
        }

        private static void OnPlayerSuicide(InputAction.CallbackContext context)
        {
            PlayerSuicide?.Invoke();
        }

        private static void OnPlayerJump(InputAction.CallbackContext context)
        {
            PlayerJump?.Invoke(context.ReadValueAsButton());
        }

        public static Vector2 ReadPlayerMove()
        {
            return gameInput.Player.Move.ReadValue<Vector2>();
        }

        public static Vector2 ReadPlayerLook()
        {
            return gameInput.Player.Look.ReadValue<Vector2>();
        }

        private static void OnPlayerWeaponSelection(InputAction.CallbackContext context)
        {
            PlayerWeaponSelection?.Invoke(context.ReadValue<float>());
        }

        private static void OnPlayerWeaponShoot(InputAction.CallbackContext context)
        {
            PlayerWeaponShoot?.Invoke(context.ReadValueAsButton());
        }

        private static void OnPlayerReloadWeapon(InputAction.CallbackContext context)
        {
            PlayerWeaponReload?.Invoke();
        }

        public static void EnablePlayerInput()
        {
            gameInput.Player.Enable();
        }

        public static void DisablePlayerInput()
        {
            gameInput.Player.Disable();
        }

        #endregion

        #region Player UI
        
        public static event Action PlayerUIPause;

        private static void OnPlayerPause(InputAction.CallbackContext context)
        {
            PlayerUIPause?.Invoke();
        }

        public static void EnablePlayerUI()
        {
            gameInput.PlayerUI.Enable();
        }

        public static void DisablePlayerUI()
        {
            gameInput.PlayerUI.Disable();
        }

        #endregion

        #region Player Death Cam

        public static Vector2 ReadPlayerDeathCamLook()
        {
            return gameInput.PlayerDeathCam.Look.ReadValue<Vector2>();
        }

        public static void EnablePlayerDeathCamInput()
        {
            gameInput.PlayerDeathCam.Enable();
        }

        public static void DisablePlayerDeathCamInput()
        {
            gameInput.PlayerDeathCam.Disable();
        }

        #endregion

        #region Chat

        public static event Action ChatToggle;
        public static event Action ChatSubmit;

        private static void OnChatToggle(InputAction.CallbackContext context)
        {
            ChatToggle?.Invoke();
        }

        private static void OnChatSubmit(InputAction.CallbackContext context)
        {
            ChatSubmit?.Invoke();
        }

        public static void EnableChatInput()
        {
            gameInput.Chat.Enable();
        }

        public static void DisableChatInput()
        {
            gameInput.Chat.Disable();
        }

        #endregion
    }
}