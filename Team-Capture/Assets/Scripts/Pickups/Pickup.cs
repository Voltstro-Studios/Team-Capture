using Player;
using UnityEngine;

namespace Pickups
{
	public abstract class Pickup : MonoBehaviour
	{
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

		public abstract void OnPlayerPickup(PlayerManager player);
	}
}
