// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;
using UnityEngine.Pool;

namespace Team_Capture.Pooling
{
    public abstract class GameObjectPoolBase
    {
        protected readonly IObjectPool<GameObject> objectPool;

        public GameObjectPoolBase(GameObject prefab)
        {
            objectPool = new LinkedPool<GameObject>(() => CreateObject(prefab), OnTakeObject, OnReturnObject,
                OnDestroyObject);
        }

        public GameObject GetPooledObject()
        {
            return objectPool.Get();
        }

        public virtual void ReturnPooledObject(GameObject gameObject)
        {
            objectPool.Release(gameObject);
        }

        protected abstract GameObject CreateObject(GameObject prefab);

        protected virtual void OnTakeObject(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        protected virtual void OnReturnObject(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroyObject(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}