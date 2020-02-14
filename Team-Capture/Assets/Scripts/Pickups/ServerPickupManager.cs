using System.Collections.Generic;
using UnityEngine;

namespace Pickups
{
	public class ServerPickupManager : MonoBehaviour
	{
		private static List<GameObject> UnActivePickups = new List<GameObject>();

		public static GameObject[] GetUnActivePickups()
		{
			return UnActivePickups.ToArray();
		}

		public static void ClearUnActivePickupsList()
		{
			UnActivePickups.Clear();
		}

		public static void DeActivePickup(GameObject pickup)
		{
			UnActivePickups.Add(pickup);

			pickup.SetActive(false);
		}

		public static void ActivatePickup(GameObject pickup)
		{
			UnActivePickups.Remove(pickup);

			pickup.SetActive(true);
		}
	}
}
