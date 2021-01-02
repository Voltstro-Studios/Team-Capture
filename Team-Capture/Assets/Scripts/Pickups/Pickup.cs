using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Team_Capture.Core;
using Team_Capture.Core.Networking;
using Team_Capture.Helper;
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

		[SyncVar(hook = nameof(OnIsPickedUp))] private bool isPickedUp;

		public List<PickupMaterials> pickupMaterials = new List<PickupMaterials>();

		private void Start()
		{
			if (TCNetworkManager.IsServer)
			{
				SphereCollider newCollider = gameObject.AddComponent<SphereCollider>();
				newCollider.isTrigger = true;
				newCollider.radius = triggerRadius;
			}

			if (Game.IsHeadless) return;

			GeneratePickupMaterials();
			SetMeshRenderMaterials();
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

		private void OnIsPickedUp(bool oldValue, bool newValue)
		{
			SetMeshRenderMaterials();
		}

		private void GeneratePickupMaterials()
		{
			//Get all mesh renderers
			MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in meshRenderers)
			{
				//Get the non-picked up material
				Material nonPickedUpMaterial = meshRenderer.material;
				Color nonPickedUpMaterialColor = nonPickedUpMaterial.color;

				//Copy it, and set it's color to have alpha
				Material pickedUpMaterial = new Material(nonPickedUpMaterial)
				{
					color = new Color(nonPickedUpMaterialColor.r, nonPickedUpMaterialColor.g,
						nonPickedUpMaterialColor.b, 0.4f)
				};
				pickedUpMaterial.ChangeMaterialTransparency(true);

				pickupMaterials.Add(new PickupMaterials
				{
					meshRenderer = meshRenderer,
					defaultMaterial = nonPickedUpMaterial,
					pickedUpMaterial = pickedUpMaterial
				});
			}
		}

		private void SetMeshRenderMaterials()
		{
			foreach (PickupMaterials pickupMaterial in pickupMaterials)
			{
				pickupMaterial.meshRenderer.material =
					isPickedUp ? pickupMaterial.pickedUpMaterial : pickupMaterial.defaultMaterial;
			}
		}

		[Serializable]
		public struct PickupMaterials
		{
			public MeshRenderer meshRenderer;

			public Material defaultMaterial;

			public Material pickedUpMaterial;
		}
	}
}