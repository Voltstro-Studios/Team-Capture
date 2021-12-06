// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Team_Capture.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Console
{
    /// <summary>
    ///     An in-game console
    /// </summary>
    internal class ConsoleGUI : MonoBehaviour, IConsoleUI
    {
        [ConVar("console_scale", "Sets the console's scale", nameof(UpdateConsoleScaleCallback), true)]
        public static float ConsoleTextScale = 1;

        [ConVar("console_log_debug", "Shows debug logs in the console", true)]
        public static bool ShowDebugMessages = false;

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI consoleTextArea;
        [SerializeField] private ScrollRect consoleScrollRect;
        [SerializeField] private GameObject consolePanel;

        private readonly List<string> lines = new();
        private float defaultFontSize;

        public void Init()
        {
            defaultFontSize = consoleTextArea.fontSize;

            //Disable it
            ToggleConsole();

            Logger.Info("Console in-game GUI ready!");

            InputReader.ConsoleToggle += ToggleConsole;
            InputReader.ConsoleAutoComplete += AutoCompleteConsole;
            InputReader.ConsoleHistoryUp += HistoryUp;
            InputReader.ConsoleHistoryDown += HistoryDown;
            InputReader.ConsoleSubmitInput += SubmitInput;

            InputReader.EnableConsoleInput();
        }

        public void Shutdown()
        {
            InputReader.DisableConsoleInput();
            
            InputReader.ConsoleToggle -= ToggleConsole;
            InputReader.ConsoleAutoComplete -= AutoCompleteConsole;
            InputReader.ConsoleHistoryUp -= HistoryUp;
            InputReader.ConsoleHistoryDown -= HistoryDown;
            InputReader.ConsoleSubmitInput -= SubmitInput;
        }

        public void UpdateConsole()
        {
        }

        public void LogMessage(string message, LogType logType)
        {
            if (consoleTextArea == null) return;

            if (logType == LogType.Assert && !ShowDebugMessages) return;

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
            Canvas.ForceUpdateCanvases();
            consoleScrollRect.normalizedPosition = new Vector2(0, 0);
        }

        #region Console GUI

        /// <summary>
        ///     Toggles the in-game viewable console
        /// </summary>
        public void ToggleConsole()
        {
            consolePanel.SetActive(!IsOpen());

            if (IsOpen())
                inputField.ActivateInputField();
        }

        public bool IsOpen()
        {
            return consolePanel.activeSelf;
        }

        private void AutoCompleteConsole()
        {
            if (!IsOpen())
                return;
            
            if(string.IsNullOrWhiteSpace(inputField.text))
                return;

            inputField.text = ConsoleBackend.AutoComplete(inputField.text);
            SetInputFieldCaretPos(inputField.text.Length);
        }

        private void HistoryUp()
        {
            if (!IsOpen())
                return;

            inputField.text = ConsoleBackend.HistoryUp(inputField.text);
            SetInputFieldCaretPos(inputField.text.Length);
        }

        private void HistoryDown()
        {
            if (!IsOpen())
                return;

            inputField.text = ConsoleBackend.HistoryDown();
            SetInputFieldCaretPos(inputField.text.Length);
        }

        private void SetInputFieldCaretPos(int pos)
        {
            StartCoroutine(SetPosition());

            IEnumerator SetPosition()
            {
                int width = inputField.caretWidth;
                inputField.caretWidth = 0;

                yield return new WaitForEndOfFrame();

                inputField.caretWidth = width;
                inputField.caretPosition = pos;
            }
        }

        #endregion

        #region Console Input

        public void SubmitInput()
        {
            if (!IsOpen())
                return;

            HandleInput(inputField.text);

            inputField.text = "";
            inputField.ActivateInputField();
        }

        private static void HandleInput(string value)
        {
            Logger.Info($"cmd>: {value}");

            if (string.IsNullOrWhiteSpace(value)) return;

            ConsoleBackend.ExecuteCommand(value);
        }

        #endregion

        #region Console Commands

        [ConCommand("help", "Shows a list of all the commands")]
        public static void HelpCommand(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var helpList = new List<string>();
            foreach (var command in ConsoleBackend.GetAllCommands())
                helpList.Add($"\n`{command.Key}` - {command.Value.CommandSummary}");

            helpList.Sort(string.Compare);

            Logger.Info(string.Join("", helpList));

            stopwatch.Stop();
            Logger.Debug("Took {Time}ms to build help menu.", stopwatch.Elapsed.TotalMilliseconds);
        }

        [ConCommand("version", "Shows Team-Capture's current version")]
        public static void VersionCommand(string[] args)
        {
            Logger.Info($"You are running TC version {Application.version} using Unity {Application.unityVersion}");
        }

        [ConCommand("console", "Toggles the console", CommandRunPermission.Both, 0, 0, true)]
        public static void ToggleConsoleCommand(string[] args)
        {
            if (ConsoleSetup.ConsoleUI is ConsoleGUI gui) gui.ToggleConsole();
        }

        public static void UpdateConsoleScaleCallback()
        {
            if (!(ConsoleSetup.ConsoleUI is ConsoleGUI gui)) return;

            gui.consoleTextArea.fontSize = gui.defaultFontSize * ConsoleTextScale;
        }

        #endregion
    }
}