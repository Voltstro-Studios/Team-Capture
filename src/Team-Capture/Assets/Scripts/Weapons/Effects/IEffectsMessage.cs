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
    public interface IEffectsMessage : NetworkMessage
    {
        public EffectsType EffectsType { get; }

        public void Serialize(NetworkWriter writer);
    }

    [Preserve]
    public static class EffectsMessageNetworkWriteReader
    {
        public static void Write(this NetworkWriter writer, IEffectsMessage effectsMessage)
        {
            writer.WriteByte((byte)effectsMessage.EffectsType);
            effectsMessage.Serialize(writer);
        }

        public static IEffectsMessage Read(this NetworkReader reader)
        {
            EffectsType effectsType = (EffectsType)reader.ReadByte();
            switch (effectsType)
            {
                case EffectsType.Default:
                    return new DefaultEffectsMessage(reader);
                case EffectsType.Melee:
                    return new MeleeEffectsMessage(reader);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}