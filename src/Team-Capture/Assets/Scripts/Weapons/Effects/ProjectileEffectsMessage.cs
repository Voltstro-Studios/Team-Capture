// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;

namespace Team_Capture.Weapons.Effects
{
    public struct ProjectileEffectsMessage : IEffectsMessage
    {
        public EffectsMessageType EffectsMessageType => EffectsMessageType.Projectile;
        
        public void Serialize(NetworkWriter writer)
        {
        }
    }
}