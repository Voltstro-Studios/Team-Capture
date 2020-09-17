using Core;
using Core.Networking.Messages;
using Mirror;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Pickups
{
	public static class ClientPickupManager
	{
		public static void SetupClientPickupManager()
		{
			NetworkClient.RegisterHandler<PickupStatusMessage>(SetPickupStatus);
		}

		public static void ShutdownClient()
		{
			NetworkClient.UnregisterHandler<PickupStatusMessage>();
		}

		private static void SetPickupStatus(NetworkConnection conn, PickupStatusMessage message)
		{
			//Deactivate any deactivated pickups
			string pickupParent = GameManager.GetActiveScene().pickupsParent;
			foreach (string unActivePickup in message.DisabledPickups)
			{
				GameObject pickup = GameObject.Find(pickupParent + unActivePickup);
				if (pickup == null)
				{
					Logger.Error("There was a pickup with the name `{@PickupName}` sent by the server that doesn't exist! Either the server's game is out of date or ours is!", pickup.name);
					continue;
				}

				Pickup pickupLogic = pickup.GetComponent<Pickup>();

				foreach (PickupMaterials pickupMaterials in pickupLogic.pickupMaterials)
				{
					pickupMaterials.meshToChange.material = pickupMaterials.pickupPickedUpMaterial;
				}
			}
		}
	}
}