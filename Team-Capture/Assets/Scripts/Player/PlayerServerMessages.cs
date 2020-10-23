using Core;
using Core.Networking.Messages;
using Mirror;
using Pickups;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Player
{
	/// <summary>
	/// Handles messages from the server
	/// </summary>
	public class PlayerServerMessages : MonoBehaviour
	{
		private PlayerUIManager uiManager;

		private void Awake()
		{
			//Register all our custom messages
			NetworkClient.RegisterHandler<SetPickupStatus>(PickupMessage);
			NetworkClient.RegisterHandler<PlayerDiedMessage>(PlayerDiedMessage);

			uiManager = GetComponent<PlayerUIManager>();
		}

		private void OnDestroy()
		{
			//Unregister our custom messages on destroy
			NetworkClient.UnregisterHandler<SetPickupStatus>();
			NetworkClient.UnregisterHandler<PlayerDiedMessage>();
		}

		/// <summary>
		/// Player died message, for killfeed
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="message"></param>
		private void PlayerDiedMessage(NetworkConnection conn, PlayerDiedMessage message)
		{
			uiManager.AddKillfeedItem(message);
		}

		/// <summary>
		/// When a pickup changes status
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="status"></param>
		private static void PickupMessage(NetworkConnection conn, SetPickupStatus status)
		{
			string pickupDirectory = GameManager.GetActiveScene().pickupsParent + status.PickupName;
			GameObject pickup = GameObject.Find(pickupDirectory);
			if (pickup == null)
			{
				Logger.Error("Was told to change status of a pickup at `{@PickupDirectory}` that doesn't exist!", pickupDirectory);
				return;
			}

			Pickup pickupLogic = pickup.GetComponent<Pickup>();

			foreach (PickupMaterials pickupMaterial in pickupLogic.pickupMaterials)
			{
				pickupMaterial.meshToChange.material = status.IsActive ? pickupMaterial.pickupMaterial : pickupMaterial.pickupPickedUpMaterial;
			}
		}
	}
}