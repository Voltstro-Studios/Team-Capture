using System.Collections.Generic;
using Core.Networking.Messages;
using Mirror;
using UnityEngine;

namespace Pickups
{
	/// <summary>
	/// A static class for the server to manage pickups
	/// </summary>
	public static class ServerPickupManager
	{
		private static readonly List<string> UnActivePickups = new List<string>();

		/// <summary>
		/// Gets all un active pickups
		/// </summary>
		/// <returns></returns>
		public static string[] GetUnActivePickups()
		{
			return UnActivePickups.ToArray();
		}

		/// <summary>
		/// Removes all pickups from the list
		/// </summary>
		public static void ClearUnActivePickupsList()
		{
			UnActivePickups.Clear();
		}

		/// <summary>
		/// Deactivate a pickup
		/// </summary>
		/// <param name="pickup"></param>
		public static void DeactivatePickup(GameObject pickup)
		{
			UnActivePickups.Add(pickup.name);

			NetworkServer.SendToAll(new SetPickupStatus
			{
				PickupName = pickup.name,
				IsActive = false
			});
		}

		/// <summary>
		/// Activate a pickup
		/// </summary>
		/// <param name="pickup"></param>
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
