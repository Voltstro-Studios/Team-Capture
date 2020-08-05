using Core;
using Mirror;
using UI;
using UnityEngine;

namespace Player
{
	public class PlayerSetup : NetworkBehaviour
	{
		[SerializeField] private Camera localCamera;

		[Header("Player UI")] 
		[SerializeField] private GameObject clientUiPrefab;

		private PlayerManager playerManager;

		private void Awake()
		{
			playerManager = GetComponent<PlayerManager>();
		}

		public override void OnStartLocalPlayer()
		{
			//Setup UI
			ClientUI clientUi = Instantiate(clientUiPrefab).GetComponent<ClientUI>();
			playerManager.ClientUi = clientUi;
			clientUi.SetupUI(playerManager);

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

		public override void OnStartClient()
		{
			GameManager.AddPlayer(netId.ToString(), GetComponent<PlayerManager>());

			base.OnStartClient();
		}

		private void OnDisable()
		{
			GameManager.RemovePlayer(transform.name);

			if (!isLocalPlayer) return;

			GameManager.GetActiveSceneCamera().SetActive(true);

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		public Camera GetPlayerCamera()
		{
			return localCamera;
		}
	}
}