using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Networking;
using Player;
using TMPro;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace UI.ScoreBoard
{
	/// <summary>
	/// The score board
	/// </summary>
	public class ScoreBoard : MonoBehaviour
	{
		[HideInInspector] public PlayerManager clientPlayer;

		[Header("Scoreboard Settings")] 
		[SerializeField] private TextMeshProUGUI mapNameText;
		[SerializeField] private TextMeshProUGUI gameNameText;
		[SerializeField] private TextMeshProUGUI killDeathRatioText;

		[SerializeField] private GameObject playerItemPrefab;

		[Header("Player List Settings")]
		[SerializeField] private TextMeshProUGUI playerNameText;
		[SerializeField] private TextMeshProUGUI playerStatsText;

		[SerializeField] private Transform playerListTransform;

		private readonly Dictionary<PlayerManager, GameObject> playerList = new Dictionary<PlayerManager, GameObject>();
		private List<PlayerManager> players = new List<PlayerManager>();

		private void Start()
		{
			mapNameText.text = GameManager.GetActiveScene().displayName;
			gameNameText.text = TCNetworkManager.Instance.gameName;
		}

		private void OnEnable()
		{
			playerNameText.text = clientPlayer.username;

			//Get all players and create a player item on the scoreboard for them
			players = GameManager.GetAllPlayers().ToList();
			foreach (PlayerManager player in players)
			{
				CreateNewPlayerItem(player);
			}

			UpdatePlayerStats();

			//Set our stats at the bottom of the screen
			if (clientPlayer.Kills != 0 && clientPlayer.Deaths != 0)
				killDeathRatioText.text = "K/D: " + clientPlayer.Kills / clientPlayer.Deaths;

			playerStatsText.text = $"Kills: {clientPlayer.Kills}\nDeaths: {clientPlayer.Deaths}";
		}

		private void OnDisable()
		{
			for (int i = 0; i < playerListTransform.childCount; i++)
				Destroy(playerListTransform.GetChild(i).gameObject);

			foreach (PlayerManager player in players)
			{
				//Unsubscribe from the player killed event
				player.EventPlayerKilled -= PlayerOnEventPlayerKilled;
			}

			playerList.Clear();
		}

		private void UpdatePlayerStats()
		{
			//Sort the player list
			players.Sort(new PlayerListComparer());

			for (int i = 0; i < players.Count; i++)
			{
				//Get the player in the dictionary
				GameObject player = playerList[players[i]];

				//If the player doesn't exist then create a new player item on the scoreboard
				if (player == null)
				{
					CreateNewPlayerItem(players[i]);

					//Now it defiantly should exist
					player = playerList[players[i]];
				}

				//Update its spot
				player.transform.SetSiblingIndex(i);

				//Update its stats
				player.GetComponent<ScoreBoardPlayer>().UpdatePlayerStats();
			}
		}

		private void CreateNewPlayerItem(PlayerManager player)
		{
			GameObject newPlayerItem = Instantiate(playerItemPrefab, playerListTransform, false);
			ScoreBoardPlayer playerItemLogic = newPlayerItem.GetComponent<ScoreBoardPlayer>();
			if (playerItemLogic == null)
			{
				Logger.Error("The playerItemPrefab doesn't have a ScoreBoardPlayer behaviour on it!");
				return;
			}

			playerItemLogic.SetupPlayerInfo(player);
			playerItemLogic.UpdatePlayerStats();

			player.EventPlayerKilled += PlayerOnEventPlayerKilled;

			playerList.Add(player, newPlayerItem);
		}

		private void PlayerOnEventPlayerKilled(string playerKilledId, string playerKillerId)
		{
			UpdatePlayerStats();

			//Update our stats if it was our client
			if (GameManager.GetPlayer(playerKilledId) != clientPlayer &&
			    GameManager.GetPlayer(playerKillerId) != clientPlayer) return;

			if (clientPlayer.Kills != 0 && clientPlayer.Deaths != 0)
				killDeathRatioText.text = "K/D: " + clientPlayer.Kills / clientPlayer.Deaths;

			playerStatsText.text = $"Kills: {clientPlayer.Kills}\nDeaths: {clientPlayer.Deaths}";
		}

		private class PlayerListComparer : IComparer<PlayerManager>
		{
			public int Compare(PlayerManager x, PlayerManager y)
			{
				// ReSharper disable PossibleNullReferenceException
				if (x.Kills == 0 && y.Kills == 0)
					return 0;

				return y.Kills.CompareTo(x.Kills);
				// ReSharper enable PossibleNullReferenceException
			}
		}
	}
}