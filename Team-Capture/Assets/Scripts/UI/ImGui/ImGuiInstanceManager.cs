using System.Collections;
using Team_Capture.SceneManagement;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.ImGui
{
    [RequireComponent(typeof(UImGui.UImGui))]
    public class ImGuiInstanceManager : SingletonMonoBehaviour<ImGuiInstanceManager>
    {
        private UImGui.UImGui uImGui;
        private Camera uImguiCamera;

        public string sceneCameraTag = "SceneCamera";

        private bool firstCameraSet = true;
        
        protected override void SingletonAwakened()
        {
            TCScenesManager.PreparingSceneLoadEvent += OnScenePreparingToLoad;
            TCScenesManager.OnSceneLoadedEvent += OnSceneLoaded;
            
            uImGui = GetComponent<UImGui.UImGui>();
            SetInstanceCamera(Camera.main);
            EnableUImgui();
        }

        private void OnSceneLoaded(TCScene scene)
        {
            SetInstanceCamera(FindMainCamera());
            StartCoroutine(AfterSceneLoad());
        }

        private void OnScenePreparingToLoad(TCScene scene)
        {
            uImGui.enabled = false;
        }

        public void SetInstanceCamera(Camera renderCamera)
        {
            uImguiCamera = renderCamera;
            uImGui.SetCamera(renderCamera);
            if(!firstCameraSet)
                uImGui.Reload();

            firstCameraSet = false;
        }

        public void EnableUImgui()
        {
            if (uImguiCamera == null)
            {
                Logger.Warn("Not enabling UImGui, camera is null!");
                return;
            }

            uImGui.enabled = true;
        }

        private IEnumerator AfterSceneLoad()
        {
            yield return new WaitForEndOfFrame();

            Camera sceneDefaultCam = FindMainCamera();
            SetInstanceCamera(sceneDefaultCam);
            
            yield return new WaitForEndOfFrame();
            
            EnableUImgui();
        }

        private Camera FindMainCamera()
        {
            GameObject cameraObj = GameObject.FindWithTag(sceneCameraTag);
            
            //Fall back to main camera if we can't find the scene camera
            if (cameraObj == null)
            {
                Logger.Debug("Did not find scene camera! Falling back to Camera.main.");
                return Camera.main;
            }
            
            Camera foundCam = cameraObj.GetComponent<Camera>();
            if (foundCam == null)
            {
                Logger.Debug("Did not find scene camera! Falling back to Camera.main.");
                return Camera.main;
            }

            Logger.Debug("Found SceneCamera");
            return foundCam;
        }
    }
}
