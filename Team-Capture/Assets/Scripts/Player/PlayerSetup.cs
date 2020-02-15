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
		[SerializeField] private AudioListener localAudioListener;
		[SerializeField] private Camera localCamera;

		[Header("Components to Destroy")] [SerializeField]
		private CapsuleCollider localCapsuleCollider;

		[Header("Components to Enable")] [SerializeField]
		private CharacterController localCharacterController;

		[SerializeField] private PlayerInput localPlayerInput;
		[SerializeField] private PlayerMovement localPlayerMovement;

		[Header("Player UI")] [SerializeField] private GameObject clientUiPrefab;

		public override void OnStartLocalPlayer()
		{
			Logger.Log("Setting up player!");

			base.OnStartLocalPlayer();

			Destroy(localCapsuleCollider);

			//Register our handler for pickups
			NetworkClient.RegisterHandler<SetPickupStatus>(PickupMessage);

			localCharacterController.enabled = true;
			localPlayerMovement.enabled = true;

			GameManager.GetActiveSceneCamera().SetActive(false);

			localCamera.enabled = true;
			localAudioListener.enabled = true;
			localPlayerInput.enabled = true;

			//Setup UI
			ClientUI clientUi = Instantiate(clientUiPrefab).GetComponent<ClientUI>();
			GetComponent<PlayerManager>().clientUi = clientUi;
			clientUi.SetupUi(GetComponent<PlayerManager>());

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			Logger.Log("I am now ready! :)");
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