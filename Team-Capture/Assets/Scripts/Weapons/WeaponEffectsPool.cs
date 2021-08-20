// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;
using UnityEngine.Pool;

namespace Team_Capture.Weapons
{
    internal sealed class WeaponEffectsPool
    {
        private readonly IObjectPool<GameObject> objectPool;

        public WeaponEffectsPool(GameObject prefab)
        {
            objectPool = new LinkedPool<GameObject>(() => CreateObject(prefab), OnTakeObject, OnReturnObject, OnDestroyObject);
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
            return Object.Instantiate(prefab);
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
            Object.Destroy(gameObject);
        }
    }
}