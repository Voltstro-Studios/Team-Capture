﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Team_Capture.Core;
using Team_Capture.Core.Networking;
using Team_Capture.Player;
using Team_Capture.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using Logger = Team_Capture.Logging.Logger;

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

        /// <summary>
        ///     <see cref="LocalizedString" /> for the death text
        /// </summary>
        [Tooltip("Localized String for the death text")] [SerializeField]
        private LocalizedString deathText;

        /// <summary>
        ///     <see cref="LocalizedString" /> for the kills text
        /// </summary>
        [Tooltip("Localized String for the kills text")] [SerializeField]
        private LocalizedString killsText;

        private readonly List<ScoreBoardPlayer> playerItems = new();

        private string deathTextCache;
        private string killsTextCache;
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
            for (int i = 0; i < players.Count; i++)
            {
                PlayerManager player = players[i];
                GameObject playerItem = Instantiate(playerItemPrefab, playerListTransform, false);
                ScoreBoardPlayer scoreBoardPlayer = playerItem.GetComponent<ScoreBoardPlayer>();

                scoreBoardPlayer.SetupPlayerInfo(player);
                playerItems.Add(scoreBoardPlayer);

                player.PlayerDeath += PlayerKilled;
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
                item.GetComponent<ScoreBoardPlayer>().PlayerToTrack.PlayerDeath -= PlayerKilled;

                Destroy(item);
            }
        }

        /// <summary>
        ///     Updates the UI positions of the player scoreboard items
        /// </summary>
        private void UpdateUIPositions()
        {
            for (int i = 0; i < playerItems.Count; i++)
                playerItems[i].transform.SetSiblingIndex(players.IndexOf(playerItems[i].PlayerToTrack));
        }

        private void PlayerKilled()
        {
            SortPlayerList();
            UpdateUIPositions();

            for (int i = 0; i < playerItems.Count; i++)
                playerItems[i].UpdatePlayerStats();
        }

        private void ClientPlayerUpdateUI()
        {
            killDeathRatioText.text = $"{clientPlayer.Kills}/{clientPlayer.Deaths}";
            playerStatsText.text =
                $"{killsTextCache}: {clientPlayer.Kills}\n{deathTextCache}: {clientPlayer.Deaths}";
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

        private void Start()
        {
            playerNameText.text = clientPlayer.User.UserName;
            mapNameText.text = GameSceneManager.GetActiveScene().DisplayNameLocalized;
            gameNameText.text = TCNetworkManager.Instance.serverConfig.GameName.String;
        }

        private void OnEnable()
        {
            SetPlayerList();

            GameManager.PlayerAdded += OnPlayerAdded;
            GameManager.PlayerRemoved += OnPlayerRemoved;

            killsText.StringChanged += OnKillsTextChange;
            deathText.StringChanged += OnDeathsTextChange;

            killsText.RefreshString();
            deathText.RefreshString();

            clientPlayer.PlayerKill += ClientPlayerUpdateUI;
            clientPlayer.PlayerDeath += ClientPlayerUpdateUI;
            ClientPlayerUpdateUI();
        }

        private void OnDisable()
        {
            players.Clear();
            ClearScoreBoardPlayerItems();

            GameManager.PlayerAdded -= OnPlayerAdded;
            GameManager.PlayerRemoved -= OnPlayerRemoved;

            killsText.StringChanged -= OnKillsTextChange;
            deathText.StringChanged -= OnDeathsTextChange;

            clientPlayer.PlayerKill -= ClientPlayerUpdateUI;
            clientPlayer.PlayerDeath -= ClientPlayerUpdateUI;
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

        #region Localized String Updates

        private void OnKillsTextChange(string value)
        {
            killsTextCache = value;
        }

        private void OnDeathsTextChange(string value)
        {
            deathTextCache = value;
        }

        #endregion
    }
}