using Core.Console;
using TMPro;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class ConsoleGUI : ConsoleInterface
	{
		[SerializeField] private TMP_InputField inputField;

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
			if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
			{
				HandleInput(inputField.text);

				inputField.text = "";
				inputField.ActivateInputField();
			}
		}

		private void HandleInput(string value)
		{
			Logger.Log($" cmd>: {value}");

			if(string.IsNullOrWhiteSpace(value)) return;

			ExecuteCommand(value);
		}
	}
}