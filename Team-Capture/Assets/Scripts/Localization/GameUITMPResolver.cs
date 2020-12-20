using TMPro;
using UnityEngine;
using Logger = Team_Capture.Core.Logging.Logger;

namespace Team_Capture.Localization
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public sealed class GameUITMPResolver : MonoBehaviour
	{
		private void Start()
		{
			TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
			string currentId = text.text;
			text.text = GameUILocale.ResolveString(text.text);

			//The text was not found
			if (text.text == currentId)
				Logger.Warn("The localization key '{@Key}' was not found in the GameUI locale! ({@ObjectName})",
					currentId, gameObject.name);

			Destroy(this);
		}
	}
}