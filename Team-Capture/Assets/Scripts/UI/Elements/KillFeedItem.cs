using TMPro;
using UnityEngine;

namespace UI.Elements
{
	public class KillFeedItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text killFeedText;

		public void SetupItem(string killerName, string killedName)
		{
			killFeedText.text = $"<b>{killerName}</b> / {killedName}";
		}
	}
}