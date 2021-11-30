// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using UnityEngine;

namespace Team_Capture.Weapons.Effects
{
    /// <summary>
    ///     Melee <see cref="IEffectsMessage"/> for <see cref="WeaponMelee"/>
    /// </summary>
    public struct MeleeEffectsMessage : IEffectsMessage
    {
        public MeleeEffectsMessage(NetworkReader reader)
        {
            HitPoint = reader.ReadVector3Nullable();
            HitNormal = reader.ReadVector3Nullable();
        }

        public MeleeEffectsMessage(Vector3? hitPoint, Vector3? hitNormal)
        {
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
        
        public EffectsMessageType EffectsMessageType => EffectsMessageType.Melee;

        /// <summary>
        ///     Where did we hit
        /// </summary>
        public Vector3? HitPoint;
        
        /// <summary>
        ///     The normal of the hit point
        /// </summary>
        public Vector3? HitNormal;
        
        public void Serialize(NetworkWriter writer)
        {
            writer.WriteVector3Nullable(HitPoint);
            writer.WriteVector3Nullable(HitNormal);
        }
    }
}