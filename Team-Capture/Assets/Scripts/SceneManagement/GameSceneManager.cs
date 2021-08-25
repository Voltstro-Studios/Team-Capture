// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using JetBrains.Annotations;
using Team_Capture.Core;
using Team_Capture.Pooling;
using Team_Capture.UI.ImGui;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.SceneManagement
{
    /// <summary>
    ///     Manager for scene related stuff
    /// </summary>
    public class GameSceneManager : SingletonMonoBehaviour<GameSceneManager>
    {
        #region TCScene

        private TCScene activeScene;

        /// <summary>
        ///     The associated <see cref="TCScene"/> for this scene
        /// </summary>
        public TCScene ActiveScene => activeScene;

        public static TCScene GetActiveScene()
        {
            return Instance.activeScene;
        }

        #endregion

        #region Scene Camera

        private const string SceneCameraTag = "SceneCamera";
        
        private Camera sceneCamera;

        /// <summary>
        ///     Switches to or from scene camera to <see cref="camera"/>
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="sceneCamera"></param>
        public static void SwitchCameras(Camera camera, bool sceneCamera)
        {
            GameSceneManager sceneManager = Instance;
            if(sceneManager == null)
                return;
            
            if (sceneCamera)
            {
                camera.gameObject.SetActive(false);
                sceneManager.sceneCamera.gameObject.SetActive(true);
                if (!Game.IsHeadless)
                {
                    ImGuiInstanceManager.Instance.SetInstanceCamera(Instance.sceneCamera);
                }
                return;
            }
            
            camera.gameObject.SetActive(true);
            sceneManager.sceneCamera.gameObject.SetActive(false);
            if (!Game.IsHeadless)
            {
                ImGuiInstanceManager.Instance.SetInstanceCamera(camera);
            }
        }

        #endregion

        #region Pools

        internal GameObjectPool tracersEffectsPool;
        internal GameObjectPool bulletHolePool;

        #endregion

        protected override bool DoDestroyOnLoad => true;

        protected override void SingletonAwakened()
        {
            activeScene = TCScenesManager.GetActiveScene();
            if (activeScene == null)
            {
                Logger.Error("The scene '{Scene}' doesn't have a TCScene assigned to it!", SceneManager.GetActiveScene().name);
                return;
            }

            GameObject foundSceneCamera = GameObject.FindWithTag(SceneCameraTag);
            if (foundSceneCamera == null)
                Logger.Error(
                    "The scene {Scene} doesn't have a Camera with the tag `{SceneCameraTag}` assigned to it!",
                    activeScene.scene, SceneCameraTag);

            sceneCamera = foundSceneCamera.GetComponent<Camera>();
            if(sceneCamera == null)
                Logger.Error("The scene {Scene} doesn't have a Camera attached to the object!", activeScene.scene);
            
            tracersEffectsPool = new GameObjectPool(activeScene.traceEffectPrefab);
            bulletHolePool = new GameObjectPool(activeScene.bulletHoleEffectPrefab);
        }
    }
}