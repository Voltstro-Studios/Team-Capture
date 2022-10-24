// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.AddressablesAddons;
using Team_Capture.Helper;
using Team_Capture.Pooling;
using Team_Capture.SceneManagement;
using Team_Capture.UI;
using Team_Capture.Weapons.Effects;
using Team_Capture.Weapons.Projectiles;
using Team_Capture.Weapons.UI;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Weapons
{
    [CreateAssetMenu(fileName = "New Projectile Type Weapon", menuName = "Team-Capture/Weapons/Projectile")]
    public class WeaponProjectile : WeaponBase
    {
        /// <summary>
        ///     How far does something have to be for "auto aim" to kick in
        /// </summary>
        [Header("Weapon Aiming")] public float projectileAutoAimMinRange = 10f;

        /// <summary>
        ///     How many projectiles to hold
        /// </summary>
        [Header("Weapon Raycast Settings")] public int maxWeaponProjectileCount = 4;

        /// <summary>
        ///     The fire rate of the weapon
        /// </summary>
        [Tooltip("The fire rate of the weapon")]
        public float weaponFireRate = 10;

        /// <summary>
        ///     How long it takes for the weapon to reload (in milliseconds)
        /// </summary>
        [Header("Weapon Reloading")] [Tooltip("How long it takes for the weapon to reload (in milliseconds)")]
        public int weaponReloadTime = 2000;

        public CachedAddressable<GameObject> projectileObject;

        private int currentProjectileCount;
        private bool isReloading;

        private float nextTimeToFire;

        private GameObjectPoolBase projectileObjectsPool;

        private CancellationTokenSource reloadCancellation;
        private CancellationTokenSource shootRepeatedlyCancellation;
        private WeaponGraphics weaponGraphics;

        public override WeaponType WeaponType => WeaponType.Projectile;
        public override bool IsReloadable => true;

        public override void OnPerform(WeaponManager weaponManager, bool buttonDown)
        {
            if (buttonDown)
            {
                shootRepeatedlyCancellation?.Cancel();
                shootRepeatedlyCancellation = new CancellationTokenSource();
                TimeHelper.InvokeRepeatedly(() => FireWeapon(weaponManager), 1f / weaponFireRate, shootRepeatedlyCancellation.Token)
                    .Forget();
                return;
            }

            if (shootRepeatedlyCancellation == null)
                return;

            shootRepeatedlyCancellation.Cancel();
            shootRepeatedlyCancellation = null;
        }

        public override void OnReload(WeaponManager weaponManager)
        {
            if (isReloading)
                return;

            if (reloadCancellation != null)
                CancelReload();

            if (currentProjectileCount == maxWeaponProjectileCount)
                return;

            isReloading = true;
            reloadCancellation = new CancellationTokenSource();
            ReloadTask(weaponManager).Forget();
        }

        public override void OnSwitchOnTo(WeaponManager weaponManager)
        {
            if (isLocalClient)
            {
                HudAmmoControls controls = weaponManager.playerManager.PlayerUIManager.HudAmmoControls;

                controls.ammoText.gameObject.SetActive(true);
                controls.maxAmmoText.gameObject.SetActive(true);
                controls.reloadTextGameObject.gameObject.SetActive(isReloading);
            }

            if (!isServer)
                return;

            //Start reloading again if we switch to and we our out of projectiles
            if (currentProjectileCount <= 0)
                OnReload(weaponManager);

            UpdateUI(weaponManager);
        }

        public override void OnSwitchOff(WeaponManager weaponManager)
        {
            if (isLocalClient)
            {
                HudAmmoControls controls = weaponManager.playerManager.PlayerUIManager.HudAmmoControls;

                controls.ammoText.gameObject.SetActive(false);
                controls.maxAmmoText.gameObject.SetActive(false);
                controls.reloadTextGameObject.gameObject.SetActive(false);
            }

            if (!isServer)
                return;

            CancelReload();
            shootRepeatedlyCancellation?.Cancel();
        }

        protected override void OnAdd(WeaponManager weaponManager)
        {
            nextTimeToFire = 0f;
            currentProjectileCount = maxWeaponProjectileCount;
            isReloading = false;

            if (isServer)
                projectileObjectsPool = GameSceneManager.Instance.GetPoolByObject(projectileObject);

            if (isLocalClient)
                OnUIUpdate(weaponManager, new DefaultHudUpdateMessage(null, currentProjectileCount, isReloading));

            weaponGraphics = weaponObjectInstance.GetComponent<WeaponGraphics>();
            if (weaponGraphics == null)
                Logger.Error("Weapon model doesn't contain a weapon graphics!");
        }

        public override void OnRemove()
        {
            if (!isServer)
                return;

            CancelReload();
            shootRepeatedlyCancellation?.Cancel();
        }

        public override void OnWeaponEffects(WeaponManager weaponManager, IEffectsMessage effectsMessage)
        {
            if (effectsMessage is ProjectileEffectsMessage)
            {
                weaponGraphics.muzzleFlash.Play();
                weaponManager.WeaponRecoil.OnWeaponFire(weaponRecoilRotation);
                
                weaponManager.CameraEffects.OnWeaponFire(weaponCameraRecoilAmount);
            }
        }

        public override void OnUIUpdate(WeaponManager weaponManager, IHudUpdateMessage hudUpdateMessage)
        {
            if (hudUpdateMessage is DefaultHudUpdateMessage defaultHudUpdateMessage)
            {
                HudAmmoControls controls = weaponManager.playerManager.PlayerUIManager.HudAmmoControls;

                controls.ammoText.text = defaultHudUpdateMessage.CurrentBullets.ToString();
                controls.maxAmmoText.text = maxWeaponProjectileCount.ToString();
                controls.reloadTextGameObject.SetActive(defaultHudUpdateMessage.IsReloading);

                isReloading = defaultHudUpdateMessage.IsReloading;
            }
        }

        public override KeyValuePair<CachedAddressable<GameObject>, bool>[] GetObjectsNeededToBePooled()
        {
            return new[] { KeyValuePair.Create(projectileObject, true) };
        }

        [Server]
        private void FireWeapon(WeaponManager weaponManager)
        {
            //We out of projectiles, reload
            if (currentProjectileCount <= 0)
            {
                if (isReloading)
                    return;

                OnReload(weaponManager);
                return;
            }

            if (Time.time < nextTimeToFire)
                return;

            if (isReloading)
            {
                if (currentProjectileCount <= 0)
                    return;

                CancelReload();
            }

            nextTimeToFire = Time.time + 1f / weaponFireRate;
            currentProjectileCount--;
            DoProjectile(weaponManager);
            UpdateUI(weaponManager);
        }

        [Server]
        private void DoProjectile(WeaponManager weaponManager)
        {
            Transform projectileSpawnPoint = weaponGraphics.bulletTracerPosition;
            Transform playerFacingDirection = weaponManager.localPlayerCamera;

            //Figure out where the projectile should aim for
            RaycastHit[] hits = RaycastHelper.RaycastAllSorted(playerFacingDirection.position,
                playerFacingDirection.forward, float.MaxValue,
                weaponManager.raycastLayerMask);

            //We need to filter through each hit
            RaycastHit? raycastHit = null;
            foreach (RaycastHit hit in hits)
            {
                //Don't count if we hit the shooting player
                if (hit.collider.name == weaponManager.transform.name)
                    continue;

                raycastHit = hit;
            }

            //Spawn the object
            GameObject newProjectile = projectileObjectsPool.GetPooledObject();
            if (raycastHit.HasValue)
            {
                RaycastHit hit = raycastHit.Value;
                if (hit.distance > projectileAutoAimMinRange)
                {
                    newProjectile.transform.LookAt(hit.point);
                    Logger.Debug("Pointing player's projectile at cross-hair.");
                }
            }

            ProjectileBase projectile = newProjectile.GetComponent<ProjectileBase>();
            if (projectile == null)
            {
                Logger.Error("Weapon projectile doesn't have a projectile base on it!");
                return;
            }

            projectile.SetupOwner(weaponManager.playerManager);
            projectile.ServerEnable(projectileSpawnPoint.position, projectileSpawnPoint.rotation.eulerAngles);

            DoWeaponEffects(weaponManager, new ProjectileEffectsMessage());
            weaponManager.CameraEffects.OnWeaponFire(weaponCameraRecoilAmount);
        }

        [Server]
        private async UniTask ReloadTask(WeaponManager weaponManager)
        {
            Logger.Debug("Reloading player's weapon.");
            isReloading = true;
            UpdateUI(weaponManager);
            
            await TimeHelper.CountUp(
                maxWeaponProjectileCount - currentProjectileCount, weaponReloadTime, tick =>
                {
                    //Backup
                    if (currentProjectileCount == maxWeaponProjectileCount)
                        return;

                    //Increase projectiles
                    currentProjectileCount++;
                    UpdateUI(weaponManager);
                }, reloadCancellation.Token);
            
            isReloading = false;
            UpdateUI(weaponManager);
            reloadCancellation.Dispose();
            reloadCancellation = null;
        }

        [Server]
        private void CancelReload()
        {
            isReloading = false;

            if (reloadCancellation is {IsCancellationRequested: false})
            {
                reloadCancellation.Cancel();
                reloadCancellation.Dispose();
                reloadCancellation = null;
            }
        }

        [Server]
        private void UpdateUI(WeaponManager weaponManager)
        {
            DefaultHudUpdateMessage message = new(weaponId, currentProjectileCount, isReloading);
            Logger.Debug("Sent client UI weapon update: {@Message}", message);
            DoPlayerUIUpdate(weaponManager, message);
        }

        #region Networking

        protected override void OnSerialize(NetworkWriter writer)
        {
            writer.WriteString(weaponId);
            writer.WriteInt(currentProjectileCount);
            writer.WriteBool(isReloading);
        }

        public static WeaponProjectile OnDeserialize(NetworkReader reader)
        {
            string weaponId = reader.ReadString();
            WeaponProjectile weaponResource = WeaponsResourceManager.GetWeapon(weaponId) as WeaponProjectile;
            WeaponProjectile newWeapon = Instantiate(weaponResource);
            newWeapon.currentProjectileCount = reader.ReadInt();
            newWeapon.isReloading = reader.ReadBool();
            return newWeapon;
        }

        #endregion
    }
}