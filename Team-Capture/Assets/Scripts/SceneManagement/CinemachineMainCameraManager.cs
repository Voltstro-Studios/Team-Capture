using Cinemachine;
using Team_Capture.Console;
using Team_Capture.Core;
using Team_Capture.Settings;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.SceneManagement
{
    [RequireComponent(typeof(CinemachineBrain))]
    [RequireComponent(typeof(UniversalAdditionalCameraData))]
    public class CinemachineMainCameraManager : MonoBehaviour
    {
        [SerializeField] private bool handleAudioListeners = true;

        private UniversalAdditionalCameraData cameraData;

        private void Start()
        {
            if (Game.IsHeadless)
            {
                Destroy(this);
                return;
            }

            CinemachineBrain brain = GetComponent<CinemachineBrain>();
            if (handleAudioListeners)
                brain.m_CameraActivatedEvent.AddListener(OnCameraActivated);

            cameraData = GetComponent<UniversalAdditionalCameraData>();

            GameSettings.SettingsUpdated += UpdateSettings;

            UpdateSettings();
        }

        private void OnDestroy()
        {
            GameSettings.SettingsUpdated -= UpdateSettings;
        }

        private void OnCameraActivated(ICinemachineCamera toCam, ICinemachineCamera fromCam)
        {
            if (toCam == null)
                return;

            //To cam audio listener
            AudioListener toCamAudioListener = toCam.VirtualCameraGameObject.GetComponent<AudioListener>();
            if (toCamAudioListener != null)
                toCamAudioListener.enabled = true;

            if (fromCam == null)
                return;

            AudioListener fromCamAudioListener = fromCam.VirtualCameraGameObject.GetComponent<AudioListener>();
            if (fromCamAudioListener != null)
                fromCamAudioListener.enabled = false;
        }

        private void UpdateSettings()
        {
            cameraData.renderPostProcessing = GameSettings.AdvSettings.PostProcessing;
            cameraData.antialiasing = GameSettings.AdvSettings.CameraAntialiasing;
            cameraData.antialiasingQuality = GameSettings.AdvSettings.CameraAntialiasingQuality;
        }

        [ConCommand("r_antialiasing", "Changes the antialiasing mode", CommandRunPermission.ClientOnly, 1, 1, true)]
        public static void AntialiasingModeCommand(string[] args)
        {
            if (int.TryParse(args[0], out int modeIndex))
            {
                AntialiasingMode antialiasingMode = (AntialiasingMode) modeIndex;

                GameSettings.AdvSettings.CameraAntialiasing = antialiasingMode;
                GameSettings.Save();

                return;
            }

            Logger.Error("Invalid input!");
        }

        [ConCommand("r_antialiasing_quality", "Changes the antialiasing quality", CommandRunPermission.ClientOnly, 1, 1,
            true)]
        public static void AntialiasingQualityCommand(string[] args)
        {
            if (int.TryParse(args[0], out int qualityIndex))
            {
                AntialiasingQuality antialiasingQuality = (AntialiasingQuality) qualityIndex;

                GameSettings.AdvSettings.CameraAntialiasingQuality = antialiasingQuality;
                GameSettings.Save();

                return;
            }

            Logger.Error("Invalid input!");
        }
    }
}