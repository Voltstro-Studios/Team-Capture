using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

namespace Panels
{
	public class QuitPanel : MonoBehaviour
	{
		public string[] quitSentences;

		public TextMeshProUGUI quitSentenceText;

		public Button noBtn;

		private void OnEnable()
		{
			quitSentenceText.text = quitSentences[Random.Range(0, quitSentences.Length)];
		}

		public void Quit()
		{
			Application.Quit(0);
		}
	}
}
