using System.Collections.Generic;
using Core.Console;
using Core.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class ConsoleGUI : ConsoleInterface
	{
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private Text consoleTextArea;
		[SerializeField] private GameObject consolePanel;
		[SerializeField] private KeyCode consoleToggleKey = KeyCode.F1;

		private readonly List<string> lines = new List<string>();

		private bool showDebugMessages;

		private static ConsoleGUI _instance;

		private void Awake()
		{
			if (_instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Logger.ConsoleLogEvent += LoggerLog;

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

		private void LoggerLog(string message, LogVerbosity logVerbosity)
		{
			if(consoleTextArea == null) return;

			if(logVerbosity == LogVerbosity.Debug && !showDebugMessages) return;

			lines.Add(message);
			int count = Mathf.Min(100, lines.Count);
			int start = lines.Count - count;
			consoleTextArea.text = string.Join("\n", lines.GetRange(start, count).ToArray());
		}

		#region Console GUI

		public void ToggleConsole()
		{
			consolePanel.SetActive(!IsOpen());

			if(IsOpen())
				inputField.ActivateInputField();
		}

		public bool IsOpen()
		{
			return consolePanel.activeSelf;
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

		[ConCommand(Name = "debug_messages", Summary = "Do you want to show debug messages in the console?")]
		public static void ShowDebugMessagesCommand(string[] args)
		{
			if (args.Length == 0)
			{
				Logger.Log("Invalid argument!", LogVerbosity.Error);
				return;
			}

			string toggle = args[0].ToLower();

			switch (toggle)
			{
				case "1":
				case "true":
					_instance.showDebugMessages = true;
					Logger.Log("Console will now show debug messages.");
					break;
				case "0":
				case "false":
					_instance.showDebugMessages = false;
					Logger.Log("Console will no longer show debug messages.");
					break;
				default:
					Logger.Log("Invalid argument!", LogVerbosity.Error);
					break;
			}
		}
	}
}