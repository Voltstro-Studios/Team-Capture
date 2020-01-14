using System.Collections.Generic;
using System.Linq;
using Global;
using Mirror;
using Player;
using TMPro;
using UnityEngine;
using Logger = Global.Logger;

public class ScoreBoard : MonoBehaviour
{
	private List<PlayerManager> players = new List<PlayerManager>();

	[HideInInspector] public PlayerManager clientPlayer;

	[Header("Scoreboard Settings")]
	[SerializeField] private TextMeshProUGUI mapNameText;
	[SerializeField] private TextMeshProUGUI ipText;
	[SerializeField] private TextMeshProUGUI playerNameText;

	[Header("Player List Settings")]
	[SerializeField] private Transform playerListTransform;
	[SerializeField] private GameObject playerItemPrefab;

	private void Start()
	{
		mapNameText.text = GameManager.Instance.scene.displayName;
		ipText.text = NetworkManager.singleton.networkAddress;
	}

	private void OnEnable()
	{
		playerNameText.text = clientPlayer.username;

		players = GameManager.GetAllPlayers().ToList();

		foreach (PlayerManager player in players)
		{
			GameObject newPlayerItem = Instantiate(playerItemPrefab, playerListTransform, false);
			ScoreBoardPlayer playerItemLogic = newPlayerItem.GetComponent<ScoreBoardPlayer>();
			if (playerItemLogic == null)
			{
				Logger.Log("The playerItemPrefab doesn't have a ScoreBoardPlayer behaviour on it!", LogVerbosity.Error);
				return;
			}

			playerItemLogic.SetupPlayerInfo(player);
			playerItemLogic.UpdatePlayerStats();
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < playerListTransform.childCount; i++)
		{
			Destroy(playerListTransform.GetChild(i).gameObject);
		}
	}

	private void Update()
	{
		for (int i = 0; i < playerListTransform.childCount; i++)
		{
			playerListTransform.GetChild(i).GetComponent<ScoreBoardPlayer>().UpdatePlayerStats();
		}
	}
}
