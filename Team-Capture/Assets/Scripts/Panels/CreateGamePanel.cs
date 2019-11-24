using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SceneManagement;
using TMPro;
using Mirror;

namespace Panels
{
	public class CreateGamePanel : MonoBehaviour
	{
		public TMP_Dropdown mapsDropdown;

		public Button cancelButton;

		private NetworkManager netManager;

		private List<TCScene> activeTCScenes;

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
			netManager.onlineScene = activeTCScenes[mapsDropdown.value].sceneName;
			netManager.StartHost();
		}
	}
}
