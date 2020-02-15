using System.Collections.Generic;
using Core.Networking.Messages;
using Mirror;
using UnityEngine;

namespace Pickups
{
	public class ServerPickupManager : MonoBehaviour
	{
		private static readonly List<string> UnActivePickups = new List<string>();

		public static string[] GetUnActivePickups()
		{
			return UnActivePickups.ToArray();
		}

		public static void ClearUnActivePickupsList()
		{
			UnActivePickups.Clear();
		}

		public static void DeactivatePickup(GameObject pickup)
		{
			UnActivePickups.Add(pickup.name);

			NetworkServer.SendToAll(new SetPickupStatus
			{
				PickupName = pickup.name,
				IsActive = false
			});
		}

		public static void ActivatePickup(GameObject pickup)
		{
			UnActivePickups.Remove(pickup.name);

			NetworkServer.SendToAll(new SetPickupStatus
			{
				PickupName = pickup.name,
				IsActive = true
			});
		}
	}
}
