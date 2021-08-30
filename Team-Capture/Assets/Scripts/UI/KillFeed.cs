﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections;
using Team_Capture.Core;
using Team_Capture.Player;
using Team_Capture.UI.Elements;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI
{
	/// <summary>
	///     Displays a list of kills
	/// </summary>
	internal class KillFeed : MonoBehaviour
	{
		/// <summary>
		///     Where the kill feed items go
		/// </summary>
		[Tooltip("Where the kill feed items go")]
		public Transform killFeedItemsHolder;

		/// <summary>
		///     A kill feed item
		/// </summary>
		[Tooltip("A kill feed item")] [SerializeField]
		private GameObject killFeedItem;

		/// <summary>
		///     How long a kill feed item should last for
		/// </summary>
		[Tooltip("How long a kill feed item should last for")] [SerializeField]
		private float killFeedItemLastTime = 5.0f;

		/// <summary>
		///     The max amount of kill feed items that can be displayed
		/// </summary>
		[Tooltip("The max amount of kill feed items that can be displayed")] [SerializeField]
		private int maxAmountOfKillFeedItems = 5;

		/// <summary>
		///     Adds a kill feed item
		/// </summary>
		/// <param name="message"></param>
		public void AddKillfeedItem(PlayerDiedMessage message)
		{
			//Removes the last kill feed item
			if (killFeedItemsHolder.childCount >= maxAmountOfKillFeedItems)
				Destroy(killFeedItemsHolder.GetChild(killFeedItemsHolder.childCount - 1));

			PlayerManager killer = GameManager.GetPlayer(message.PlayerKiller);
			PlayerManager killed = GameManager.GetPlayer(message.PlayerKilled);

			GameObject newKillFeedItem = Instantiate(killFeedItem, killFeedItemsHolder, false);
			string killerUsername = killer.User.UserName;
			string victimUsername = killed.User.UserName;
			
			newKillFeedItem.GetComponent<KillFeedItem>().SetupItem(killerUsername, victimUsername);
			StartCoroutine(DestructInTime(newKillFeedItem));

			Logger.Info($"`{killerUsername}` killed `{victimUsername}` using `{message.WeaponName}`.");
		}

		private IEnumerator DestructInTime(Object killFeedItemToDestroy)
		{
			yield return new WaitForSeconds(killFeedItemLastTime);

			Destroy(killFeedItemToDestroy);
		}
	}
}