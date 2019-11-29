using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using SceneManagement;

namespace Panels
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
        ///     Starts the server and connects a player
        /// </summary>
        public void CreateGame()
        {
            netManager.onlineScene = activeTCScenes[mapsDropdown.value].sceneName;
            netManager.StartHost();
        }
    }
}