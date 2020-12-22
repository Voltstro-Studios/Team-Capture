using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Team_Capture.Input
{
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
    }
}