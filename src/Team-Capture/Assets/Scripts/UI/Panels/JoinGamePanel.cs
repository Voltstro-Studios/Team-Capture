// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;
using Mirror;
using NetFabric.Hyperlinq;
using Team_Capture.AddressablesAddons;
using Team_Capture.Core.Networking;
using Team_Capture.Core.Networking.Discovery;
using Team_Capture.UI.Elements;
using Team_Capture.UI.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.Panels
{
    /// <summary>
    ///     A panel for joining games
    /// </summary>
    internal class JoinGamePanel : PanelBase
    {
        /// <summary>
        ///     The prefab for a server button
        /// </summary>
        [Tooltip("The prefab for a server button")]
        public GameObject serverItemPrefab;

        /// <summary>
        ///     The <see cref="Transform" /> for the server list
        /// </summary>
        public Transform serverListTransform;

        /// <summary>
        ///     Text for the current status
        /// </summary>
        public TextMeshProUGUI statusText;

        /// <summary>
        ///     The <see cref="JoiningServerPanel" /> panel
        /// </summary>
        public JoiningServerPanel joiningServerPanel;

        /// <summary>
        ///     Button for refreshing the list
        /// </summary>
        public Button refreshButton;

        public CachedLocalizedString searchingServersText;

        public CachedLocalizedString connectingToServerText;

        private readonly List<TCServerResponse> servers = new();
        private TCGameDiscovery gameDiscovery;

        private bool isConnecting;
        private MenuController menuController;

        private TCNetworkManager netManager;

        private void Awake()
        {
            netManager = TCNetworkManager.Instance;
            gameDiscovery = netManager.gameDiscovery;
            menuController = GetComponentInParent<MenuController>();
        }

        private void Start()
        {
            statusText.text = searchingServersText.Value;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            joiningServerPanel.gameObject.SetActive(false);

            if (gameDiscovery == null)
                return;

            //Add a listener to the game discovery for when a server is found
            gameDiscovery.OnServerFound.AddListener(AddServer);
            gameDiscovery.StartDiscovery();
        }

        private void OnDisable()
        {
            if (gameDiscovery == null) return;

            //Remove the game discovery for when a server is found
            gameDiscovery.OnServerFound.RemoveListener(AddServer);
            gameDiscovery.StopDiscovery();
            RefreshServerList();
        }

        /// <summary>
        ///     Refreshes the server by removing all servers
        /// </summary>
        public void RefreshServerList()
        {
            statusText.text = searchingServersText.Value;
            statusText.gameObject.SetActive(true);
        }

        private void ClearList()
        {
            //Remove all servers
            servers.Clear();
            for (int i = 0; i < serverListTransform.childCount; i++)
                Destroy(serverListTransform.GetChild(i).gameObject);
        }

        /// <summary>
        ///     Adds a <see cref="TCServerResponse" /> to the list of servers
        /// </summary>
        /// <param name="server"></param>
        public void AddServer(TCServerResponse server)
        {
            //We are connecting to a server...
            if (NetworkClient.isConnecting)
                return;

            //If the server already exists in the list then ignore it
            if (servers.AsValueEnumerable().Any(x => Equals(x.EndPoint, server.EndPoint)))
                return;

            //Add the server to the list
            servers.Add(server);
            AddServerItem(server);

            statusText.gameObject.SetActive(false);

            Logger.Debug("Found server at {Address}", server.EndPoint.Address);
        }

        /// <summary>
        ///     Connects to a server
        /// </summary>
        /// <param name="ip"></param>
        public void ConnectToServer(IPEndPoint ip)
        {
            joiningServerPanel.gameObject.SetActive(true);

            cancelButton.interactable = false;
            refreshButton.interactable = false;
            menuController.allowPanelToggling = false;

            ClearList();

            //Tell Mirror to connect to the server's IP
            netManager.networkAddress = ip.Address.ToString();
            netManager.StartClient();
            
            CheckConnection().Forget();

            //Set our status text
            statusText.gameObject.SetActive(true);
            statusText.text = string.Format(connectingToServerText.Value, ip);
        }

        private async UniTaskVoid CheckConnection()
        {
            await UniTask.Delay(25);
            
            //Wait while we are still connecting
            await UniTask.WaitWhile(() => NetworkClient.isConnecting);
            
            //We should be connected, so we should only see this if something fucked up
            joiningServerPanel.FailToJoin();
            RefreshServerList();
            cancelButton.interactable = true;
            refreshButton.interactable = true;
            menuController.allowPanelToggling = true;
            netManager.StopHost();
        }

        private void AddServerItem(TCServerResponse server)
        {
            //Instantiate a new button for a server
            GameObject newItem = Instantiate(serverItemPrefab, serverListTransform, false);
            newItem.GetComponent<JoinServerButton>().SetupConnectButton(server, () => ConnectToServer(server.EndPoint));
        }
    }
}