// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.Weapons.Projectiles;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;
using Object = UnityEngine.Object;

namespace Team_Capture.Pooling
{
    /// <summary>
    ///     A <see cref="GameObject"/> pool designed for objects that need to be on all clients, and managed by the
    ///     server.
    /// </summary>
    public sealed class NetworkProjectileObjectsPool : GameObjectPoolBase
    {
        /// <summary>
        ///     Instantiates a new <see cref="NetworkProjectileObjectsPool"/>
        /// </summary>
        /// <param name="prefab"></param>
        public NetworkProjectileObjectsPool(GameObject prefab)
            : base(prefab)
        {
        }

        protected override GameObject CreateObject(GameObject prefab)
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

        protected override void OnTakeObject(GameObject gameObject)
        {
        }

        protected override void OnReturnObject(GameObject gameObject)
        {
        }

        protected override void OnDestroyObject(GameObject gameObject)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}