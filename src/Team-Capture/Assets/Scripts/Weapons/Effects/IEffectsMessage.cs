// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using UnityEngine.Scripting;

namespace Team_Capture.Weapons.Effects
{
    /// <summary>
    ///     Message for clients to play an effect
    /// </summary>
    public interface IEffectsMessage : NetworkMessage
    {
        public EffectsMessageType EffectsMessageType { get; }

        public void Serialize(NetworkWriter writer);
    }

    [Preserve]
    public static class EffectsMessageNetworkWriteReader
    {
        public static void Write(this NetworkWriter writer, IEffectsMessage effectsMessage)
        {
            writer.WriteByte((byte) effectsMessage.EffectsMessageType);
            effectsMessage.Serialize(writer);
        }

        public static IEffectsMessage Read(this NetworkReader reader)
        {
            EffectsMessageType effectsMessageType = (EffectsMessageType) reader.ReadByte();
            switch (effectsMessageType)
            {
                case EffectsMessageType.Default:
                    return new DefaultEffectsMessage(reader);
                case EffectsMessageType.Melee:
                    return new MeleeEffectsMessage(reader);
                case EffectsMessageType.Projectile:
                    return new ProjectileEffectsMessage();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}