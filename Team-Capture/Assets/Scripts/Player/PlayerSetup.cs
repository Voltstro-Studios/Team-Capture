using Mirror;
using UnityEngine;

namespace Player
{
	public class PlayerSetup : NetworkBehaviour
	{
		[Header("Components to Destroy")]
		[SerializeField] private CapsuleCollider localCapsuleCollider = null;

		[Header("Components to Enable")]
		[SerializeField] private CharacterController localCharacterController = null;
		[SerializeField] private PlayerMovement localPlayerMovement = null;
		[SerializeField] private GameObject localCamera = null;

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			Destroy(localCapsuleCollider);

			localCharacterController.enabled = true;
			localPlayerMovement.enabled = true;

			GameManager.Instance.sceneCamera.SetActive(false);

			localCamera.SetActive(true);

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		public override void OnStartClient()
		{
			base.OnStartClient();

			GameManager.AddPlayer(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<PlayerManager>());
		}

		private void OnDisable()
		{
			GameManager.RemovePlayer(transform.name);

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
	}
}
