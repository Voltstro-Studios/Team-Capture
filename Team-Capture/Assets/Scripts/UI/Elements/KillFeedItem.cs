// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using TMPro;
using UnityEngine;

namespace Team_Capture.UI.Elements
{
	/// <summary>
	///     An item for the kill feed
	/// </summary>
	internal class KillFeedItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text killFeedText;

		/// <summary>
		///     Sets up the kill feed item
		/// </summary>
		/// <param name="killerName"></param>
		/// <param name="killedName"></param>
		public void SetupItem(string killerName, string killedName)
		{
			killFeedText.text = $"<b>{killerName}</b> / {killedName}";
		}
	}
}