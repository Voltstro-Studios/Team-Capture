// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;

namespace Team_Capture.Weapons.UI
{
    public struct DefaultHudUpdateMessage : IHudUpdateMessage
    {
        public DefaultHudUpdateMessage(NetworkReader reader)
        {
            WeaponId = reader.ReadString();
            CurrentBullets = reader.ReadInt();
            IsReloading = reader.ReadBool();
        }

        public DefaultHudUpdateMessage(string weaponId, int currentBullets, bool isReloading)
        {
            WeaponId = weaponId;
            CurrentBullets = currentBullets;
            IsReloading = isReloading;
        }
        
        public UIUpdateType UpdateType => UIUpdateType.Default;
        public string WeaponId { get; set; }

        public int CurrentBullets { get; }
        public bool IsReloading { get; }
        
        public void Serialize(NetworkWriter writer)
        {
            writer.WriteString(WeaponId);
            writer.WriteInt(CurrentBullets);
            writer.WriteBool(IsReloading);
        }
    }
}