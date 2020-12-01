using Core;
using Mirror;
using UI;
using UnityEngine;

namespace Player
{
	/// <summary>
	///     Handles setting up the player
	/// </summary>
	internal sealed class PlayerSetup : NetworkBehaviour
	{
		/// <summary>
		///     Player's local <see cref="Camera" />
		/// </summary>
		[Tooltip("Player's local Camera")] [SerializeField]
		private Camera localCamera;

		/// <summary>
		///     The prefab for the client's UI
		/// </summary>
		[Header("Player UI")] [Tooltip("The prefab for the client's UI")] [SerializeField]
		private GameObject clientUiPrefab;

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

			base.OnStartLocalPlayer();

			//Set up scene stuff
			GameManager.GetActiveSceneCamera().SetActive(false);
			localCamera.enabled = true;
			localCamera.gameObject.AddComponent<AudioListener>();

			//Player Input
			gameObject.AddComponent<PlayerInput>();

			//Lock the cursor
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		/// <summary>
		///     Gets this player's local <see cref="Camera" />
		/// </summary>
		/// <returns></returns>
		public Camera GetPlayerCamera()
		{
			return localCamera;
		}

		#region Unity Methods

		private void Awake()
		{
			playerManager = GetComponent<PlayerManager>();
		}

		public void Start()
		{
			GameManager.AddPlayer(netId.ToString(), GetComponent<PlayerManager>());
		}

		private void OnDisable()
		{
			//Remove this player from the GameManger
			GameManager.RemovePlayer(transform.name);

			if (!isLocalPlayer) return;

			//Unlock the cursor
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			//Go back to the scene camera
			if (GameManager.Instance == null)
				return;

			GameManager.GetActiveSceneCamera().SetActive(true);
		}

		#endregion
	}
}