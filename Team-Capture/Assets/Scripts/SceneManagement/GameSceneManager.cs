// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using JetBrains.Annotations;
using Team_Capture.Core;
using Team_Capture.Pooling;
using Team_Capture.Settings.Controllers;
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

            tracersEffectsPool = new GameObjectPool(activeScene.traceEffectPrefab);
            bulletHolePool = new GameObjectPool(activeScene.bulletHoleEffectPrefab);
            
            if(activeScene.overrideProfile != null)
                VolumeSettingsController.Instance.ApplyVolumeOverride(activeScene.overrideProfile);
        }

        protected override void SingletonDestroyed()
        {
            if(VolumeSettingsController.IsCurrentlyOverriden)
                VolumeSettingsController.Instance.RevertVolumeOverride();
        }
    }
}