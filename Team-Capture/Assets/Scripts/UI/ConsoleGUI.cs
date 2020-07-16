using System;
using System.Collections.Generic;
using System.Text;
using Attributes;
using Core.Console;
using Core.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Logging.Logger;

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

		public static ConsoleGUI Instance;

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
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
			Logger.Debug("Console ready!");
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

		internal void LoggerLog(string message, LogVerbosity logVerbosity)
		{
			if(consoleTextArea == null) return;

			if(logVerbosity == LogVerbosity.Debug && !showDebugMessages) return;

			switch (logVerbosity)
			{
				case LogVerbosity.Debug:
				case LogVerbosity.Info:
					lines.Add(message);
					break;
				case LogVerbosity.Error:
					lines.Add($"<color=red>{message}</color>");
					break;
				case LogVerbosity.Warn:
					lines.Add($"<color=yellow>{message}</color>");
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logVerbosity), logVerbosity, null);
			}

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

		#endregion

		#region Console Input

		public void SubmitInput()
		{
			HandleInput(inputField.text);

			inputField.text = "";
			inputField.ActivateInputField();
		}

		private static void HandleInput(string value)
		{
			Logger.Info($"cmd>: {value}");

			if(string.IsNullOrWhiteSpace(value)) return;

			ExecuteCommand(value);
		}
		
		#endregion

		#region Console Commands

		[ConCommand("help", "Shows a list of all the commands")]
		public static void HelpCommand(string[] args)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("\n");

			foreach (KeyValuePair<string, ConsoleCommand> command in GetAllCommands())
			{
				sb.Append($"`{command.Key}` - {command.Value.CommandSummary}\n");
			}

			Logger.Info(sb.ToString());
		}

		[ConCommand("version", "Shows Team-Capture's current version")]
		public static void VersionCommand(string[] args)
		{
			Logger.Info($"You are running TC version {Application.version} using Unity {Application.unityVersion}");
		}

		[ConCommand("debug_messages", "Do you want to show debug messages in the console?", 1, 1)]
		public static void ShowDebugMessagesCommand(string[] args)
		{
			string toggle = args[0].ToLower();

			switch (toggle)
			{
				case "1":
				case "true":
					Instance.showDebugMessages = true;
					Logger.Info("Console will now show debug messages.");
					break;
				case "0":
				case "false":
					Instance.showDebugMessages = false;
					Logger.Info("Console will no longer show debug messages.");
					break;
				default:
					Logger.Error("Invalid argument!");
					break;
			}
		}

		[ConCommand("console", "Toggles the console")]
		public static void ToggleConsoleCommand(string[] args)
		{
			Instance.ToggleConsole();
		}

		[ConCommand("console_scale", "Changes the console's text scale", 1, 1)]
		public static void ConsoleScaleCommand(string[] args)
		{
			if (int.TryParse(args[0], out int result))
			{
				Instance.consoleTextScale = result;
				Instance.consoleTextArea.resizeTextMaxSize = Instance.defaultFontSize * Instance.consoleTextScale;

				return;
			}

			Logger.Error("The imputed argument isn't a number!");
		}

		#endregion
	}
}