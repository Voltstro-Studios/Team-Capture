// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.Helper;
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
    public class WeaponManager : NetworkBehaviour
    {
        /// <summary>
        ///     The layer to use when creating our weapons (for local weapons)
        /// </summary>
        [SerializeField] private string weaponLayerName = "LocalWeapon";

        /// <summary>
        ///     Where all the weapons are created
        /// </summary>
        [SerializeField] private Transform weaponsHolderSpot;

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

        /// <summary>
        ///     <see cref="Weapons.WeaponSway" /> script, for use by <see cref="PlayerInputManager" />
        /// </summary>
        [NonSerialized] internal WeaponSway WeaponSway;

        /// <summary>
        ///     What is the selected weapon
        /// </summary>
        [field: SyncVar]
        public int SelectedWeaponIndex { get; private set; }

        /// <summary>
        ///     Gets how many weapons are on the weapon holder spot
        /// </summary>
        public int WeaponHolderSpotChildCount => weaponsHolderSpot.childCount;

        [NonSerialized] public PlayerManager playerManager;

        #region Unity Event Functions

        private void Awake()
        {
            playerManager = GetComponent<PlayerManager>();
        }

        private void Start()
        {
            //Create all existing weapons on start
            for (int i = 0; i < weapons.Count; i++)
            {
                GameObject newWeapon = CreateNewWeaponModel(weapons[i]);
                weapons[i].Setup(this, isServer, isLocalPlayer, newWeapon);

                if (isLocalPlayer)
                    SetupWeaponObjectLocal(newWeapon);

                newWeapon.SetActive(SelectedWeaponIndex == i);
            }
            
            //Setup our add weapon callback
            weapons.Callback += WeaponListCallback;
        }

        #endregion

        /// <summary>
        ///     Server callback for when <see cref="weapons" /> is modified
        /// </summary>
        /// <param name="op"></param>
        /// <param name="itemIndex"></param>
        /// <param name="oldWeapon"></param>
        /// <param name="newWeapon"></param>
        private void WeaponListCallback(SyncList<WeaponBase>.Operation op, int itemIndex,
            WeaponBase oldWeapon, WeaponBase newWeapon)
        {
            switch (op)
            {
                case SyncList<WeaponBase>.Operation.OP_ADD:
                    if(isServer)
                        break;
                    
                    GameObject newWeaponModel = CreateNewWeaponModel(newWeapon);
                    newWeapon.Setup(this, isServer, isLocalPlayer, newWeaponModel);
                    Logger.Info("On add weapon");
                    break;
                case SyncList<WeaponBase>.Operation.OP_CLEAR:
                    RemoveAllWeaponsModels();
                    break;
            }
        }
        
        #region Network Overrides

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            WeaponSway = weaponsHolderSpot.gameObject.AddComponent<WeaponSway>();
        }

        #endregion

        #region Weapon List Management

        /// <summary>
        ///     Get the active weapon
        /// </summary>
        /// <returns></returns>
        internal WeaponBase GetActiveWeapon()
        {
            return weapons.Count == 0 ? null : weapons[SelectedWeaponIndex];
        }

        /// <summary>
        ///     Gets a weapon from the list based off its ID
        /// </summary>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        internal WeaponBase GetWeaponFromId(string weaponId)
        {
            IEnumerable<WeaponBase> result = from a in weapons
                where a.weaponId == weaponId
                select a;

            return WeaponsResourceManager.GetWeapon(result.FirstOrDefault()?.weaponId);
        }

        /// <summary>
        ///     Gets a weapon from the list
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        internal WeaponBase GetWeapon(WeaponBase weapon)
        {
            IEnumerable<WeaponBase> result = from a in weapons
                where a == weapon
                select a;

            return WeaponsResourceManager.GetWeapon(result.FirstOrDefault()?.weaponId);
        }

        #endregion

        #region Weapon Shooting

        [TargetRpc(channel = Channels.Unreliable)]
        internal void RpcUpdateUI(IHudUpdateMessage hudUpdateMessage)
        {
            string weaponId = hudUpdateMessage.WeaponId;
            WeaponBase weapon = GetWeaponFromId(weaponId);
            if(weapon != null)
                weapon.OnUIUpdate(hudUpdateMessage);
        }

        [Command(channel = Channels.Unreliable)]
        internal void CmdShootWeapon(bool buttonDown)
        {
            GetActiveWeapon().OnPerform(buttonDown);
        }

        [ClientRpc(channel = Channels.Unreliable)]
        internal void RpcDoWeaponEffects(IEffectsMessage effectsMessage)
        {
            GetActiveWeapon().OnWeaponEffects(effectsMessage);
        }

        #endregion
        
        #region Weapon Reloading

        /// <summary>
        ///     Requests the server to reload the current weapon
        /// </summary>
        [Client]
        internal void ClientReloadWeapon()
        {
            //Ask the server kindly to reload the weapon
            CmdReloadPlayerWeapon();
        }

        /// <summary>
        ///     Reloads clients current weapon
        /// </summary>
        [Command(channel = Channels.Unreliable)]
        private void CmdReloadPlayerWeapon()
        {
            WeaponBase weapon = GetActiveWeapon();
            weapon.OnReload();
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

            GameObject weaponObject = CreateNewWeaponModel(weapon);
            weapon.Setup(this, true, false, weaponObject);
            weapons.Add(weapon);

            if (weapons.Count > 1) 
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

            GameObject newWeapon = Instantiate(weapon.weaponObjectPrefab, weaponsHolderSpot);

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
            foreach (WeaponBase weapon in weapons)
                weapon.OnRemove();
            
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
            if (weapons.ElementAt(index) == null)
                return;

            Logger.Debug($"Player `{transform.name}` set their weapon index to `{index}`.");
            SetClientWeaponIndex(index);
        }

        /// <summary>
        ///     Sets the <see cref="SelectedWeaponIndex" />
        /// </summary>
        /// <param name="index"></param>
        [Server]
        private void SetClientWeaponIndex(int index)
        {
            GetActiveWeapon().OnSwitchOff();
            
            SelectedWeaponIndex = index;
            
            GetActiveWeapon().OnSwitchOnTo();
            
            RpcSelectWeapon(index);
        }

        /// <summary>
        ///     Changes the weapons <see cref="GameObject" /> active on this client
        /// </summary>
        /// <param name="index"></param>
        [ClientRpc(channel = Channels.Unreliable)]
        private void RpcSelectWeapon(int index)
        {
            for (int i = 0; i < weaponsHolderSpot.childCount; i++)
                weaponsHolderSpot.GetChild(i).gameObject.SetActive(i == index);
        }

        #endregion
    }
}