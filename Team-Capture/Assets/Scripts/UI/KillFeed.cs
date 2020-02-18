using Core;
using Core.Networking.Messages;
using Player;
using UI.Elements;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class KillFeed : MonoBehaviour
	{
		[SerializeField] private GameObject killFeedItem;
		[SerializeField] private float killFeedItemLastTime = 5.0f;
		[SerializeField] private int maxAmountOfKillFeedItems = 5;

		public void AddFeedBackItem(PlayerDiedMessage message)
		{
			if(transform.childCount >= maxAmountOfKillFeedItems)
				Destroy(transform.GetChild(transform.childCount - 1));

			PlayerManager killer = GameManager.GetPlayer(message.PlayerKiller);
			PlayerManager killed = GameManager.GetPlayer(message.PlayerKilled);

			Instantiate(killFeedItem, transform, false).GetComponent<KillFeedItem>().SetupItem(killer.username, killed.username, killFeedItemLastTime);

			Logger.Log($"`{killer.username}` killed `{killed.username}` using `{message.WeaponName}`.");
		}
	}
}
