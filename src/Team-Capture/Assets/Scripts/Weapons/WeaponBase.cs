// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.AddressablesAddons;
using Team_Capture.UI;
using Team_Capture.Weapons.Effects;
using Team_Capture.Weapons.UI;
using UnityEngine;
using UnityEngine.Scripting;

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     Base implementation for all weapons
    /// </summary>
    public abstract class WeaponBase : ScriptableObject
    {
        /// <summary>
        ///     The 'ID' of the weapon, what will be used to share across on the network, so make it unique
        /// </summary>
        [Tooltip("The 'ID' of the weapon, what will be used to share across on the network, so make it unique")]
        public string weaponId;

        /// <summary>
        ///     The prefab of the weapon
        /// </summary>
        [Tooltip("The prefab of the weapon")]
        public CachedAddressable<GameObject> weaponObjectPrefab;

        /// <summary>
        ///     The <see cref="WeaponType"/> that this instance is
        /// </summary>
        public abstract WeaponType WeaponType { get; }
        
        public abstract bool IsReloadable { get; }

        /// <summary>
        ///     Access to instantiated weapon object
        /// </summary>
        protected GameObject weaponObjectInstance;
        
        /// <summary>
        ///     Access to the player's <see cref="WeaponManager"/>
        /// </summary>
        protected WeaponManager weaponManager;
        
        /// <summary>
        ///     Is this running on a server?
        /// </summary>
        protected bool isServer;
        
        /// <summary>
        ///     Is this running on a local client
        /// </summary>
        protected bool isLocalClient;

        /// <summary>
        ///     <see cref="HudAmmoControls"/>, for controlling the local client's ammo part of the HUD
        /// </summary>
        protected HudAmmoControls HudAmmoControls => weaponManager.playerManager.PlayerUIManager.HudAmmoControls;
        
        /// <summary>
        ///     Tells clients to to the RPC effects
        /// </summary>
        /// <param name="effectsMessage"></param>
        protected void DoWeaponEffects(IEffectsMessage effectsMessage)
        {
            weaponManager.RpcDoWeaponEffects(effectsMessage);
        }

        /// <summary>
        ///     Tells the local client that this belongs to, to update their UI
        /// </summary>
        /// <param name="hudUpdateMessage"></param>
        protected void DoPlayerUIUpdate(IHudUpdateMessage hudUpdateMessage)
        {
            weaponManager.RpcUpdateUI(hudUpdateMessage);
        }

        /// <summary>
        ///     Sets up this <see cref="WeaponBase"/>
        /// </summary>
        /// <param name="weaponMan"></param>
        /// <param name="server"></param>
        /// <param name="localClient"></param>
        /// <param name="objectInstance"></param>
        internal void Setup(WeaponManager weaponMan, bool server, bool localClient, GameObject objectInstance)
        {
            weaponObjectInstance = objectInstance;
            isServer = server;
            isLocalClient = localClient;
            weaponManager = weaponMan;
            OnAdd();
        }
        
        /// <summary>
        ///     Called when we want to fire
        /// </summary>
        /// <param name="buttonDown"></param>
        public abstract void OnPerform(bool buttonDown);

        /// <summary>
        ///     Called when we want to reload
        /// </summary>
        public abstract void OnReload();

        /// <summary>
        ///     Called when the weapon is switched onto
        /// </summary>
        public abstract void OnSwitchOnTo();
        
        /// <summary>
        ///     Called when the weapon is switched off
        /// </summary>
        public abstract void OnSwitchOff();

        /// <summary>
        ///     Called when the weapon is added
        /// </summary>
        protected abstract void OnAdd();
        
        /// <summary>
        ///     Called when the weapon is removed
        /// </summary>
        public abstract void OnRemove();
        
        /// <summary>
        ///     Called on clients when the server requests to player the weapon's UI effects
        /// </summary>
        /// <param name="effectsMessage"></param>
        public abstract void OnWeaponEffects(IEffectsMessage effectsMessage);
        
        /// <summary>
        ///     Called on local client when the server provides new UI info
        /// </summary>
        /// <param name="hudUpdateMessage"></param>
        public abstract void OnUIUpdate(IHudUpdateMessage hudUpdateMessage);
        
        //Networking
        
        /// <summary>
        ///     Writes <see cref="WeaponBase"/> info to a <see cref="NetworkWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        internal void Serialize(NetworkWriter writer) => OnSerialize(writer);
        
        /// <summary>
        ///     Called when the weapon info needs to be written to a <see cref="NetworkWriter"/>
        /// </summary>
        /// <param name="writer"></param>
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
                case WeaponType.Melee:
                    return WeaponMelee.OnDeserialize(reader);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}