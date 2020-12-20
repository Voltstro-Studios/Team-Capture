using System;
using System.Collections;
using Team_Capture.Player;
using UnityEngine;

namespace Team_Capture.Pickups
{
	/// <summary>
	///     Base class for a something that can picked up, such as a weapon or a health pack
	/// </summary>
	public abstract class Pickup : MonoBehaviour
	{
		//TODO: We should create picked up variations of materials at runtime
		/// <summary>
		///     Version of the picked up version of the materials on the pickup
		/// </summary>
		[Tooltip("Version of the picked up version of the materials on the pickup")]
		public PickupMaterials[] pickupMaterials;

		/// <summary>
		///     How long this pickup takes to respawn
		/// </summary>
		[Tooltip("How long this pickup takes to respawn")]
		public float pickupRespawnTime = 4.0f;

		/// <summary>
		///     The radius of the trigger
		/// </summary>
		[Tooltip("The radius of the trigger")] [SerializeField]
		private float triggerRadius = 1.3f;

		/// <summary>
		///     The centre of the trigger
		/// </summary>
		[Tooltip("The centre of the trigger")] [SerializeField]
		private Vector3 triggerCenter = Vector3.zero;

		private bool isPickedUp;

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
		///     Sets up the collider (trigger) for the server
		/// </summary>
		public void SetupTrigger()
		{
			SphereCollider newCollider = gameObject.AddComponent<SphereCollider>();
			newCollider.isTrigger = true;
			newCollider.radius = triggerRadius;
			newCollider.center = triggerCenter;
		}

		/// <summary>
		///     Called when a <see cref="PlayerManager" /> interacts with the pickup
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OnPlayerPickup(PlayerManager player)
		{
			isPickedUp = true;

			//Deactivate the pickup and respawn it
			ServerPickupManager.DeactivatePickup(this);
			StartCoroutine(RespawnPickup());
		}

		/// <summary>
		///     Activates the pickup's GFX after <see cref="pickupRespawnTime" /> time is up
		/// </summary>
		/// <returns></returns>
		public IEnumerator RespawnPickup()
		{
			yield return new WaitForSeconds(pickupRespawnTime);

			ServerPickupManager.ActivatePickup(this);
			isPickedUp = false;
		}
	}

	[Serializable]
	public class PickupMaterials
	{
		public MeshRenderer meshToChange;

		public Material pickupMaterial;
		public Material pickupPickedUpMaterial;
	}
}