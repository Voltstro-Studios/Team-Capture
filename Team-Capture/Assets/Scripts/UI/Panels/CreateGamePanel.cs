using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Networking;
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
		private Image gameNameImage;

		public TMP_InputField maxPlayersText;
		private Image maxPlayersImage;

		public Color errorColor = Color.red;

		private int maxPlayers = 16;

		private TCNetworkManager netManager;

		private void Start()
		{
			mapsDropdown.ClearOptions();

			activeTCScenes = TCScenesManager.GetAllEnabledOnlineScenesInfo().ToList();

			List<string> scenes = activeTCScenes.Select(scene => scene.displayName).ToList();

			mapsDropdown.AddOptions(scenes);
			mapsDropdown.RefreshShownValue();

			netManager = TCNetworkManager.Instance;

			gameNameImage = gameNameText.GetComponent<Image>();
			maxPlayersImage = maxPlayersText.GetComponent<Image>();
		}

		/// <summary>
		/// Starts the server and connects a player
		/// </summary>
		public void CreateGame()
		{
			if (string.IsNullOrWhiteSpace(gameNameText.text))
			{
				gameNameImage.color = errorColor;
				return;
			}

			if (int.TryParse(maxPlayersText.text, out int result))
			{
				//Make sure max players is greater then 1
				if (result <= 1)
				{
					maxPlayersImage.color = errorColor;
					return;
				}

				maxPlayers = result;
			}
			else
			{
				maxPlayersImage.color = errorColor;
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
			netManager.onlineScene = activeTCScenes[mapsDropdown.value].scene;
			netManager.gameName = gameNameText.text;
			netManager.maxConnections = maxPlayers;
			netManager.StartHost();
		}

		private IEnumerator QuitExistingGame(Action doLast)
		{
			netManager.StopHost();

			yield return new WaitForSeconds(0.1f);

			doLast();
		}

		public void ResetGameNameTextColor()
		{
			if(gameNameImage.color == errorColor)
				gameNameText.GetComponent<Image>().color = Color.white;
		}

		public void ResetMaxPlayersTextColor()
		{
			if(maxPlayersImage.color == errorColor)
				maxPlayersImage.color = Color.white;
		}
	}
}