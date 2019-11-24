using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Player;
using SceneManagement;

public class GameManager : MonoBehaviour
{
	//On player equip weapon
	public delegate void OnPlayerPickupWeapon(string player, GameObject weaponPickup);
	public OnPlayerPickupWeapon onPlayerPickupWeapon;

	public static GameManager Instance;

	public TCScene scene;

	public GameObject sceneCamera;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("There is more than one GameManger in active scene!");
		}
		else
		{
			Instance = this;

			Setup();
		}
	}

	public void Setup()
	{
		scene = TCScenesManager.FindScene(SceneManager.GetActiveScene().name);
		if (scene == null)
		{
			Debug.LogError("This scene doesn't have a TCScene!");
		}

		sceneCamera = GameObject.FindWithTag("SceneCamera");
		if (sceneCamera == null)
		{
			Debug.LogError("No GameObject with a tag of SceneCamera!");
		}
	}

	#region Player Tracking

	private const string PlayerIdPrefix = "Player ";

	private static readonly Dictionary<string, PlayerManager> Players = new Dictionary<string, PlayerManager>();

	/// <summary>
	/// Adds a player
	/// </summary>
	/// <param name="netId"></param>
	/// <param name="playerManager"></param>
	public static void AddPlayer(string netId, PlayerManager playerManager)
	{
		string playerId = PlayerIdPrefix + netId;
		playerManager.transform.name = playerId;
		Players.Add(playerId, playerManager);

		Debug.Log($"Added {playerId}.");
	}

	/// <summary>
	/// Removes a player
	/// </summary>
	/// <param name="playerId"></param>
	public static void RemovePlayer(string playerId)
	{
		Players.Remove(playerId);
	}

	public static PlayerManager GetPlayer(string playerId)
	{
		return Players[playerId];
	}

	public static PlayerManager[] GetAllPlayers()
	{
		return Players.Values.ToArray();
	}

	#endregion
}
