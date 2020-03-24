using Core;
using Mirror;
using UI;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace Player
{
	public class PlayerSetup : NetworkBehaviour
	{
		[SerializeField] private Camera localCamera;

		[Header("Components to Destroy")] [SerializeField]
		private CapsuleCollider localCapsuleCollider;

		[Header("Player UI")] 
		[SerializeField] private GameObject clientUiPrefab;

		[Header("Char Controller Settings")] 
		[SerializeField] private float charControllerSlopeLimit = 45.0f;
		[SerializeField] private float charControllerStepOffset = 0.3f;
		[SerializeField] private float charControllerSkinWidth = 0.08f;
		[SerializeField] private float charControllerMinMoveDistance = 0.001f;
		[SerializeField] private float charControllerRadius = 0.5f;
		[SerializeField] private float charControllerHeight = 2f;
		[SerializeField] private Vector3 charControllerCenter = Vector3.zero;

		public override void OnStartLocalPlayer()
		{
			Logger.Log("Setting up my local player...");

			//Setup UI
			ClientUI clientUi = Instantiate(clientUiPrefab).GetComponent<ClientUI>();
			GetComponent<PlayerManager>().ClientUi = clientUi;
			clientUi.SetupUI(GetComponent<PlayerManager>());

			base.OnStartLocalPlayer();

			//Don't need a collider since the charController acts as one
			Destroy(localCapsuleCollider);

			gameObject.AddComponent<PlayerServerMessages>();

			//Character Controller
			CharacterController charController = gameObject.AddComponent<CharacterController>();
			charController.slopeLimit = charControllerSlopeLimit;
			charController.stepOffset = charControllerStepOffset;
			charController.skinWidth = charControllerSkinWidth;
			charController.minMoveDistance = charControllerMinMoveDistance;
			charController.radius = charControllerRadius;
			charController.height = charControllerHeight;
			charController.center = charControllerCenter;

			//Player Movement
			gameObject.AddComponent<PlayerMovement>();

			//Set up scene stuff
			GameManager.GetActiveSceneCamera().SetActive(false);
			localCamera.enabled = true;
			localCamera.gameObject.AddComponent<AudioListener>();
			
			//Player Input
			gameObject.AddComponent<PlayerInput>();

			//Lock the cursor
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			Logger.Log("Local player is ready!");
		}

		public override void OnStartClient()
		{
			GameManager.AddPlayer(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<PlayerManager>());

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