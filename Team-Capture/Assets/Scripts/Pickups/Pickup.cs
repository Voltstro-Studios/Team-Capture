using System.Collections;
using Mirror;
using Team_Capture.Core.Networking;
using Team_Capture.Player;
using UnityEngine;

namespace Team_Capture.Pickups
{
	/// <summary>
	///     Base class for a something that can picked up, such as a weapon or a health pack
	/// </summary>
	internal abstract class Pickup : NetworkBehaviour
	{
		/// <summary>
		///     How long this pickup takes to respawn
		/// </summary>
		[Tooltip("How long this pickup takes to respawn")]
		public float pickupRespawnTime = 4.0f;

		/// <summary>
		///     The radius of the trigger
		/// </summary>
		[Tooltip("The radius of the trigger")] 
		[SerializeField] private float triggerRadius = 1.3f;

		[SyncVar] private bool isPickedUp;

		private void Start()
		{
			//TODO: Create picked up materials

			if (TCNetworkManager.IsServer)
			{
				SphereCollider newCollider = gameObject.AddComponent<SphereCollider>();
				newCollider.isTrigger = true;
				newCollider.radius = triggerRadius;
			}
		}

		/// <summary>
		///     Called when a collider enters the trigger
		/// </summary>
		/// <param name="other"></param>
		public void OnTriggerEnter(Collider other)
		{
			if (isPickedUp) return;

			if (other.GetComponent<PlayerManager>() == null) return;

			OnPlayerPickup(other.GetComponent<PlayerManager>());
		}

		/// <summary>
		///     Called when a <see cref="PlayerManager" /> interacts with the pickup
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OnPlayerPickup(PlayerManager player)
		{
			isPickedUp = true;

			//Deactivate the pickup and respawn it
			StartCoroutine(RespawnPickup());
		}

		/// <summary>
		///     Activates the pickup's GFX after <see cref="pickupRespawnTime" /> time is up
		/// </summary>
		/// <returns></returns>
		public IEnumerator RespawnPickup()
		{
			yield return new WaitForSeconds(pickupRespawnTime);

			isPickedUp = false;
		}
	}
}