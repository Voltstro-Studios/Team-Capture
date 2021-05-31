// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using TMPro;
using UnityEngine;

namespace Team_Capture.Localization
{
	[RequireComponent(typeof(TMP_InputField))]
	public class GameUITMPInputFieldResolver : MonoBehaviour
	{
		private void Start()
		{
			TMP_InputField input = GetComponent<TMP_InputField>();
			input.text = GameUILocale.ResolveString(input.text);
			Destroy(this);
		}
	}
}