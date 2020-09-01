using TMPro;
using UnityEngine;

namespace UI.Elements
{
	/// <summary>
	/// An item for the kill feed
	/// </summary>
	public class KillFeedItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text killFeedText;

		/// <summary>
		/// Sets up the kill feed item
		/// </summary>
		/// <param name="killerName"></param>
		/// <param name="killedName"></param>
		public void SetupItem(string killerName, string killedName)
		{
			killFeedText.text = $"<b>{killerName}</b> / {killedName}";
		}
	}
}