using System.Collections;
using Core;
using Core.Networking.Messages;
using Player;
using UI.Elements;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace UI
{
	internal class KillFeed : MonoBehaviour
	{
		public Transform killFeedItemsHolder;
		[SerializeField] private GameObject killFeedItem;
		[SerializeField] private float killFeedItemLastTime = 5.0f;
		[SerializeField] private int maxAmountOfKillFeedItems = 5;

		public void AddFeedBackItem(PlayerDiedMessage message)
		{
			if(killFeedItemsHolder.childCount >= maxAmountOfKillFeedItems)
				Destroy(killFeedItemsHolder.GetChild(killFeedItemsHolder.childCount - 1));

			PlayerManager killer = GameManager.GetPlayer(message.PlayerKiller);
			PlayerManager killed = GameManager.GetPlayer(message.PlayerKilled);

			GameObject newKillFeedItem = Instantiate(killFeedItem, killFeedItemsHolder, false);
			newKillFeedItem.GetComponent<KillFeedItem>().SetupItem(killer.username, killed.username);
			StartCoroutine(DestructInTime(newKillFeedItem));

			Logger.Info($"`{killer.username}` killed `{killed.username}` using `{message.WeaponName}`.");
		}

		private IEnumerator DestructInTime(Object killFeedItemToDestroy)
		{
			yield return new WaitForSeconds(killFeedItemLastTime);

			Destroy(killFeedItemToDestroy);
		}
	}
}
