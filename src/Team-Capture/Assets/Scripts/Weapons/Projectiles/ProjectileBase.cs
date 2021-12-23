// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Player;
using Team_Capture.Pooling;
using UnityEngine;
using UnityEngine.Scripting;

namespace Team_Capture.Weapons.Projectiles
{
    [Preserve]
    public abstract class ProjectileBase : NetworkBehaviour
    {
        /// <summary>
        ///     The game object that will hold everything
        /// </summary>
        [Header("Component")]
        [SerializeField] private GameObject projectileObject;
        
        /// <summary>
        ///     <see cref="Player.PlayerManager"/> who "owns" this projectile
        /// </summary>
        protected PlayerManager ProjectileOwner;

        /// <summary>
        ///     Game objects pool
        /// </summary>
        protected NetworkProjectileObjectsPool NetworkProjectileObjectsPool;

        [SyncVar(hook = nameof(OnOwnerUpdate))] private string ownerPlayerId;

        /// <summary>
        ///     Internal setup method for projectiles
        /// </summary>
        /// <param name="playerManager"></param>
        [Server]
        internal void SetupOwner(PlayerManager playerManager)
        {
            ProjectileOwner = playerManager;
            ownerPlayerId = playerManager.transform.name;
        }

        /// <summary>
        ///     Internal setup for the objects pool
        /// </summary>
        /// <param name="objectsPool"></param>
        [Server]
        internal void SetupPool(NetworkProjectileObjectsPool objectsPool)
        {
            NetworkProjectileObjectsPool = objectsPool;
        }

        /// <summary>
        ///     Enable method for the server
        /// </summary>
        [Server]
        internal void ServerEnable(Vector3 location, Vector3 rotation)
        {
            RpcEnable(location, rotation);
            Enable(location, rotation);
        }

        /// <summary>
        ///     Disable method for the server
        /// </summary>
        [Server]
        internal void ServerDisable()
        {
            RpcDisable();
            Disable();
        }

        [Server]
        internal void ServerReturnToPool()
        {
            NetworkProjectileObjectsPool.ReturnPooledObject(gameObject);
        }

        protected abstract void OnUserIdUpdate(string userId);

        protected virtual void Enable(Vector3 location, Vector3 rotation)
        {
            Transform projectileTrans = transform;
            projectileTrans.position = location;
            projectileTrans.rotation = Quaternion.Euler(rotation);
            projectileObject.SetActive(true);
        }

        protected virtual void Disable()
        {
            projectileObject.SetActive(false);
        }
        
        [ClientRpc(includeOwner = false)]
        private void RpcEnable(Vector3 location, Vector3 rotation)
        {
            Enable(location, rotation);
        }

        [ClientRpc(includeOwner = false)]
        private void RpcDisable()
        {
            Disable();
        }

        private void OnOwnerUpdate(string oldId, string newId)
        {
            if(isClient)
                OnUserIdUpdate(newId);
        }
    }
}