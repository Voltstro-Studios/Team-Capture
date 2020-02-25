using Core;
using Mirror;
using TMPro;
using UnityEngine;

namespace UI.Panels
{
	public class QuitPanel : MainMenuPanelBase
	{
		public string[] quitSentences;

		public TextMeshProUGUI quitSentenceText;

		private void OnEnable()
		{
			quitSentenceText.text = quitSentences[Random.Range(0, quitSentences.Length)];
		}

		public void Quit()
		{
			if (NetworkManager.singleton.isNetworkActive)
				NetworkManager.singleton.StopHost();

			Game.QuitGame();
		}
	}
}