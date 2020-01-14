using Player;
using TMPro;
using UnityEngine;

public class ScoreBoardPlayer : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI playerNameText;
	[SerializeField] private TextMeshProUGUI playerKillsText;
	[SerializeField] private TextMeshProUGUI playerDeathsText;

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

		playerKillsText.text = playerToTrack.GetKills.ToString();
		playerDeathsText.text = playerToTrack.GetDeaths.ToString();
	}
}