using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Team_Capture.Core;
using Team_Capture.Core.Networking;
using Team_Capture.Localization;
using Team_Capture.Player;
using TMPro;
using UnityEngine;
using Logger = Team_Capture.Core.Logging.Logger;

namespace Team_Capture.UI.ScoreBoard
{
	/// <summary>
	///     The scoreboard controller
	/// </summary>
	internal class ScoreBoard : MonoBehaviour
	{
		[HideInInspector] public PlayerManager clientPlayer;

		/// <summary>
		///     The map text element
		/// </summary>
		[Header("Scoreboard Settings")] [Tooltip("The map text element")] [SerializeField]
		private TextMeshProUGUI mapNameText;

		/// <summary>
		///     The game name text element
		/// </summary>
		[Tooltip("The game name text element")] [SerializeField]
		private TextMeshProUGUI gameNameText;

		/// <summary>
		///     The kill death ratio text element
		/// </summary>
		[Tooltip("The kill death ratio text element")] [SerializeField]
		private TextMeshProUGUI killDeathRatioText;

		/// <summary>
		///     The prefab used for players on the scoreboard
		/// </summary>
		[Tooltip("The prefab used for players on the scoreboard")] [SerializeField]
		private GameObject playerItemPrefab;

		/// <summary>
		///     The player name text element
		/// </summary>
		[Header("Player List Settings")] [Tooltip("The player name text element")] [SerializeField]
		private TextMeshProUGUI playerNameText;

		/// <summary>
		///     The player stats text element
		/// </summary>
		[Tooltip("The player stats text element")] [SerializeField]
		private TextMeshProUGUI playerStatsText;

		/// <summary>
		///     The player list <see cref="Transform" />
		/// </summary>
		[Tooltip("The player list transform")] [SerializeField]
		private Transform playerListTransform;

		private readonly List<ScoreBoardPlayer> playerItems = new List<ScoreBoardPlayer>();

		private string deathsTextLocale;

		private string killsTextLocale;

		private List<PlayerManager> players;

		/// <summary>
		///     Generate the list from scratch
		///     <para>Should only need to do this when a player is added or removed, or when the panel is opened</para>
		/// </summary>
		private void SetPlayerList()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			players = GameManager.GetAllPlayers().ToList();
			SortPlayerList();
			SetScoreBoardPlayerItems();
			UpdateUIPositions();

			stopwatch.Stop();
			Logger.Debug($"Took {stopwatch.Elapsed.TotalMilliseconds}ms to update scoreboard player items.");
		}

		/// <summary>
		///     Sorts the <see cref="players" /> list
		/// </summary>
		private void SortPlayerList()
		{
			players.Sort(new PlayerListComparer());
		}

		/// <summary>
		///     Generate the player scoreboard items from scratch
		/// </summary>
		private void SetScoreBoardPlayerItems()
		{
			ClearScoreBoardPlayerItems();
			foreach (PlayerManager player in players)
			{
				GameObject playerItem = Instantiate(playerItemPrefab, playerListTransform, false);
				ScoreBoardPlayer scoreBoardPlayer = playerItem.GetComponent<ScoreBoardPlayer>();

				scoreBoardPlayer.SetupPlayerInfo(player);
				playerItems.Add(scoreBoardPlayer);

				player.PlayerDied += PlayerKilled;
			}
		}

		/// <summary>
		///     Clears all the player scoreboard items
		/// </summary>
		private void ClearScoreBoardPlayerItems()
		{
			playerItems.Clear();

			for (int i = 0; i < playerListTransform.childCount; i++)
			{
				GameObject item = playerListTransform.GetChild(i).gameObject;
				item.GetComponent<ScoreBoardPlayer>().PlayerToTrack.PlayerDied -= PlayerKilled;

				Destroy(item);
			}
		}

		/// <summary>
		///     Updates the UI positions of the player scoreboard items
		/// </summary>
		private void UpdateUIPositions()
		{
			foreach (ScoreBoardPlayer scoreBoardPlayer in playerItems)
				scoreBoardPlayer.transform.SetSiblingIndex(players.IndexOf(scoreBoardPlayer.PlayerToTrack));
		}

		private void PlayerKilled()
		{
			SortPlayerList();
			UpdateUIPositions();

			foreach (ScoreBoardPlayer scoreBoardPlayer in playerItems)
				scoreBoardPlayer.UpdatePlayerStats();
		}

		private void ClientPlayerUpdateUI()
		{
			killDeathRatioText.text = $"{clientPlayer.Kills}/{clientPlayer.Deaths}";
			playerStatsText.text =
				$"{killsTextLocale}: {clientPlayer.Kills}\n{deathsTextLocale}: {clientPlayer.Deaths}";
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

		#region Unity Callbacks

		private void Awake()
		{
			killsTextLocale = GameUILocale.ResolveString("ScoreBoard_Kills");
			deathsTextLocale = GameUILocale.ResolveString("ScoreBoard_Deaths");
		}

		private void Start()
		{
			playerNameText.text = clientPlayer.username;
			mapNameText.text = GameManager.GetActiveScene().DisplayNameLocalized;
			gameNameText.text = TCNetworkManager.Instance.serverConfig.gameName;
		}

		private void OnEnable()
		{
			SetPlayerList();

			GameManager.PlayerAdded += OnPlayerAdded;
			GameManager.PlayerRemoved += OnPlayerRemoved;

			clientPlayer.PlayerKilled += ClientPlayerUpdateUI;
			clientPlayer.PlayerDied += ClientPlayerUpdateUI;
			ClientPlayerUpdateUI();
		}

		private void OnDisable()
		{
			players.Clear();
			ClearScoreBoardPlayerItems();

			GameManager.PlayerAdded -= OnPlayerAdded;
			GameManager.PlayerRemoved -= OnPlayerRemoved;

			clientPlayer.PlayerKilled -= ClientPlayerUpdateUI;
			clientPlayer.PlayerDied -= ClientPlayerUpdateUI;
		}

		#endregion

		#region GameManager Callbacks

		private void OnPlayerAdded(string playerId)
		{
			SetPlayerList();
		}

		private void OnPlayerRemoved(string playerId)
		{
			SetPlayerList();
		}

		#endregion
	}
}