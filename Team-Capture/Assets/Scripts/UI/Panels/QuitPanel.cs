using System.IO;
using Mirror;
using Team_Capture.Core;
using TMPro;
using UnityEngine;

namespace Team_Capture.UI.Panels
{
	internal class QuitPanel : MainMenuPanelBase
	{
		/// <summary>
		///     The location of where the quit messages are
		/// </summary>
		[Tooltip("The location of where the quit messages are")]
		public string quitMessagesLocation = "Resources/quit-messages.txt";

		/// <summary>
		///     The text for where the quit messages will go
		/// </summary>
		[Tooltip("The text for where the quit messages will go")]
		public TextMeshProUGUI quitSentenceText;

		private string[] quitSentences;

		private void Awake()
		{
			if (File.Exists($"{Game.GetGameExecutePath()}/{quitMessagesLocation}"))
				quitSentences = File.ReadAllLines($"{Game.GetGameExecutePath()}/{quitMessagesLocation}");
		}

		public override void OnEnable()
		{
			base.OnEnable();

			if (quitSentences != null)
			{
				quitSentenceText.text = quitSentences[Random.Range(0, quitSentences.Length)];
				return;
			}

			quitSentenceText.text = $"When you are missing {quitMessagesLocation}";
		}

		/// <summary>
		///     Quits from the game
		/// </summary>
		public void Quit()
		{
			if (NetworkManager.singleton.isNetworkActive)
				NetworkManager.singleton.StopHost();

			Game.QuitGame();
		}
	}
}