using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Panels
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
            Application.Quit(0);
        }
    }
}