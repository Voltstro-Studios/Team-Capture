using Cinemachine;
using Team_Capture.Input;
using UnityEngine;

namespace Team_Capture.Player
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    internal class PlayerDeathCam : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private float lookSensitivity = 100f;
        
        private CinemachineVirtualCamera virtualCamera;
        private PlayerManager playerManager;

        private float rotationX;
        private float rotationY;

        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        private void OnEnable()
        {
            inputReader.EnablePlayerDeathCamInput();
        }

        private void OnDisable()
        {
            rotationX = 0;
            rotationY = 0;
            inputReader.DisablePlayerDeathCamInput();
        }
        
        internal void Update()
        {
            if(playerManager != null)
            {
                if (playerManager.IsDead)
                    StopTrackingPlayer();
                
                return;
            }
            
            Vector2 look = inputReader.ReadPlayerDeathCamLook() * Time.deltaTime * lookSensitivity;
            rotationX -= look.y;
            rotationY += look.x;
            
            rotationX = Mathf.Clamp(rotationX, -90, 90);
            
            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        internal void StartTrackingPlayer(PlayerManager playerToTrack)
        {
            playerManager = playerToTrack;
            virtualCamera.m_LookAt = playerManager.transform;
        }
        
        internal void StopTrackingPlayer()
        {
            playerManager = null;
            virtualCamera.LookAt = null;
        }
    }
}
