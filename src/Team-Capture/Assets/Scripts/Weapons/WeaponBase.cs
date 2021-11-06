// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.Weapons.Effects;
using UnityEngine;
using UnityEngine.Scripting;

namespace Team_Capture.Weapons
{
    public abstract class WeaponBase : ScriptableObject
    {
        public string weaponId;

        public GameObject weaponObjectPrefab;
        
        public abstract WeaponType WeaponType { get; }

        protected GameObject weaponObjectInstance;
        protected WeaponManager weaponManager;
        protected bool isServer;

        #region Weapon Perform Helper
        
        protected void DoWeaponEffects(IEffectsMessage effectsMessage)
        {
            weaponManager.RpcDoWeaponEffects(effectsMessage);
        }

        #endregion

        internal void Setup(WeaponManager weaponMan, bool server, GameObject objectInstance)
        {
            weaponObjectInstance = objectInstance;
            isServer = server;
            weaponManager = weaponMan;
            OnAdd();
        }
        
        public abstract void OnPerform(bool buttonDown);

        public abstract void OnReload();

        public abstract void OnWeaponEffects(IEffectsMessage effectsMessage);

        public abstract void OnSwitchOnTo();
        public abstract void OnSwitchOff();

        protected abstract void OnAdd();
        public abstract void OnRemove();
        
        //Networking
        internal void Serialize(NetworkWriter writer) => OnSerialize(writer);
        protected abstract void OnSerialize(NetworkWriter writer);
    }
    
    [Preserve]
    public static class WeaponNetworkWriteReader
    {
        public static void Write(this NetworkWriter writer, WeaponBase weapon)
        {
            writer.WriteByte((byte)weapon.WeaponType);
            weapon.Serialize(writer);
        }

        public static WeaponBase Read(this NetworkReader reader)
        {
            WeaponType weaponType = (WeaponType)reader.ReadByte();
            switch (weaponType)
            {
                case WeaponType.Default:
                    return WeaponDefault.OnDeserialize(reader);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}