using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	public class QuitPanel : MonoBehaviour
	{
		public Button noBtn;
		public string[] quitSentences;

		public TextMeshProUGUI quitSentenceText;

		private void OnEnable()
		{
			quitSentenceText.text = quitSentences[Random.Range(0, quitSentences.Length)];
		}

		public void Quit()
		{
			if(NetworkManager.singleton.isNetworkActive)
				NetworkManager.singleton.StopHost();

			Application.Quit(0);
		}
	}
}