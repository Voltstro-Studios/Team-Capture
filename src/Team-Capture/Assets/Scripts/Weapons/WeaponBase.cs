// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Mirror;
using Team_Capture.AddressablesAddons;
using Team_Capture.Weapons.Effects;
using Team_Capture.Weapons.UI;
using UnityEngine;
using UnityEngine.Scripting;
using Logger = Team_Capture.Logging.Logger;

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
        [Header("Weapon Details")]
        [Tooltip("The 'ID' of the weapon, what will be used to share across on the network, so make it unique")]
        public string weaponId;

        /// <summary>
        ///     The prefab of the weapon
        /// </summary>
        [Tooltip("The prefab of the weapon")] public CachedAddressable<GameObject> weaponObjectPrefab;

        /// <summary>
        ///     The multiplier for the weapon sway amount
        /// </summary>
        [Header("Weapon Sway")] [Range(0, 1)] public float weaponSwayAmountMultiplier = 1.0f;

        /// <summary>
        ///     The amount of weapon sway on the X and Y axis
        /// </summary>
        public Vector2 weaponSwayMax = new(0.25f, 0.2f);

        /// <summary>
        ///     Recoil effect on the camera
        /// </summary>
        [Header("Weapon Recoil")]
        [Tooltip("Recoil effect on the camera")]
        public Vector3 weaponCameraRecoilAmount = new(4f, 0f, 0f);

        public float weaponRecoilCameraSpeed = 6f;
        public float weaponRecoilCameraReturnSpeed = 25f;

        public Vector3 weaponRecoilRotation = new(10f, 0f, 0f);
        public float weaponRecoilRotationSpeed = 8f;
        public float weaponRecoilRotationReturnSpeed = 38f;

        /// <summary>
        ///     Is this running on a local client
        /// </summary>
        protected bool isLocalClient;

        /// <summary>
        ///     Is this running on a server?
        /// </summary>
        protected bool isServer;

        /// <summary>
        ///     Access to instantiated weapon object
        /// </summary>
        protected GameObject weaponObjectInstance;

        /// <summary>
        ///     The <see cref="WeaponType" /> that this instance is
        /// </summary>
        public abstract WeaponType WeaponType { get; }

        public abstract bool IsReloadable { get; }

        /// <summary>
        ///     Tells clients to to the RPC effects
        /// </summary>
        /// <param name="weaponManager"></param>
        /// <param name="effectsMessage"></param>
        protected void DoWeaponEffects(WeaponManager weaponManager, IEffectsMessage effectsMessage)
        {
            weaponManager.RpcDoWeaponEffects(effectsMessage);
        }

        /// <summary>
        ///     Tells the local client that this belongs to, to update their UI
        /// </summary>
        /// <param name="weaponManager"></param>
        /// <param name="hudUpdateMessage"></param>
        protected void DoPlayerUIUpdate(WeaponManager weaponManager, IHudUpdateMessage hudUpdateMessage)
        {
            weaponManager.RpcUpdateUI(hudUpdateMessage);
        }

        /// <summary>
        ///     Sets up this <see cref="WeaponBase" />
        /// </summary>
        /// <param name="weaponMan"></param>
        /// <param name="server"></param>
        /// <param name="localClient"></param>
        /// <param name="objectInstance"></param>
        internal void Setup(WeaponManager weaponMan, bool server, bool localClient, GameObject objectInstance)
        {
            Logger.Debug("Setup weapon {WeaponName}(Instance: {InstanceId}) on {ClientName}({ClientObjectID}, {NetId}) (Server: {IsServer}, LocalClient: {LocalClient})", 
                weaponId, GetInstanceID(), weaponMan.playerManager.User.UserName, weaponMan.transform.name, weaponMan.netId, server, localClient);

            if (weaponMan == null)
            {
                Logger.Error("The parsed in weapon manager was null!");
                throw new NullReferenceException("The parsed in weapon manager was null!");
            }

            if (objectInstance == null)
            {
                Logger.Error("The parsed in object instance was null!");
                throw new NullReferenceException("The parsed in object instance was null!");
            }
            
            weaponObjectInstance = objectInstance;
            isServer = server;
            isLocalClient = localClient;

            OnAdd(weaponMan);
        }

        /// <summary>
        ///     Called when we want to fire
        /// </summary>
        /// <param name="weaponManager"></param>
        /// <param name="buttonDown"></param>
        public abstract void OnPerform(WeaponManager weaponManager, bool buttonDown);

        /// <summary>
        ///     Called when we want to reload
        /// </summary>
        public abstract void OnReload(WeaponManager weaponManager);

        /// <summary>
        ///     Called when the weapon is switched onto
        /// </summary>
        public abstract void OnSwitchOnTo(WeaponManager weaponManager);

        /// <summary>
        ///     Called when the weapon is switched off
        /// </summary>
        public abstract void OnSwitchOff(WeaponManager weaponManager);

        /// <summary>
        ///     Called when the weapon is added
        /// </summary>
        protected abstract void OnAdd(WeaponManager weaponManager);

        /// <summary>
        ///     Called when the weapon is removed
        /// </summary>
        public abstract void OnRemove();

        /// <summary>
        ///     Called on clients when the server requests to player the weapon's UI effects
        /// </summary>
        /// <param name="weaponManager"></param>
        /// <param name="effectsMessage"></param>
        public abstract void OnWeaponEffects(WeaponManager weaponManager, IEffectsMessage effectsMessage);

        /// <summary>
        ///     Called on local client when the server provides new UI info
        /// </summary>
        /// <param name="weaponManager"></param>
        /// <param name="hudUpdateMessage"></param>
        public abstract void OnUIUpdate(WeaponManager weaponManager, IHudUpdateMessage hudUpdateMessage);

        /// <summary>
        ///     Objects that need to be pooled
        /// </summary>
        /// <returns></returns>
        public abstract KeyValuePair<CachedAddressable<GameObject>, bool>[] GetObjectsNeededToBePooled();

        //Networking

        /// <summary>
        ///     Writes <see cref="WeaponBase" /> info to a <see cref="NetworkWriter" />
        /// </summary>
        /// <param name="writer"></param>
        internal void Serialize(NetworkWriter writer)
        {
            OnSerialize(writer);
        }

        /// <summary>
        ///     Called when the weapon info needs to be written to a <see cref="NetworkWriter" />
        /// </summary>
        /// <param name="writer"></param>
        protected abstract void OnSerialize(NetworkWriter writer);
    }

    [Preserve]
    public static class WeaponNetworkWriteReader
    {
        public static void Write(this NetworkWriter writer, WeaponBase weapon)
        {
            writer.WriteByte((byte) weapon.WeaponType);
            weapon.Serialize(writer);
        }

        public static WeaponBase Read(this NetworkReader reader)
        {
            Logger.Debug("Read network weapon");
            WeaponType weaponType = (WeaponType) reader.ReadByte();
            switch (weaponType)
            {
                case WeaponType.Default:
                    return WeaponDefault.OnDeserialize(reader);
                case WeaponType.Melee:
                    return WeaponMelee.OnDeserialize(reader);
                case WeaponType.Projectile:
                    return WeaponProjectile.OnDeserialize(reader);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}