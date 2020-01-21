using System;
using System.Globalization;
using Player;
using TMPro;
using UnityEngine;

namespace UI.ScoreBoard
{
	public class ScoreBoardPlayer : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI playerDeathsText;
		[SerializeField] private TextMeshProUGUI playerKillsText;
		[SerializeField] private TextMeshProUGUI playerNameText;
		[SerializeField] private TextMeshProUGUI playerPingText;

		private PlayerManager playerToTrack;

		public void SetupPlayerInfo(PlayerManager player)
		{
			playerToTrack = player;
		}

		public void UpdatePlayerStats()
		{
			//Check to see if the player name has changed
			if (playerNameText.text != playerToTrack.username)
				playerNameText.text = playerToTrack.username;

			playerKillsText.text = playerToTrack.Kills.ToString();
			playerDeathsText.text = playerToTrack.Deaths.ToString();
			playerPingText.text = Math.Round(playerToTrack.latency).ToString(CultureInfo.InvariantCulture);
		}
	}
}