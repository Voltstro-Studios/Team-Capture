using Core;
using Core.Networking.Messages;
using Mirror;
using Pickups;
using UI;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Player
{
	/// <summary>
	/// Handles messages from the server
	/// </summary>
	public class PlayerServerMessages : MonoBehaviour
	{
		/// <summary>
		/// The <see cref="ClientUI"/> that is associated with this client
		/// </summary>
		private ClientUI clientUi;

		private void Awake()
		{
			clientUi =  GetComponent<PlayerManager>().ClientUi;

			//Register all our custom messages
			NetworkClient.RegisterHandler<SetPickupStatus>(PickupMessage);
			NetworkClient.RegisterHandler<PlayerDiedMessage>(PlayerDiedMessage);
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
			clientUi.killFeed.AddFeedBackItem(message);
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