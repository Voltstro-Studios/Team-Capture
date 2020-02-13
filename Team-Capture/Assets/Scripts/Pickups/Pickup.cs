using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Pickups
{
	public class Pickup : MonoBehaviour
	{
		[SerializeField] private float triggerRadius = 1.3f;
		[SerializeField] private Vector3 triggerCenter = Vector3.zero;

		public OnPlayerPickupEvent playerPickupEvent = new OnPlayerPickupEvent();

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

			//Invoke the event
			playerPickupEvent.Invoke(other.GetComponent<PlayerManager>(), gameObject);
		}

		public class OnPlayerPickupEvent : UnityEvent<PlayerManager, GameObject> {}
	}
}
