using System;
using System.Collections.Generic;
using System.Text;
using Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Logging.Logger;

namespace Console
{
	/// <summary>
	/// A in-game console
	/// </summary>
	public class ConsoleGUI : MonoBehaviour, IConsoleUI
	{
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private Text consoleTextArea;
		[SerializeField] private GameObject consolePanel;
		[SerializeField] private KeyCode consoleToggleKey = KeyCode.F1;

		[SerializeField] private bool showDebugMessages;

		[SerializeField] private int consoleTextScale = 1;
		private int defaultFontSize;

		private readonly List<string> lines = new List<string>();

		public void Init()
		{
			defaultFontSize = consoleTextArea.resizeTextMaxSize;

			//Disable it
			ToggleConsole();

			Logger.Info("Console in-game GUI ready!");
		}

		public void Shutdown()
		{
		}

		public void UpdateConsole()
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

		/// <summary>
		/// Toggles the in-game viewable console
		/// </summary>
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

			ConsoleBackend.ExecuteCommand(value);
		}
		
		#endregion

		#region Console Commands

		[ConCommand("help", "Shows a list of all the commands")]
		public static void HelpCommand(string[] args)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("\n");

			foreach (KeyValuePair<string, ConsoleCommand> command in ConsoleBackend.GetAllCommands())
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

		[ConCommand("console", "Toggles the console")]
		public static void ToggleConsoleCommand(string[] args)
		{
			//This is aids but what ever
			(ConsoleSetup.ConsoleUI as ConsoleGUI)?.ToggleConsole();
		}

		[ConCommand("console_scale", "Changes the console's text scale", 1, 1)]
		public static void ConsoleScaleCommand(string[] args)
		{
			if (int.TryParse(args[0], out int result))
			{
				//More aids
				ConsoleGUI gui = ConsoleSetup.ConsoleUI as ConsoleGUI;
				if(gui == null) return;

				gui.consoleTextScale = result;
				gui.consoleTextArea.resizeTextMaxSize = gui.defaultFontSize * gui.consoleTextScale;

				return;
			}

			Logger.Error("The imputed argument isn't a number!");
		}

		#endregion

		public void LogMessage(string message, LogType logType)
		{
			if(consoleTextArea == null) return;

			if(logType == LogType.Assert && !showDebugMessages) return;

			switch (logType)
			{
				case LogType.Assert:
				case LogType.Log:
					lines.Add(message);
					break;
				case LogType.Exception:
				case LogType.Error:
					lines.Add($"<color=red>{message}</color>");
					break;
				case LogType.Warning:
					lines.Add($"<color=yellow>{message}</color>");
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
			}

			int count = Mathf.Min(100, lines.Count);
			int start = lines.Count - count;
			consoleTextArea.text = string.Join("\n", lines.GetRange(start, count).ToArray());
		}
	}
}