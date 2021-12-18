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

        private void Start()
        {
            if (isServer)
            {
                RpcSetupOnAllClients(ProjectileOwner.transform.name);
                Setup();
            }
        }

        /// <summary>
        ///     Called on setup
        /// </summary>
        protected abstract void Setup();

        //The owner is the server
        [ClientRpc(includeOwner = false)]
        private void RpcSetupOnAllClients(string ownerPlayerId)
        {
            Logger.Info("Got SETUP");
            
            if (string.IsNullOrWhiteSpace(ownerPlayerId))
            {
                Logger.Error("Passed in player ID is empty or null!");
                return;
            }
            
            ProjectileOwner = GameManager.GetPlayer(ownerPlayerId);
            Setup();
        }
    }
}