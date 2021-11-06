// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using UnityEngine.Scripting;

namespace Team_Capture.Weapons.UI
{
    public interface IHudUpdateMessage : NetworkMessage
    {
        public UIUpdateType UpdateType { get; }
        public string WeaponId { get; set; }

        //TODO: It seems like we do this a lot, maybe have one interface with this?
        public void Serialize(NetworkWriter writer);
    }

    [Preserve]
    public static class HudUpdateMessageWriterReader
    {
        public static void Write(this NetworkWriter writer, IHudUpdateMessage hudUpdateMessage)
        {
            writer.WriteByte((byte)hudUpdateMessage.UpdateType);
            hudUpdateMessage.Serialize(writer);
        }

        public static IHudUpdateMessage Read(this NetworkReader reader)
        {
            UIUpdateType updateType = (UIUpdateType) reader.ReadByte();
            switch (updateType)
            {
                case UIUpdateType.Default:
                    return new DefaultHudUpdateMessage(reader);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}