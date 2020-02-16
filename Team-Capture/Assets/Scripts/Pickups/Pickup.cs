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

		/// <summary>
		/// Sets up the collider (trigger) for the server
		/// </summary>
		public void SetupTrigger()
		{
			SphereCollider newCollider = gameObject.AddComponent<SphereCollider>();
			newCollider.isTrigger = true;
			newCollider.radius = triggerRadius;
			newCollider.center = triggerCenter;
		}

		/// <summary>
		/// Called when a collider enters the trigger
		/// </summary>
		/// <param name="other"></param>
		public void OnTriggerEnter(Collider other)
		{
			if(other.GetComponent<PlayerManager>() == null) return;

			OnPlayerPickup(other.GetComponent<PlayerManager>());
		}

		/// <summary>
		/// Called when a <see cref="PlayerManager"/> interacts with the pickup
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnPlayerPickup(PlayerManager player)
		{
			//Deactivate the pickup and respawn it
			ServerPickupManager.DeactivatePickup(gameObject);
			StartCoroutine(RespawnPickup());
		}

		/// <summary>
		/// Activates the pickup's GFX after <see cref="pickupRespawnTime"/> time is up
		/// </summary>
		/// <returns></returns>
		public IEnumerator RespawnPickup()
		{
			yield return new WaitForSeconds(pickupRespawnTime);

			ServerPickupManager.ActivatePickup(gameObject);
		}
	}
}
