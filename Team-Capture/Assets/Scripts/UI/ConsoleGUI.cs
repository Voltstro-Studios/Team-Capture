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

		[SerializeField] private bool showDebugMessages;

		[SerializeField] private int consoleTextScale = 1;
		private int defaultFontSize;

		private readonly List<string> lines = new List<string>();

		private static ConsoleGUI instance;

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Logger.ConsoleLogEvent += LoggerLog;

			instance = this;
			DontDestroyOnLoad(gameObject);
			RegisterCommands();

			defaultFontSize = consoleTextArea.resizeTextMaxSize;

#if !UNITY_EDITOR
			string[] file = {"autoexec"};
			ExecuteFile(file);
#else
			//Not the greatest idea hard coding this but what ever
			ExecuteCommand("scene MainMenu");
#endif
			Logger.Log("Console ready!");
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
		
		[ConCommand("console", "Toggles the console")]
		public static void ToggleConsoleCommand(string[] args)
		{
			instance.ToggleConsole();
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

		#region Console Commands

		[ConCommand("debug_messages", "Do you want to show debug messages in the console?")]
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
					instance.showDebugMessages = true;
					Logger.Log("Console will now show debug messages.");
					break;
				case "0":
				case "false":
					instance.showDebugMessages = false;
					Logger.Log("Console will no longer show debug messages.");
					break;
				default:
					Logger.Log("Invalid argument!", LogVerbosity.Error);
					break;
			}
		}

		[ConCommand("console_scale", "Changes the console's text scale", 1, 1)]
		public static void ConsoleScaleCommand(string[] args)
		{
			if (int.TryParse(args[0], out int result))
			{
				instance.consoleTextScale = result;
				instance.consoleTextArea.resizeTextMaxSize = instance.defaultFontSize * instance.consoleTextScale;

				return;
			}

			Logger.Log("The imputed argument isn't a number!", LogVerbosity.Error);
		}

		#endregion
	}
}