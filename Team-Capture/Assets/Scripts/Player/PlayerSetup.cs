using Core;
using Core.Logger;
using Core.Networking.Messages;
using Mirror;
using Pickups;
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

			base.OnStartLocalPlayer();

			//Don't need a collider since the charController acts as one
			Destroy(localCapsuleCollider);

			//Register our handler for pickups
			NetworkClient.RegisterHandler<SetPickupStatus>(PickupMessage);

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

			//Setup UI
			ClientUI clientUi = Instantiate(clientUiPrefab).GetComponent<ClientUI>();
			GetComponent<PlayerManager>().clientUi = clientUi;
			clientUi.SetupUi(GetComponent<PlayerManager>());

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

			NetworkClient.UnregisterHandler<SetPickupStatus>();

			GameManager.GetActiveSceneCamera().SetActive(true);

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		public Camera GetPlayerCamera()
		{
			return localCamera;
		}

		#region Pickups

		private void PickupMessage(NetworkConnection conn, SetPickupStatus status)
		{
			string pickupDirectory = GameManager.GetActiveScene().pickupsParent + status.PickupName;
			GameObject pickup = GameObject.Find(pickupDirectory);
			if (pickup == null)
			{
				Logger.Log($"Was told to change status of a pickup at `{pickupDirectory}` that doesn't exist!", LogVerbosity.Error);
				return;
			}

			pickup.GetComponent<Pickup>().pickupGfx.SetActive(status.IsActive);
		}

		#endregion
	}
}