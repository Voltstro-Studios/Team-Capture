using System.IO;
using Core;
using Mirror;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI.Panels
{
	public class QuitPanel : MainMenuPanelBase
	{
		private string[] quitSentences;

		/// <summary>
		/// The location of where the quit messages are
		/// </summary>
		[Tooltip("The location of where the quit messages are")]
		public string quitMessagesLocation = "Resources/quit-messages.txt";

		/// <summary>
		/// The text for where the quit messages will go
		/// </summary>
		[Tooltip("The text for where the quit messages will go")]
		public TextMeshProUGUI quitSentenceText;

		private void Awake()
		{
			if (File.Exists($"{Game.GetGameExecutePath()}/{quitMessagesLocation}"))
				quitSentences = File.ReadAllLines($"{Game.GetGameExecutePath()}/{quitMessagesLocation}");
		}

		private void OnEnable()
		{
			if (quitSentences != null)
			{
				quitSentenceText.text = quitSentences[Random.Range(0, quitSentences.Length)];
				return;
			}

			quitSentenceText.text = $"When you are missing {quitMessagesLocation}";
		}

		/// <summary>
		/// Quits from the game
		/// </summary>
		public void Quit()
		{
			if (NetworkManager.singleton.isNetworkActive)
				NetworkManager.singleton.StopHost();

			Game.QuitGame();
		}
	}
}