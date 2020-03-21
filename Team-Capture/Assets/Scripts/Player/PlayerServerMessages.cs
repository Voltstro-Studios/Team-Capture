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
	public class PlayerServerMessages : MonoBehaviour
	{
		private ClientUI clientUi;

		private void Start()
		{
			clientUi = GetComponent<PlayerManager>().clientUi;

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

		private void PlayerDiedMessage(NetworkConnection conn, PlayerDiedMessage message)
		{
			clientUi.killFeed.AddFeedBackItem(message);
		}

		private static void PickupMessage(NetworkConnection conn, SetPickupStatus status)
		{
			string pickupDirectory = GameManager.GetActiveScene().pickupsParent + status.PickupName;
			GameObject pickup = GameObject.Find(pickupDirectory);
			if (pickup == null)
			{
				Logger.Log($"Was told to change status of a pickup at `{pickupDirectory}` that doesn't exist!", LogVerbosity.Error);
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
