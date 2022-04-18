// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.AddressablesAddons;
using Team_Capture.Console;
using Team_Capture.Core.Networking.Discovery;
using Team_Capture.LagCompensation;
using Team_Capture.SceneManagement;
using UnityCommandLineParser;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
    /// <summary>
    ///     The networking manager for Team-Capture
    /// </summary>
    [RequireComponent(typeof(TCGameDiscovery))]
    internal class TCNetworkManager : NetworkManager
    {
        /// <summary>
        ///     The active <see cref="TCNetworkManager" />
        /// </summary>
        public static TCNetworkManager Instance;

        [CommandLineArgument("connect")] private static string autoConnectIpAddress;

        /// <summary>
        ///     The prefab for the <see cref="GameManager" />
        /// </summary>
        [Header("Team Capture")] [Tooltip("The prefab for the GameManager")]
        public GameObject gameMangerPrefab;

        /// <summary>
        ///     The prefab for the <see cref="GameSceneManager" />
        /// </summary>
        [Tooltip("The prefab for the GameSceneManager")]
        public GameObject gameSceneManagerPrefab;

        /// <summary>
        ///     The prefab for the MOTD
        /// </summary>
        [Tooltip("The prefab for the MOTD")] public GameObject motdUIPrefab;

        /// <summary>
        ///     How many frames to keep
        /// </summary>
        [Tooltip("How many frames to keep")] 
        public float secondsHistory = 4f;

        /// <summary>
        ///     Addressables that can be spawnable
        /// </summary>
        public CachedAddressable<GameObject>[] registeredSpawnableAddressables;

        /// <summary>
        ///     The active <see cref="TCGameDiscovery" />
        /// </summary>
        [HideInInspector] public TCGameDiscovery gameDiscovery;

        /// <summary>
        ///     The config for the server
        /// </summary>
        [NonSerialized] public ServerConfig serverConfig;

        /// <summary>
        ///     Team-Capture's authenticator
        /// </summary>
        [NonSerialized] public TCAuthenticator tcAuthenticator;

        /// <summary>
        ///     Are we a server or not
        /// </summary>
        public static bool IsServer
        {
            get
            {
                if (Instance == null)
                    return false;

                return Instance.mode == NetworkManagerMode.ServerOnly;
            }
        }

        public static TCAuthenticator Authenticator
        {
            get
            {
                if (Instance == null)
                    throw new ArgumentNullException();

                return Instance.tcAuthenticator;
            }
        }

        public override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            //Replace Mirror's logger with ours
            //LogFactory.ReplaceLogHandler(new TCUnityLogger());

            base.Awake();

            Instance = this;
            tcAuthenticator = GetComponent<TCAuthenticator>();
        }

        public override void Start()
        {
            foreach (CachedAddressable<GameObject> addressable in registeredSpawnableAddressables)
            {
                NetworkClient.RegisterPrefab(addressable);
            }
            
            //We are running in headless mode
            if (Game.IsHeadless && !Game.IsGameQuitting)
            {
                //Start the server
                StartServer();
            }

            if (!Game.IsHeadless && autoConnectIpAddress != null)
            {
                WaitAndThenConnect().Forget();
            }
        }

        private async UniTaskVoid WaitAndThenConnect()
        {
            await UniTask.Delay(1000);
            
            await UniTask.SwitchToMainThread();
            
            networkAddress = autoConnectIpAddress;
            StartClient();
        }

        public void Update()
        {
            if (mode == NetworkManagerMode.ServerOnly)
            {
                PingManager.ServerPingUpdate();
                LagCompensationManager.ServerUpdate();
            }
        }

        public int GetMaxFramePoints()
        {
            return (int) (secondsHistory / (1f / serverTickRate));
        }

        [ConCommand("stop", "Stops the current game, whether that is disconnecting or stopping the server")]
        public static void StopCommand(string[] args)
        {
            NetworkManager networkManager = singleton;
            if (networkManager != null)
            {
                if (networkManager.mode == NetworkManagerMode.Offline)
                {
                    Logger.Error("You are not in a game!");
                    return;
                }

                networkManager.StopHost();
            }

            //Quit the game if we are headless
            if (Game.IsHeadless)
                Game.QuitGame();
        }

        #region Server Events

        public override void OnStartServer()
        {
            try
            {
                Server.OnStartServer(this);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Server failed to start!");
                if(Game.IsHeadless)
                    Game.QuitGame();
            }
        }

        public override void OnStopServer()
        {
            try
            {
                Server.OnStopServer();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Server failed to stop (lol)!");
                if(Game.IsHeadless) //If the server has failed to stop, well... then something has seriously fucked up, and we will force close
                    Environment.Exit(-1);
            }
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            Server.OnServerAddClient(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Server.OnServerRemoveClient(conn);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Server.ServerCreatePlayerObject(conn, playerPrefab);
        }

        public override void OnServerChangeScene(string newSceneName)
        {
            Server.OnServerSceneChanging(newSceneName);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            Server.OnServerChangedScene(sceneName);
        }

        #endregion

        #region Client Events

        public override void OnStartClient()
        {
            Client.OnClientStart(this);
        }

        public override void OnStopClient()
        {
            Client.OnClientStop();
        }

        public override void OnClientConnect()
        {
            Client.OnClientConnect(NetworkClient.connection);
        }

        public override void OnClientDisconnect()
        {
            Client.OnClientDisconnect(NetworkClient.connection);
        }

        public override void OnClientSceneChanged()
        {
            Client.OnClientSceneChanged();
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation,
            bool customHandling)
        {
            Client.OnClientSceneChanging(newSceneName);
        }

        #endregion
    }
}