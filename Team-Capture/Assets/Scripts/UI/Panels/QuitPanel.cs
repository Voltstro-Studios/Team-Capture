using System.IO;
using Core;
using Mirror;
using TMPro;
using Random = UnityEngine.Random;

namespace UI.Panels
{
	public class QuitPanel : MainMenuPanelBase
	{
		private string[] quitSentences;

		public string quitMessagesLocation = "Resources/quit-messages.txt";
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

		public void Quit()
		{
			if (NetworkManager.singleton.isNetworkActive)
				NetworkManager.singleton.StopHost();

			Game.QuitGame();
		}
	}
}