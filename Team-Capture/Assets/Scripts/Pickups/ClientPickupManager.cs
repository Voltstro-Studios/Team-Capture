using Mirror;
using Team_Capture.Core;
using Team_Capture.Core.Networking.Messages;
using UnityEngine;
using Logger = Team_Capture.Core.Logging.Logger;

namespace Team_Capture.Pickups
{
	/// <summary>
	///     Handles pickup related tasks for the client
	/// </summary>
	internal static class ClientPickupManager
	{
		/// <summary>
		///     Sets up the <see cref="ClientPickupManager" />
		/// </summary>
		public static void Setup()
		{
			NetworkClient.RegisterHandler<InitPickupStatusMessage>(SetPickupStatus);
		}

		/// <summary>
		///     Shutdown the <see cref="ClientPickupManager" />
		/// </summary>
		public static void Shutdown()
		{
			NetworkClient.UnregisterHandler<InitPickupStatusMessage>();
		}

		private static void SetPickupStatus(NetworkConnection conn, InitPickupStatusMessage message)
		{
			//Deactivate any deactivated pickups
			string pickupParent = GameManager.GetActiveScene().pickupsParent;
			foreach (string unActivePickup in message.DisabledPickups)
			{
				GameObject pickup = GameObject.Find(pickupParent + unActivePickup);
				if (pickup == null)
				{
					Logger.Error(
						"There was a pickup with the name `{@PickupName}` sent by the server that doesn't exist! Either the server's game is out of date or ours is!",
						pickup.name);
					continue;
				}

				Pickup pickupLogic = pickup.GetComponent<Pickup>();

				foreach (PickupMaterials pickupMaterials in pickupLogic.pickupMaterials)
					pickupMaterials.meshToChange.material = pickupMaterials.pickupPickedUpMaterial;
			}
		}
	}
}