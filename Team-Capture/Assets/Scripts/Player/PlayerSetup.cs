using Mirror;
using UnityEngine;

namespace Player
{
	public class PlayerSetup : NetworkBehaviour
	{
		[Header("Components to Destroy")]
		[SerializeField] private CapsuleCollider localCapsuleCollider;

		[Header("Components to Enable")]
		[SerializeField] private CharacterController localCharacterController;
		[SerializeField] private PlayerMovement localPlayerMovement;
		[SerializeField] private Camera localCamera;
		[SerializeField] private AudioListener localAudioListener;
		[SerializeField] private PlayerLook localPlayerLook;

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			Destroy(localCapsuleCollider);

			localCharacterController.enabled = true;
			localPlayerMovement.enabled = true;

			GameManager.Instance.sceneCamera.SetActive(false);

			localCamera.enabled = true;
			localAudioListener.enabled = true;
			localPlayerLook.enabled = true;

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
