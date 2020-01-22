using System.Collections.Generic;
using System.Linq;
using Player;
using SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
	public class GameManager : MonoBehaviour
	{
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
			scene = TCScenesManager.FindSceneInfo(SceneManager.GetActiveScene().name);
			if (scene == null) Debug.LogError("This scene doesn't have a TCScene!");

			sceneCamera = GameObject.FindWithTag("SceneCamera");
			if (sceneCamera == null) Debug.LogError("No GameObject with a tag of SceneCamera!");
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

			Logger.Logger.Log($"Added player {playerId}.");
		}

		/// <summary>
		/// Removes a player
		/// </summary>
		/// <param name="playerId"></param>
		public static void RemovePlayer(string playerId)
		{
			Players.Remove(playerId);
			Logger.Logger.Log($"Removed player {playerId}");
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
}