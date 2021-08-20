// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;
using UnityEngine.Pool;

namespace Team_Capture.Pooling
{
    internal sealed class GameObjectPool
    {
        private readonly IObjectPool<GameObject> objectPool;

        public GameObjectPool(GameObject prefab)
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
            GameObject newObj = Object.Instantiate(prefab);
            PoolReturn returnComponent = newObj.GetComponent<PoolReturn>();
            if(returnComponent != null)
                returnComponent.Setup(this);

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
            Object.Destroy(gameObject);
        }
    }
}