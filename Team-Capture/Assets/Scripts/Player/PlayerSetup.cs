using UnityEngine;
using Mirror;

namespace Player
{
    public class PlayerSetup : NetworkBehaviour
    {
        [SerializeField] private AudioListener localAudioListener;
        [SerializeField] private Camera localCamera;

        [Header("Components to Destroy")] [SerializeField]
        private CapsuleCollider localCapsuleCollider;

        [Header("Components to Enable")] [SerializeField]
        private CharacterController localCharacterController;

        [SerializeField] private PlayerInput localPlayerInput;
        [SerializeField] private PlayerMovement localPlayerMovement;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            Destroy(localCapsuleCollider);

            localCharacterController.enabled = true;
            localPlayerMovement.enabled = true;

            GameManager.Instance.sceneCamera.SetActive(false);

            localCamera.enabled = true;
            localAudioListener.enabled = true;
            localPlayerInput.enabled = true;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void OnStartClient()
        {
	        GameManager.AddPlayer(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<PlayerManager>());

            base.OnStartClient();
        }

        private void OnDisable()
        {
            GameManager.RemovePlayer(transform.name);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public Camera GetPlayerCamera()
        {
	        return localCamera;
        }
    }
}