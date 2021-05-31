// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Core.Networking;
using UnityEngine;

namespace Team_Capture.Pickups
{
    [DisallowMultipleComponent]
    internal class PickupSpawner : MonoBehaviour
    {
	    public GameObject pickupToSpawn;

	    private void Start()
        {
	        if (TCNetworkManager.IsServer)
	        {
		        Transform pickupSpawnerTransform = transform;
		        GameObject newPickup = Instantiate(pickupToSpawn, pickupSpawnerTransform.position, pickupSpawnerTransform.rotation, pickupSpawnerTransform);
				NetworkServer.Spawn(newPickup);
	        }

			Destroy(this);
        }
    }
}