// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Logging;
using Team_Capture.Pooling;
using UnityEngine.SceneManagement;

namespace Team_Capture.SceneManagement
{
    /// <summary>
    ///     Manager for scene related stuff
    /// </summary>
    public class GameSceneManager : SingletonMonoBehaviour<GameSceneManager>
    {
        protected override bool DoDestroyOnLoad => true;

        protected override void SingletonAwakened()
        {
            ActiveScene = TCScenesManager.GetActiveScene();
            if (ActiveScene == null)
            {
                Logger.Error("The scene '{Scene}' doesn't have a TCScene assigned to it!",
                    SceneManager.GetActiveScene().name);
                return;
            }

            tracersEffectsPool = new GameObjectPool(ActiveScene.traceEffectPrefab);
            bulletHolePool = new GameObjectPool(ActiveScene.bulletHoleEffectPrefab);
        }

        #region TCScene

        /// <summary>
        ///     The associated <see cref="TCScene" /> for this scene
        /// </summary>
        public TCScene ActiveScene { get; private set; }

        public static TCScene GetActiveScene()
        {
            return Instance.ActiveScene;
        }

        #endregion

        #region Pools

        internal GameObjectPool tracersEffectsPool;
        internal GameObjectPool bulletHolePool;

        #endregion
    }
}