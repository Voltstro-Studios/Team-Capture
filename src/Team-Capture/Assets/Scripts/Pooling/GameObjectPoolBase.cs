// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;
using UnityEngine.Pool;

namespace Team_Capture.Pooling
{
    /// <summary>
    ///     Base class used for object pooling
    /// </summary>
    public abstract class GameObjectPoolBase
    {
        /// <summary>
        ///     Underlining Unity object pool
        /// </summary>
        protected readonly IObjectPool<GameObject> objectPool;

        /// <summary>
        ///     Instantiates a new <see cref="GameObjectPoolBase"/>
        /// </summary>
        /// <param name="prefab"></param>
        public GameObjectPoolBase(GameObject prefab)
        {
            objectPool = new LinkedPool<GameObject>(() => CreateObject(prefab), OnTakeObject, OnReturnObject,
                OnDestroyObject);
        }

        /// <summary>
        ///     Gets a pooled object
        /// </summary>
        /// <returns></returns>
        public GameObject GetPooledObject()
        {
            return objectPool.Get();
        }

        /// <summary>
        ///     Return a pooled object
        /// </summary>
        /// <param name="gameObject"></param>
        public void ReturnPooledObject(GameObject gameObject)
        {
            objectPool.Release(gameObject);
        }

        /// <summary>
        ///     Called when a new <see cref="GameObject"/> needs to be created
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        protected abstract GameObject CreateObject(GameObject prefab);

        /// <summary>
        ///     Called when an object is taken out of the pool
        /// </summary>
        /// <param name="gameObject"></param>
        protected virtual void OnTakeObject(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        ///     Called when an object is returned to the pool
        /// </summary>
        /// <param name="gameObject"></param>
        protected virtual void OnReturnObject(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        ///     Returned when the object is destroyed
        /// </summary>
        /// <param name="gameObject"></param>
        protected virtual void OnDestroyObject(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}