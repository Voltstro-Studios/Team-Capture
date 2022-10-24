// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using NetFabric.Hyperlinq;
using Team_Capture.AddressablesAddons;
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

            gameObjectPools = new KeyValuePair<CachedAddressable<GameObject>, GameObjectPoolBase>[ActiveScene.pooledObjects.Count];
            for (int i = 0; i < ActiveScene.pooledObjects.Count; i++)
            {
                KeyValuePair<CachedAddressable<GameObject>, bool> pooledObject = ActiveScene.pooledObjects
                    .AsValueEnumerable()
                    .ElementAt(i).Value;

                gameObjectPools[i] = pooledObject.Value
                    ? KeyValuePair.Create<CachedAddressable<GameObject>, GameObjectPoolBase>(pooledObject.Key,
                        new NetworkProjectileObjectsPool(pooledObject.Key.Value))
                    : KeyValuePair.Create<CachedAddressable<GameObject>, GameObjectPoolBase>(pooledObject.Key,
                        new GameObjectPool(pooledObject.Key.Value));
            }
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

        private KeyValuePair<CachedAddressable<GameObject>, GameObjectPoolBase>[] gameObjectPools;

        public GameObjectPoolBase GetPoolByObject(CachedAddressable<GameObject> objectToGet)
        {
            //ReSharper doesn't seem to pickup on it's NetFabric.Hyperlinq
            // ReSharper disable once ReplaceWithSingleCallToFirst
            Option<KeyValuePair<CachedAddressable<GameObject>, GameObjectPoolBase>> pool = gameObjectPools
                .AsValueEnumerable()
                .Where(x => x.Key.Equals(objectToGet)).First();

            return pool.Value.Value;
        }

        #endregion
    }
}