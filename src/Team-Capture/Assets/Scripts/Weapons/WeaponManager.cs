// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Mirror;
using NetFabric.Hyperlinq;
using Team_Capture.Helper;
using Team_Capture.Helper.Extensions;
using Team_Capture.Player;
using Team_Capture.SceneManagement;
using Team_Capture.Weapons.Effects;
using Team_Capture.Weapons.UI;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     Weapon management, such as adding, removing and selecting weapons
    /// </summary>
    [DefaultExecutionOrder(1100)]
    public class WeaponManager : NetworkBehaviour
    {
        /// <summary>
        ///     The layer to use when creating our weapons (for local weapons)
        /// </summary>
        [SerializeField] private string weaponLayerName = "LocalWeapon";

        /// <summary>
        ///     Where all the weapons are created
        /// </summary>
        [SerializeField] internal Transform weaponsHolderSpot;

        /// <summary>
        ///     Player's camera
        /// </summary>
        [SerializeField] internal Transform localPlayerCamera;

        /// <summary>
        ///     Whats layers to include in raycasting
        /// </summary>
        [SerializeField] internal LayerMask raycastLayerMask;

        /// <summary>
        ///     A synced list of all the weapons this client has
        /// </summary>
        private readonly SyncList<WeaponBase> weapons = new();

        [NonSerialized] public PlayerManager playerManager;

        /// <summary>
        ///     <see cref="Weapons.WeaponSway" /> script, for use by <see cref="PlayerInputManager" />
        /// </summary>
        [NonSerialized] internal WeaponSway WeaponSway;

        internal PlayerCameraEffects CameraEffects { get; private set; }
        
        internal PlayerWeaponRecoil WeaponRecoil { get; private set; }

        /// <summary>
        ///     What is the selected weapon
        /// </summary>
        [field: SyncVar(hook = nameof(OnWeaponIndexSet))]
        public int SelectedWeaponIndex { get; private set; }

        /// <summary>
        ///     Gets how many weapons are on the weapon holder spot
        /// </summary>
        public int WeaponHolderSpotChildCount => weaponsHolderSpot.childCount;

        public override void OnStopServer()
        {
            RemoveAllWeapons();
        }

        #region Network Overrides

        #endregion

        #region Unity Event Functions

        private void Awake()
        {
            playerManager = GetComponent<PlayerManager>();
        }

        private void Start()
        {
            weapons.Callback += OnWeaponCallback;
            
            if (!isServer)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    WeaponBase weapon = weapons[i];
                    GameObject newWeaponModel = CreateNewWeaponModel(weapon);
                    weapon.Setup(this, isServer, isLocalPlayer, newWeaponModel);

                    newWeaponModel.SetActive(SelectedWeaponIndex == i);
                }
            }

            WeaponBase selectedWeapon = weapons[SelectedWeaponIndex];
            PlayerSetup setup = this.GetComponentOrThrow<PlayerSetup>();
            
            WeaponRecoil = setup.PlayerWeaponRecoilPoint.GetComponentOrThrow<PlayerWeaponRecoil>();
            WeaponRecoil.OnWeaponChange(selectedWeapon.weaponRecoilRotationSpeed, selectedWeapon.weaponRecoilRotationReturnSpeed);
            
            CameraEffects = setup.PlayerVCam
                .GetComponentOrThrow<PlayerCameraEffects>("The PlayerCameraEffects component should exist! Ensure this script executes AFTER PlayerSetup!");
            CameraEffects.OnWeaponChange(selectedWeapon.weaponRecoilCameraSpeed, selectedWeapon.weaponRecoilCameraReturnSpeed);
            
            if (isLocalPlayer)
            {
                WeaponSway = weaponsHolderSpot.gameObject.AddComponent<WeaponSway>();
                WeaponSway.SetWeapon(selectedWeapon);
            }
        }
        
        private void OnWeaponCallback(SyncList<WeaponBase>.Operation op, int itemIndex, WeaponBase oldItem, WeaponBase newItem)
        {
            if (op == SyncList<WeaponBase>.Operation.OP_CLEAR)
                RemoveAllWeaponsModels();
            
            else if (op == SyncList<WeaponBase>.Operation.OP_ADD)
            {
                GameObject newWeaponObj = CreateNewWeaponModel(newItem);
                newItem.Setup(this, isServer, isLocalPlayer, newWeaponObj);
            
                if(isLocalPlayer)
                    SetupWeaponObjectLocal(newWeaponObj);
                
                SetIndex(itemIndex);
            }
        }

        #endregion

        #region Weapon List Management

        /// <summary>
        ///     Get the active weapon
        /// </summary>
        /// <returns></returns>
        internal WeaponBase GetActiveWeapon()
        {
            return GetWeaponAtIndex(SelectedWeaponIndex);
        }

        /// <summary>
        ///     Gets a weapon at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal WeaponBase GetWeaponAtIndex(int index)
        {
            if (index > weapons.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return weapons.Count == 0 ? null : weapons[index];
        }

        /// <summary>
        ///     Gets a weapon from the list based off its ID
        /// </summary>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        internal WeaponBase GetWeaponFromId(string weaponId)
        {
            Option<WeaponBase> result = weapons.AsValueEnumerable()
                .Where(a => a.weaponId == weaponId)
                .First();

            return result.IsNone ? null : WeaponsResourceManager.GetWeapon(result.Value.weaponId);
        }

        /// <summary>
        ///     Gets a weapon from the list
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        internal WeaponBase GetWeapon(WeaponBase weapon)
        {
            Option<WeaponBase> result = weapons.AsValueEnumerable()
                .Where(a => a == weapon)
                .First();

            return result.Value;
        }

        #endregion

        #region Weapon Shooting

        [TargetRpc(channel = Channels.Unreliable)]
        internal void RpcUpdateUI(IHudUpdateMessage hudUpdateMessage)
        {
            string weaponId = hudUpdateMessage.WeaponId;
            WeaponBase weapon = GetWeaponFromId(weaponId);
            if (weapon != null)
                weapon.OnUIUpdate(this, hudUpdateMessage);
        }

        [Command(channel = Channels.Unreliable)]
        internal void CmdShootWeapon(bool buttonDown)
        {
            if(playerManager.IsDead)
                return;
            
            Logger.Debug("Got request to fire player {PlayerId}'s weapon.", transform.name);
            WeaponBase weapon = GetActiveWeapon();
            if (weapon == null)
            {
                Logger.Error("Getting player's current weapon returned null!");
                return;
            }
            
            weapon.OnPerform(this, buttonDown);
        }

        [ClientRpc(channel = Channels.Unreliable)]
        internal void RpcDoWeaponEffects(IEffectsMessage effectsMessage)
        {
            WeaponBase weaponBase = GetActiveWeapon();
            if(weaponBase != null)
            {
                weaponBase.OnWeaponEffects(this, effectsMessage);
            }
        }

        #endregion

        #region Weapon Reloading

        /// <summary>
        ///     Requests the server to reload the current weapon
        /// </summary>
        [Client]
        internal void ClientReloadWeapon()
        {
            if (!GetActiveWeapon().IsReloadable)
                return;

            //Ask the server kindly to reload the weapon
            CmdReloadPlayerWeapon();
        }

        /// <summary>
        ///     Reloads clients current weapon
        /// </summary>
        [Command(channel = Channels.Unreliable)]
        private void CmdReloadPlayerWeapon()
        {
            if(playerManager.IsDead)
                return;
            
            WeaponBase weapon = GetActiveWeapon();
            if (!weapon.IsReloadable)
                return;

            weapon.OnReload(this);
        }

        #endregion

        #region Add Weapons

        /// <summary>
        ///     Adds the scene's stock weapons to the client
        /// </summary>
        [Server]
        public void AddStockWeapons()
        {
            foreach (WeaponBase weapon in GameSceneManager.GetActiveScene().stockWeapons)
                AddWeapon(weapon);
        }

        /// <summary>
        ///     Adds a weapon
        /// </summary>
        /// <param name="weapon">The weapon to add</param>
        [Server]
        internal void AddWeapon(WeaponBase weapon)
        {
            if (weapon == null)
                return;

            WeaponBase newWeapon = Instantiate(weapon);
            GameObject newWeaponModel = CreateNewWeaponModel(newWeapon);
            newWeapon.Setup(this, isServer, isLocalPlayer, newWeaponModel);
            weapons.Add(newWeapon);

            if (weapons.Count != 0)
                SetClientWeaponIndex(weapons.Count - 1);
        }


        /// <summary>
        ///     Instantiates a weapon model in all clients
        /// </summary>
        /// <param name="weapon"></param>
        private GameObject CreateNewWeaponModel(WeaponBase weapon)
        {
            if (weapon == null)
                return null;

            GameObject newWeapon = Instantiate(weapon.weaponObjectPrefab.Value, weaponsHolderSpot);

            if (isLocalPlayer)
                SetupWeaponObjectLocal(newWeapon);

            return newWeapon;
        }
        
        private void SetupWeaponObjectLocal(GameObject weaponObject)
        {
            LayersHelper.SetLayerRecursively(weaponObject, LayerMask.NameToLayer(weaponLayerName));
            WeaponGraphics weaponGraphics = weaponObject.GetComponent<WeaponGraphics>();
            if (weaponGraphics == null)
            {
                Logger.Error("Newly created weapon doesn't have a weapon graphics!");
                return;
            }

            weaponGraphics.DisableMeshRenderersShadows();
        }

        #endregion

        #region Weapon Removal

        /// <summary>
        ///     Removes all weapons this client has
        /// </summary>
        [Server]
        public void RemoveAllWeapons()
        {
            for (int i = 0; i < weapons.Count; i++)
                weapons[i].OnRemove();

            SelectedWeaponIndex = 0;
            weapons.Clear();
        }

        /// <summary>
        ///     Removes all weapons on the client
        /// </summary>
        private void RemoveAllWeaponsModels()
        {
            for (int i = 0; i < weaponsHolderSpot.childCount; i++) Destroy(weaponsHolderSpot.GetChild(i).gameObject);
        }

        #endregion

        #region Weapon Selection

        /// <summary>
        ///     Sets the <see cref="SelectedWeaponIndex" /> to your index
        /// </summary>
        /// <param name="index"></param>
        [Command(channel = Channels.Unreliable)]
        public void CmdSetWeaponIndex(int index)
        {
            if(playerManager.IsDead)
                return;
            
            Option<WeaponBase> result = weapons.AsValueEnumerable().ElementAt(index);

            if (result.IsNone)
                return;

            SetClientWeaponIndex(index);
        }

        /// <summary>
        ///     Sets the <see cref="SelectedWeaponIndex" />
        /// </summary>
        /// <param name="index"></param>
        [Server]
        private void SetClientWeaponIndex(int index)
        {
            Logger.Debug($"Player `{transform.name}` set their weapon index to `{index}`.");

            GetActiveWeapon().OnSwitchOff(this);
            SelectedWeaponIndex = index;
            GetActiveWeapon().OnSwitchOnTo(this);

            RpcSelectWeapon(index);
        }

        private void OnWeaponIndexSet(int oldIndex, int newIndex)
        {
            if (!isServer && !isLocalPlayer)
                return;

            if (oldIndex < weapons.Count)
                GetWeaponAtIndex(oldIndex).OnSwitchOff(this);
            if (newIndex < weapons.Count)
                GetWeaponAtIndex(newIndex).OnSwitchOnTo(this);
        }

        /// <summary>
        ///     Changes the weapons <see cref="GameObject" /> active on this client
        /// </summary>
        /// <param name="index"></param>
        [ClientRpc(channel = Channels.Unreliable)]
        private void RpcSelectWeapon(int index)
        {
            SetIndex(index);
        }

        private void SetIndex(int index)
        {
            for (int i = 0; i < weaponsHolderSpot.childCount; i++)
                weaponsHolderSpot.GetChild(i).gameObject.SetActive(i == index);

            if (index + 1 > weapons.Count)
                return;
            WeaponBase weapon = weapons[index];
            
            WeaponRecoil.OnWeaponChange(weapon.weaponRecoilRotationSpeed, weapon.weaponRecoilRotationReturnSpeed);
            
            if (isLocalPlayer)
                WeaponSway.SetWeapon(weapon);
            if (isLocalPlayer || isServer)
                CameraEffects.OnWeaponChange(weapon.weaponRecoilCameraSpeed, weapon.weaponRecoilCameraReturnSpeed);
        }

        #endregion
    }
}