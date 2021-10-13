﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Team_Capture.Core.Networking;
using Team_Capture.SceneManagement;
using Team_Capture.UI.Menus;
using Team_Capture.UserManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.Panels
{
	/// <summary>
	///     The code driving the create server panel
	/// </summary>
	internal class CreateServerPanel : PanelBase
	{
		/// <summary>
		///     Dropdown for the maps
		/// </summary>
		[Tooltip("Dropdown for the maps")] public TMP_Dropdown mapsDropdown;

		/// <summary>
		///     Input for the game name
		/// </summary>
		[Tooltip("Input for the game name")] public TMP_InputField gameNameText;

		/// <summary>
		///		The button the user clicks to start the server
		/// </summary>
		[Tooltip("The button the user clicks to start the server")]
		public Button startServerButton;

		/// <summary>
		///     Input for the max amount of players
		/// </summary>
		[Tooltip("Input for the max amount of players")]
		public TMP_InputField maxPlayersText;

		/// <summary>
		///		Dropdown for Auth mode
		/// </summary>
		[Tooltip("Dropdown for Auth mode")]
		public TMP_Dropdown authModeDropdown;

		/// <summary>
		///		Shut the server when the player disconnects
		/// </summary>
		[Tooltip("Shut the server when the player disconnects")]
		public Toggle shutOnDisconnectToggle;

		/// <summary>
		///     The color to use when there is an error
		/// </summary>
		[Tooltip("The color to use when there is an error")]
		public Color errorColor = Color.red;

		/// <summary>
		///		The panel that is displayed when starting a server
		/// </summary>
		[Tooltip("The panel that is displayed when starting a server")]
		public CreatingServerPanel onStartingServerPanel;

		private Image gameNameImage;

		private Color gameNameImageColor;

		private int maxPlayers = 16;
		private Image maxPlayersImage;
		private Color maxPlayersImageColor;

		private MenuController menuController;
		private TCNetworkManager netManager;
		private List<TCScene> onlineTCScenes;

		private void Start()
		{
			onStartingServerPanel.gameObject.SetActive(false);

			//First, clear the maps dropdown
			mapsDropdown.ClearOptions();

			//Then get all online scenes
			onlineTCScenes = TCScenesManager.GetAllEnabledOnlineScenesInfo().ToList();

			//And all the scenes to the map dropdown
			List<string> scenes = onlineTCScenes.Select(scene => scene.DisplayNameLocalized).ToList();
			mapsDropdown.AddOptions(scenes);
			mapsDropdown.RefreshShownValue();

			//Get active network manager
			netManager = TCNetworkManager.Instance;

			//Get the images that are in the input fields
			gameNameImage = gameNameText.GetComponent<Image>();
			maxPlayersImage = maxPlayersText.GetComponent<Image>();

			//Get the existing colors of the input fields
			gameNameImageColor = gameNameImage.color;
			maxPlayersImageColor = maxPlayersImage.color;

			menuController = GetComponentInParent<MenuController>();

			string[] names = Enum.GetNames(typeof(UserProvider));
			authModeDropdown.ClearOptions();
			authModeDropdown.AddOptions(names.ToList());
			authModeDropdown.RefreshShownValue();
		}

		/// <summary>
		///     Starts the server and connects a player
		/// </summary>
		public void CreateGame()
		{
#if UNITY_EDITOR
			//If we are running as the editor, then we to check to see if an existing build already exists and use that instead
			if (!Directory.Exists($"{Voltstro.UnityBuilder.Build.GameBuilder.GetBuildDirectory()}Team-Capture-Quick/"))
			{
				Debug.LogError("There is no pre-existing build of Team-Capture! Build the game using VoltBuild.");
				return;
			}
#endif

			//Make sure the game name isn't white space or null
			if (string.IsNullOrWhiteSpace(gameNameText.text))
			{
				Logger.Error("Game name input is white space or null!");
				gameNameImage.color = errorColor;
				return;
			}

			//Make sure the max players input is actually a number
			if (int.TryParse(maxPlayersText.text, out int result))
			{
				//Make sure max players is greater then 1
				if (result <= 1)
				{
					Logger.Error("Max players must be greater then one!");
					maxPlayersImage.color = errorColor;
					return;
				}

				maxPlayers = result;
			}
			else //Display an error if is not a number
			{
				Logger.Error("Max players input isn't just an int!");
				maxPlayersImage.color = errorColor;
				return;
			}

			if (netManager.isNetworkActive)
			{
				StartCoroutine(QuitExistingGame(CreateServerProcess));
				return;
			}

			CreateServerProcess();
		}

		private void CreateServerProcess()
		{
			onStartingServerPanel.gameObject.SetActive(true);
			startServerButton.interactable = false;
			cancelButton.interactable = false;
			menuController.allowPanelToggling = false;

			UserProvider userProvider = (UserProvider)authModeDropdown.value;

			//Now start the server
			netManager.CreateServerProcess(gameNameText.text, onlineTCScenes[mapsDropdown.value].SceneFileName, maxPlayers, userProvider,
				shutOnDisconnectToggle.isOn,
				() => ConnectToCreatedServer().Forget(),
				() =>
				{
					EnableElements();
					onStartingServerPanel.FailedToStartMessage();
				});
		}

		private IEnumerator QuitExistingGame(Action doLast)
		{
			netManager.StopHost();

			yield return new WaitForSeconds(0.1f);

			doLast();
		}

		private async UniTask ConnectToCreatedServer()
		{
			await Integrations.UniTask.UniTask.Delay(1000);
			
			netManager.networkAddress = "localhost";
			netManager.StartClient();

			while (Client.Status == ClientStatus.Connecting)
			{
				await Integrations.UniTask.UniTask.Delay(100);
			}

			//We failed to connect
			if (Client.Status == ClientStatus.Offline)
			{
				EnableElements();
				onStartingServerPanel.FailedToConnectMessage();
				Logger.Error("Failed to connect!");
			}
		}

		private void EnableElements()
		{
			menuController.allowPanelToggling = true;
			cancelButton.interactable = true;
			startServerButton.interactable = true;
		}
		
		public void ResetGameNameTextColor()
		{
			if (gameNameImage.color == errorColor)
				gameNameImage.color = gameNameImageColor;
		}

		public void ResetMaxPlayersTextColor()
		{
			if (maxPlayersImage.color == errorColor)
				maxPlayersImage.color = maxPlayersImageColor;
		}
	}
}