using TMPro;
using UnityEngine;

namespace Localization
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class GameUITMPResolver : MonoBehaviour
	{
		private void Start()
		{
			TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
			text.text = GameUILocale.ResolveString(text.text);
		}
	}
}