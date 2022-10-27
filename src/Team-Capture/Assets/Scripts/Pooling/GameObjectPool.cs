// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Pooling
{
    /// <summary>
    ///     A standard object pool for <see cref="GameObject"/>s
    /// </summary>
    public sealed class GameObjectPool : GameObjectPoolBase
    {
        /// <summary>
        ///     Instantiates a new <see cref="GameObjectPool"/>
        /// </summary>
        /// <param name="prefab"></param>
        public GameObjectPool(GameObject prefab)
            : base(prefab)
        {
        }

        protected override GameObject CreateObject(GameObject prefab)
        {
            GameObject newObj = Object.Instantiate(prefab);
            PoolReturn returnComponent = newObj.GetComponent<PoolReturn>();
            if (returnComponent != null)
                returnComponent.Setup(this);

            return newObj;
        }
    }
}