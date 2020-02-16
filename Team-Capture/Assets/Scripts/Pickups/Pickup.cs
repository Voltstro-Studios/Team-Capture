using System.Collections;
using Player;
using UnityEngine;

namespace Pickups
{
	public abstract class Pickup : MonoBehaviour
	{
		public GameObject pickupGfx;
		public float pickupRespawnTime = 4.0f;
		[SerializeField] private float triggerRadius = 1.3f;
		[SerializeField] private Vector3 triggerCenter = Vector3.zero;

		public void SetupTrigger()
		{
			SphereCollider newCollider = gameObject.AddComponent<SphereCollider>();
			newCollider.isTrigger = true;
			newCollider.radius = triggerRadius;
			newCollider.center = triggerCenter;
		}

		public void OnTriggerEnter(Collider other)
		{
			if(other.GetComponent<PlayerManager>() == null) return;

			OnPlayerPickup(other.GetComponent<PlayerManager>());
		}

		public virtual void OnPlayerPickup(PlayerManager player)
		{
			//Deactivate the pickup and respawn it
			ServerPickupManager.DeactivatePickup(gameObject);
			StartCoroutine(RespawnPickup());
		}

		public IEnumerator RespawnPickup()
		{
			yield return new WaitForSeconds(pickupRespawnTime);

			ServerPickupManager.ActivatePickup(gameObject);
		}
	}
}
