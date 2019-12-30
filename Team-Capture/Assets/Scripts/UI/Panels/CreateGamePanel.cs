using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	public class CreateGamePanel : MonoBehaviour
	{
		private List<TCScene> activeTCScenes;

		public Button cancelButton;
		public TMP_Dropdown mapsDropdown;

		private NetworkManager netManager;

		private void Start()
		{
			mapsDropdown.ClearOptions();

			activeTCScenes = TCScenesManager.GetAllEnabledTCScenes().ToList();

			List<string> scenes = activeTCScenes.Select(scene => scene.displayName).ToList();

			mapsDropdown.AddOptions(scenes);
			mapsDropdown.RefreshShownValue();

			netManager = NetworkManager.singleton;
		}

		/// <summary>
		/// Starts the server and connects a player
		/// </summary>
		public void CreateGame()
		{
			if (netManager.isNetworkActive)
			{
				StartCoroutine(QuitExistingGame(StartServer));
				return;
			}

			StartServer();
		}

		private void StartServer()
		{
			netManager.onlineScene = activeTCScenes[mapsDropdown.value].sceneName;
			netManager.StartHost();
		}

		private IEnumerator QuitExistingGame(Action doLast)
		{
			netManager.StopHost();

			yield return new WaitForSeconds(0.1f);

			doLast();
		}
	}
}