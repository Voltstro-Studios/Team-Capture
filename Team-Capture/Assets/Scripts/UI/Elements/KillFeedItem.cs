using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.Elements
{
	public class KillFeedItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text killFeedText;

		public void SetupItem(string killerName, string killedName, float destroyTime)
		{
			killFeedText.text = $"<b>{killerName}</b> / {killedName}";

			StartCoroutine(DestructInTime(destroyTime));
		}

		private IEnumerator DestructInTime(float time)
		{
			yield return new WaitForSeconds(time);

			Destroy(gameObject);
		}
	}
}
