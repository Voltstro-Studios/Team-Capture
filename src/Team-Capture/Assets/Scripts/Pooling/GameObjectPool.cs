// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Pooling
{
    public sealed class GameObjectPool : GameObjectPoolBase
    {
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