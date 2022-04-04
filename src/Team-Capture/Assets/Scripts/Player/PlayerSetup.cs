// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Cinemachine;
using Mirror;
using Team_Capture.Core;
using Team_Capture.UI;
using UnityEngine;

namespace Team_Capture.Player
{
    /// <summary>
    ///     Handles setting up the player
    /// </summary>
    [DefaultExecutionOrder(1000)]
    internal sealed class PlayerSetup : NetworkBehaviour
    {
        /// <summary>
        ///     Player's local <see cref="Camera" />
        /// </summary>
        [Tooltip("Player's VCamera")] 
        [field: SerializeField]
        //Note: It is set by in the Unity editor as it is marked with SerializeField
        //ReSharper disable once UnusedAutoPropertyAccessor.Local
        public CinemachineVirtualCamera PlayerVCam { get; private set; }

        /// <summary>
        ///     The prefab for the client's UI
        /// </summary>
        [Header("Player UI")] [Tooltip("The prefab for the client's UI")] [SerializeField]
        private GameObject clientUiPrefab;

        [SerializeField] private MeshRenderer[] gfxMeshes;

        /// <summary>
        ///     This client's <see cref="PlayerManager" />
        /// </summary>
        private PlayerManager playerManager;

        public override void OnStartLocalPlayer()
        {
            //Setup UI
            ClientUI clientUi = Instantiate(clientUiPrefab).GetComponent<ClientUI>();
            clientUi.SetupUI(playerManager);
            gameObject.AddComponent<PlayerUIManager>().Setup(clientUi);

            //Allows for custom messages
            gameObject.AddComponent<PlayerServerMessages>();

            foreach (MeshRenderer gfxMesh in gfxMeshes)
                gfxMesh.enabled = false;

            //Set up scene stuff
            PlayerVCam.enabled = true;

            //Player Input
            gameObject.AddComponent<PlayerInputManager>().Setup();

            //Lock the cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        #region Unity Methods

        private void Awake()
        {
            playerManager = GetComponent<PlayerManager>();
        }

        public void Start()
        {
            GameManager.AddPlayer(netId.ToString(), GetComponent<PlayerManager>());

            //Setup player camera effects
            if (isLocalPlayer || isServer)
            {
                PlayerCameraEffects cameraEffects = PlayerVCam.gameObject.AddComponent<PlayerCameraEffects>();
                cameraEffects.Setup(transform, isServer);
            }
        }

        private void OnDisable()
        {
            //Remove this player from the GameManger
            GameManager.RemovePlayer(transform.name);

            if (!isLocalPlayer) return;

            //Unlock the cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            PlayerVCam.enabled = false;
        }

        #endregion
    }
}