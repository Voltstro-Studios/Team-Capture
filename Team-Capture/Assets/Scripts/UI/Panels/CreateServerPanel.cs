using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Networking;
using SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace UI.Panels
{
	public class CreateServerPanel : MainMenuPanelBase
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
#if UNITY_EDITOR
			if (!System.IO.Directory.Exists($"{VoltBuilder.BuildTool.GetBuildFolder()}Team-Capture-Quick/"))
			{
				Debug.LogError("There is no pre-existing build of Team-Capture! Build the game using VoltBuild.");
				return;
			}
#endif

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

			Process newTcServer = new Process
			{
				StartInfo = new ProcessStartInfo
				{
#if UNITY_EDITOR
					FileName = $"{VoltBuilder.BuildTool.GetBuildFolder()}Team-Capture-Quick/Team-Capture.exe",
#elif UNITY_STANDALONE_WIN
					FileName = "Team-Capture.exe",
#else
					FileName = "Team-Capture",
#endif
					Arguments = "-batchmode -nographics"
				}
			};
			newTcServer.Start();

			netManager.StartClient();
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