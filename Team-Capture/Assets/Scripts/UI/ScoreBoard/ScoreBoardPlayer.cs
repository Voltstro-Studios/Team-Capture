// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Globalization;
using Team_Capture.Player;
using TMPro;
using UnityEngine;

namespace Team_Capture.UI.ScoreBoard
{
	internal class ScoreBoardPlayer : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI playerDeathsText;
		[SerializeField] private TextMeshProUGUI playerKillsText;
		[SerializeField] private TextMeshProUGUI playerNameText;
		[SerializeField] private TextMeshProUGUI playerPingText;

		internal PlayerManager PlayerToTrack;

		public void SetupPlayerInfo(PlayerManager player)
		{
			PlayerToTrack = player;
			UpdatePlayerStats();
		}

		public void UpdatePlayerStats()
		{
			//Check to see if the player name has changed
			string userName = PlayerToTrack.User.UserName;
			if (playerNameText.text != userName)
				playerNameText.text = userName;

			playerKillsText.text = PlayerToTrack.Kills.ToString();
			playerDeathsText.text = PlayerToTrack.Deaths.ToString();
			playerPingText.text = Math.Round(PlayerToTrack.latency).ToString(CultureInfo.InvariantCulture);
		}
	}
}