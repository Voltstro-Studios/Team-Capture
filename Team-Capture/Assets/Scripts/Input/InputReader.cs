using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Team_Capture
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

				//Player
				gameInput.Player.ScoreBoard.performed += OnPlayerScoreBoard;
				gameInput.Player.Suicide.performed += OnPlayerSuicide;
				gameInput.Player.Jump.performed += OnPlayerJump;
				gameInput.Player.Pause.performed += OnPlayerPause;

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

			//Player
			gameInput.Player.ScoreBoard.performed -= OnPlayerScoreBoard;
			gameInput.Player.Suicide.performed -= OnPlayerSuicide;
			gameInput.Player.Jump.performed -= OnPlayerJump;
			gameInput.Player.Pause.performed -= OnPlayerPause;

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

	    #region Player

	    public event Action PlayerScoreboard;
	    public event Action PlayerSuicide;
	    public event Action<bool> PlayerJump;
	    public event Action PlayerPause;

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