// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.Core;
using Team_Capture.Logging;
using Team_Capture.Player;

namespace Team_Capture.Weapons.Projectiles
{
    public abstract class ProjectileBase : NetworkBehaviour
    {
        /// <summary>
        ///     <see cref="Player.PlayerManager"/> who "owns" this projectile
        /// </summary>
        protected PlayerManager ProjectileOwner;

        /// <summary>
        ///     Internal setup method for projectiles
        /// </summary>
        /// <param name="playerManager"></param>
        internal void Setup(PlayerManager playerManager)
        {
            ProjectileOwner = playerManager;
        }

        /// <summary>
        ///     Called on setup
        /// </summary>
        protected virtual void Setup()
        {
            if(ProjectileOwner == null)
                Logger.Error("Projectile owner is null!");
        }

        private void Start()
        {
            if (isServer)
            {
                RpcSetupOnAllClients(ProjectileOwner.transform.name);
                Setup();
            }
        }

        [ClientRpc]
        private void RpcSetupOnAllClients(string ownerPlayerId)
        {
            ProjectileOwner = GameManager.GetPlayer(ownerPlayerId);
            Setup();
        }
    }
}