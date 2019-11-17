using System.Collections.Generic;
using System.Linq;
using Mirror;
using SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

		public void CreateGame()
		{
			netManager.onlineScene = activeTCScenes[mapsDropdown.value].sceneName;
			netManager.StartHost();
		}
	}
}
