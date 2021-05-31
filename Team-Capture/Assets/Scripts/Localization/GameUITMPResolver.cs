// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using TMPro;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

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