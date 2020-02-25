using Core.Console;
using TMPro;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class ConsoleGUI : ConsoleInterface
	{
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private GameObject consolePanel;
		[SerializeField] private KeyCode consoleToggleKey = KeyCode.F1;

		private static ConsoleGUI _instance;

		private void Awake()
		{
			if (_instance != null)
			{
				Destroy(gameObject);
				return;
			}

			_instance = this;
			DontDestroyOnLoad(gameObject);
			RegisterCommands();
		}

		private void Update()
		{
			if (Input.GetKeyDown(consoleToggleKey))
			{
				ToggleConsole();
			}

			if(!consolePanel.activeSelf) return;

			if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
			{
				SubmitInput();
			}
		}

		#region Console GUI

		public void ToggleConsole()
		{
			consolePanel.SetActive(!consolePanel.activeSelf);
		}
		
		[ConCommand(Name = "console", Summary = "Toggles the console")]
		public static void ToggleConsoleCommand(string[] args)
		{
			_instance.ToggleConsole();
		}

		#endregion

		#region Console Input

		public void SubmitInput()
		{
			HandleInput(inputField.text);

			inputField.text = "";
			inputField.ActivateInputField();
		}

		private void HandleInput(string value)
		{
			Logger.Log($" cmd>: {value}");

			if(string.IsNullOrWhiteSpace(value)) return;

			ExecuteCommand(value);
		}
		
		#endregion
	}
}