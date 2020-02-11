using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	public class CreateGamePanel : MainMenuPanelBase
	{
		private List<TCScene> activeTCScenes;

		public TMP_Dropdown mapsDropdown;

		public TMP_InputField gameNameText;

		public Color gameNameErrorColor = Color.red;

		private TCNetworkManager netManager;

		private void Start()
		{
			mapsDropdown.ClearOptions();

			activeTCScenes = TCScenesManager.GetAllEnabledOnlineScenesInfo().ToList();

			List<string> scenes = activeTCScenes.Select(scene => scene.displayName).ToList();

			mapsDropdown.AddOptions(scenes);
			mapsDropdown.RefreshShownValue();

			netManager = TCNetworkManager.Instance;
		}

		/// <summary>
		/// Starts the server and connects a player
		/// </summary>
		public void CreateGame()
		{
			if (string.IsNullOrWhiteSpace(gameNameText.text))
			{
				gameNameText.GetComponent<Image>().color = gameNameErrorColor;
				return;
			}

			if (netManager.isNetworkActive)
			{
				StartCoroutine(QuitExistingGame(StartServer));
				return;
			}

			StartServer();
		}

		private void StartServer()
		{
			netManager.onlineScene = activeTCScenes[mapsDropdown.value];
			netManager.gameName = gameNameText.text;
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