// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Pooling;
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
        
        private GameObject sceneCamera;

        /// <summary>
        ///     This scene's 'overview' camera
        /// </summary>
        public GameObject SceneCamera => sceneCamera;

        public static GameObject GetSceneCamera()
        {
            return Instance.sceneCamera;
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

            sceneCamera = GameObject.FindWithTag(SceneCameraTag);
            if (sceneCamera == null)
                Logger.Error(
                    "The scene {Scene} doesn't have a Camera with the tag `{SceneCameraTag}` assigned to it!",
                    activeScene.scene, SceneCameraTag);
            
            tracersEffectsPool = new GameObjectPool(activeScene.traceEffectPrefab);
            bulletHolePool = new GameObjectPool(activeScene.bulletHoleEffectPrefab);
        }
    }
}