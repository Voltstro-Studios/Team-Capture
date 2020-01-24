using System.Collections.Generic;
using System.Linq;
using Core.Logger;
using Player;
using SceneManagement;
using UnityEngine;

namespace Core
{
	public class GameManager : MonoBehaviour
	{
		private const string SceneCameraTag = "SceneCamera";

		/// <summary>
		/// The active <see cref="GameManager"/>
		/// </summary>
		public static GameManager Instance;

		/// <summary>
		/// The active <see cref="TCScene"/> that this <see cref="GameManager"/> is running on
		/// </summary>
		public TCScene scene;

		/// <summary>
		/// The primary <see cref="Camera"/> <see cref="GameObject"/> that is in this scene
		/// </summary>
		public GameObject sceneCamera;

		#region Gamemanager Handling Stuff

		private void Awake()
		{
			if (Instance != null)
			{
				Logger.Logger.Log("There is already an active GameManager running!", LogVerbosity.Error);
			}
			else
			{
				Instance = this;
				Setup();
			}
		}

		private void Setup()
		{
			scene = TCScenesManager.GetActiveScene();
			if (scene == null)
			{
				Logger.Logger.Log($"The scene '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}' doesn't have a TCScene assigned to it!", LogVerbosity.Error);
				return;
			}

			sceneCamera = GameObject.FindWithTag(SceneCameraTag);
			if (sceneCamera == null)
			{
				Logger.Logger.Log($"The scene {scene.scene} doesn't have a Camera with the tag `{SceneCameraTag}` assigned to it!", LogVerbosity.Error);
			}
		}

		/// <summary>
		/// Gets this scene's main camera
		/// </summary>
		/// <returns></returns>
		public static GameObject GetActiveSceneCamera() => Instance.sceneCamera;

		/// <summary>
		/// Gets the active <see cref="TCScene"/> running on this <see cref="GameManager"/>
		/// </summary>
		/// <returns></returns>
		public static TCScene GetActiveScene() => Instance.scene;

		#endregion

		#region Player Tracking

		private const string PlayerIdPrefix = "Player ";

		private static readonly Dictionary<string, PlayerManager> Players = new Dictionary<string, PlayerManager>();

		/// <summary>
		/// Adds a <see cref="PlayerManager"/>
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
		/// Removes a <see cref="PlayerManager"/> using their assigned ID
		/// </summary>
		/// <param name="playerId"></param>
		public static void RemovePlayer(string playerId)
		{
			Players.Remove(playerId);
			Logger.Logger.Log($"Removed player {playerId}");
		}

		/// <summary>
		/// Returns a <see cref="PlayerManager"/> using their assigned ID
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public static PlayerManager GetPlayer(string playerId)
		{
			return Players[playerId];
		}

		/// <summary>
		/// Gets all <see cref="PlayerManager"/>s
		/// </summary>
		/// <returns></returns>
		public static PlayerManager[] GetAllPlayers()
		{
			return Players.Values.ToArray();
		}

		#endregion
	}
}