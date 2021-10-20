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
	///		Handles reading inputs
	/// </summary>
	[CreateAssetMenu(fileName = "InputReader", menuName = "Team Capture/Input Reader")]
    public class InputReader : ScriptableObject
    {
	    private static GameInput gameInput;

	    public void OnEnable()
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
				gameInput.Player.Pause.performed += OnPlayerPause;
				gameInput.Player.WeaponSelection.performed += OnPlayerWeaponSelection;
				gameInput.Player.ShootWeapon.performed += OnPlayerWeaponShoot;
				gameInput.Player.ReloadWeapon.performed += OnPlayerReloadWeapon;
				
				//Chat
				gameInput.Chat.SubmitChat.performed += OnChatSubmit;
				gameInput.Chat.ToggleChat.performed += OnChatToggle;

				Application.quitting += ShutdownInput;
		    }
	    }

	    public void ShutdownInput()
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
			gameInput.Player.Pause.performed -= OnPlayerPause;
			gameInput.Player.WeaponSelection.performed -= OnPlayerWeaponSelection;
			gameInput.Player.ShootWeapon.performed -= OnPlayerWeaponShoot;
			gameInput.Player.ReloadWeapon.performed -= OnPlayerReloadWeapon;
			
			//Chat
			gameInput.Chat.SubmitChat.performed -= OnSubmitInput;
			gameInput.Chat.ToggleChat.performed -= OnChatToggle;

			gameInput.Dispose();

		    Application.quitting -= ShutdownInput;
	    }

	    #region Console

	    public event Action ConsoleToggle;
	    public event Action ConsoleAutoComplete;
	    public event Action ConsoleHistoryUp;
	    public event Action ConsoleHistoryDown;
	    public event Action ConsoleSubmitInput;

	    private void OnToggleConsole(InputAction.CallbackContext context)
	    {
		    ConsoleToggle?.Invoke();
	    }

	    private void OnAutoComplete(InputAction.CallbackContext context)
	    {
		    ConsoleAutoComplete?.Invoke();
	    }

	    private void OnHistoryUp(InputAction.CallbackContext context)
	    {
		    ConsoleHistoryUp?.Invoke();
	    }

	    private void OnHistoryDown(InputAction.CallbackContext context)
	    {
		    ConsoleHistoryDown?.Invoke();
	    }

	    private void OnSubmitInput(InputAction.CallbackContext context)
	    {
		    ConsoleSubmitInput?.Invoke();
	    }

	    public void EnableConsoleInput()
	    {
			gameInput.Console.Enable();
	    }

	    public void DisableConsoleInput()
	    {
			gameInput.Console.Disable();
	    }

	    #endregion

	    #region MenuController

	    public event Action MenuClose;
	    
	    private void OnMenuClose(InputAction.CallbackContext context)
	    {
		    MenuClose?.Invoke();
	    }

	    public void EnableMenuControllerInput()
	    {
			gameInput.MenuController.Enable();
	    }

	    public void DisableMenuControllerInput()
	    {
			gameInput.MenuController.Disable();
	    }

	    #endregion

	    #region StartVideo

	    public event Action StartVideoSkip;

	    private void OnStartVideoSkip(InputAction.CallbackContext context)
	    {
		    StartVideoSkip?.Invoke();
	    }

	    public void EnableStartVideoInput()
	    {
			gameInput.StartVideo.Enable();
	    }

	    public void DisableStartVideoInput()
	    {
			gameInput.StartVideo.Disable();
	    }

	    #endregion

	    #region DebugMenu

	    public event Action DebugMenuToggle;

	    private void OnDebugMenuToggle(InputAction.CallbackContext context)
	    {
		    DebugMenuToggle?.Invoke();
	    }

	    public void EnableDebugMenuInput()
	    {
			gameInput.DebugMenu.Enable();
	    }

	    public void DisableDebugMenuInput()
	    {
			gameInput.DebugMenu.Disable();
	    }

	    #endregion

	    #region Player

	    public event Action PlayerScoreboard;
	    public event Action PlayerSuicide;
	    public event Action<bool> PlayerJump;
	    public event Action PlayerPause;
	    public event Action<float> PlayerWeaponSelection;
	    public event Action<bool> PlayerWeaponShoot;
	    public event Action PlayerWeaponReload;

	    private void OnPlayerScoreBoard(InputAction.CallbackContext context)
	    {
		    PlayerScoreboard?.Invoke();
	    }

	    private void OnPlayerSuicide(InputAction.CallbackContext context)
	    {
		    PlayerSuicide?.Invoke();
	    }
		
	    private void OnPlayerJump(InputAction.CallbackContext context)
	    {
		    PlayerJump?.Invoke(context.ReadValueAsButton());
	    }

	    private void OnPlayerPause(InputAction.CallbackContext context)
	    {
		    PlayerPause?.Invoke();
	    }

	    public Vector2 ReadPlayerMove() => gameInput.Player.Move.ReadValue<Vector2>();

	    public Vector2 ReadPlayerLook() => gameInput.Player.Look.ReadValue<Vector2>();

	    private void OnPlayerWeaponSelection(InputAction.CallbackContext context)
	    {
		    PlayerWeaponSelection?.Invoke(context.ReadValue<float>());
	    }

	    private void OnPlayerWeaponShoot(InputAction.CallbackContext context)
	    {
		    PlayerWeaponShoot?.Invoke(context.ReadValueAsButton());
	    }

	    private void OnPlayerReloadWeapon(InputAction.CallbackContext context)
	    {
		    PlayerWeaponReload?.Invoke();
	    }

	    public void EnablePlayerInput()
	    {
			gameInput.Player.Enable();
	    }

	    public void DisablePlayerInput()
	    {
			gameInput.Player.Disable();
	    }

	    #endregion

	    #region Player Death Cam

	    public Vector2 ReadPlayerDeathCamLook() => gameInput.PlayerDeathCam.Look.ReadValue<Vector2>();

	    public void EnablePlayerDeathCamInput()
	    {
		    gameInput.PlayerDeathCam.Enable();
	    }

	    public void DisablePlayerDeathCamInput()
	    {
		    gameInput.PlayerDeathCam.Disable();
	    }

	    #endregion

	    #region Chat

	    public event Action ChatToggle;
	    public event Action ChatSubmit;

	    private void OnChatToggle(InputAction.CallbackContext context)
	    {
		    ChatToggle?.Invoke();
	    }

	    private void OnChatSubmit(InputAction.CallbackContext context)
	    {
		    ChatSubmit?.Invoke();
	    }

	    public void EnableChatInput()
	    {
		    gameInput.Chat.Enable();
	    }

	    public void DisableChatInput()
	    {
		    gameInput.Chat.Disable();
	    }

	    #endregion
    }
}