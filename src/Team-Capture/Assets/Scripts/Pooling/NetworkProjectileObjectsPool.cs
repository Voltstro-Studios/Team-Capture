// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.Weapons.Projectiles;
using UnityEngine;
using UnityEngine.Pool;
using Logger = Team_Capture.Logging.Logger;
using Object = UnityEngine.Object;

namespace Team_Capture.Pooling
{
    public sealed class NetworkProjectileObjectsPool
    {
        private readonly IObjectPool<GameObject> objectPool;

        public NetworkProjectileObjectsPool(GameObject prefab)
        {
            objectPool = new LinkedPool<GameObject>(() => CreateObject(prefab), OnTakeObject, OnReturnObject,
                OnDestroyObject);
        }

        public GameObject GetPooledObject()
        {
            return objectPool.Get();
        }

        public void ReturnPooledObject(GameObject gameObject)
        {
            objectPool.Release(gameObject);
        }

        private GameObject CreateObject(GameObject prefab)
        {
            GameObject newObj = Object.Instantiate(prefab);
            ProjectileBase projectileBase = newObj.GetComponent<ProjectileBase>();
            if (projectileBase == null)
            {
                const string errorMsg = "The pooled projectile object doesn't have a projectile base script attached!";
                Logger.Error(errorMsg);
                throw new NullReferenceException(errorMsg);
            }
            
            projectileBase.SetupPool(this);
            
            NetworkServer.Spawn(newObj);

            return newObj;
        }

        private void OnTakeObject(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        private void OnReturnObject(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        private void OnDestroyObject(GameObject gameObject)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}