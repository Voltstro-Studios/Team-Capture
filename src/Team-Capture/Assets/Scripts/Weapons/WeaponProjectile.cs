// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.Helper;
using Team_Capture.Pooling;
using Team_Capture.SceneManagement;
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
        ///     The <see cref="GameObject"/> that the weapon will spawn on fire
        /// </summary>
        public GameObject projectilePrefab;

        /// <summary>
        ///     How far does something have to be for "auto aim" to kick in
        /// </summary>
        public float projectileAutoAimMinRange = 10f;

        /// <summary>
        ///     How many projectiles to hold
        /// </summary>
        public int maxWeaponProjectileCount = 4;
        
        /// <summary>
        ///     The fire rate of the weapon
        /// </summary>
        [Tooltip("The fire rate of the weapon")]
        public float weaponFireRate = 10;
        
        /// <summary>
        ///     How long it takes for the weapon to reload (in milliseconds)
        /// </summary>
        [Tooltip("How long it takes for the weapon to reload (in milliseconds)")]
        public int weaponReloadTime = 2000;
        
        public override WeaponType WeaponType => WeaponType.Projectile;
        public override bool IsReloadable => true;
        
        private int currentProjectileCount;
        private bool isReloading;
        
        private float nextTimeToFire;
        private WeaponGraphics weaponGraphics;
        
        private CancellationTokenSource reloadCancellation;
        private CancellationTokenSource shootRepeatedlyCancellation;

        private NetworkProjectileObjectsPool projectileObjectsPool;
        
        public override void OnPerform(bool buttonDown)
        {
            if (buttonDown)
            {
                shootRepeatedlyCancellation?.Cancel();
                shootRepeatedlyCancellation = new CancellationTokenSource();
                TimeHelper.InvokeRepeatedly(FireWeapon, 1f / weaponFireRate, shootRepeatedlyCancellation.Token).Forget();
                return;
            }

            if (shootRepeatedlyCancellation == null) 
                return;
            
            shootRepeatedlyCancellation.Cancel();
            shootRepeatedlyCancellation = null;
        }

        public override void OnReload()
        {
            if(isReloading)
                return;
            
            if(reloadCancellation != null)
                CancelReload();
            
            if(currentProjectileCount == maxWeaponProjectileCount)
                return;
            
            isReloading = true;
            reloadCancellation = new CancellationTokenSource();
            ReloadTask().Forget();
        }

        public override void OnSwitchOnTo()
        {
            if (isLocalClient)
            {
                HudAmmoControls.ammoText.gameObject.SetActive(true);
                HudAmmoControls.maxAmmoText.gameObject.SetActive(true);
                HudAmmoControls.reloadTextGameObject.gameObject.SetActive(isReloading);
            }
            
            if(!isServer)
                return;
            
            //Start reloading again if we switch to and we our out of projectiles
            if (currentProjectileCount <= 0)
                OnReload();
            
            UpdateUI();
        }

        public override void OnSwitchOff()
        {
            if (isLocalClient)
            {
                HudAmmoControls.ammoText.gameObject.SetActive(false);
                HudAmmoControls.maxAmmoText.gameObject.SetActive(false);
                HudAmmoControls.reloadTextGameObject.gameObject.SetActive(false);
            }
            
            if(!isServer)
                return;
            
            CancelReload();
            shootRepeatedlyCancellation?.Cancel();
        }

        protected override void OnAdd()
        {
            nextTimeToFire = 0f;
            currentProjectileCount = maxWeaponProjectileCount;
            isReloading = false;
            
            if (isServer)
                projectileObjectsPool = GameSceneManager.Instance.rocketsPool;
            
            if (isLocalClient)
                OnUIUpdate(new DefaultHudUpdateMessage(null, currentProjectileCount, isReloading));

            weaponGraphics = weaponObjectInstance.GetComponent<WeaponGraphics>();
            if (weaponGraphics == null)
                Logger.Error("Weapon model doesn't contain a weapon graphics!");
        }

        public override void OnRemove()
        {
            if(!isServer)
                return;
            
            CancelReload();
            shootRepeatedlyCancellation?.Cancel();
        }

        public override void OnWeaponEffects(IEffectsMessage effectsMessage)
        {
            if (effectsMessage is ProjectileEffectsMessage)
            {
                weaponGraphics.muzzleFlash.Play();
            }
        }

        public override void OnUIUpdate(IHudUpdateMessage hudUpdateMessage)
        {
            if (hudUpdateMessage is DefaultHudUpdateMessage defaultHudUpdateMessage)
            {
                HudAmmoControls.ammoText.text = defaultHudUpdateMessage.CurrentBullets.ToString();
                HudAmmoControls.maxAmmoText.text = maxWeaponProjectileCount.ToString();
                HudAmmoControls.reloadTextGameObject.SetActive(defaultHudUpdateMessage.IsReloading);

                isReloading = defaultHudUpdateMessage.IsReloading;
            }
        }

        protected override void OnSerialize(NetworkWriter writer)
        {
            writer.WriteString(weaponId);
            writer.WriteInt(currentProjectileCount);
            writer.WriteBool(isReloading);
        }

        public static WeaponProjectile OnDeserialize(NetworkReader reader)
        {
            string weaponId = reader.ReadString();
            WeaponProjectile weapon = WeaponsResourceManager.GetWeapon(weaponId) as WeaponProjectile;
            weapon.currentProjectileCount = reader.ReadInt();
            weapon.isReloading = reader.ReadBool();
            return weapon;
        }

        [Server]
        private void FireWeapon()
        {
            //We out of projectiles, reload
            if (currentProjectileCount <= 0)
            {
                if(isReloading)
                    return;
                
                OnReload();
                return;
            }

            if(Time.time < nextTimeToFire)
                return;
            
            if (isReloading)
            {
                if(currentProjectileCount <= 0)
                    return;
                
                CancelReload();
            }

            nextTimeToFire = Time.time + 1f / weaponFireRate;
            currentProjectileCount--;
            DoProjectile();
            UpdateUI();
        }

        [Server]
        private void DoProjectile()
        {
            Transform projectileSpawnPoint = weaponGraphics.bulletTracerPosition;
            Transform playerFacingDirection = weaponManager.localPlayerCamera;
            
            //Figure out where the projectile should aim for
            RaycastHit[] hits = RaycastHelper.RaycastAllSorted(playerFacingDirection.position, playerFacingDirection.forward, float.MaxValue, 
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

            DoWeaponEffects(new ProjectileEffectsMessage());
        }

        [Server]
        private async UniTask ReloadTask()
        {
            Logger.Debug("Reloading player's weapon.");
            UpdateUI();
            
            await UniTask.Delay(weaponReloadTime, cancellationToken: reloadCancellation.Token);

            currentProjectileCount = maxWeaponProjectileCount;
            isReloading = false;
            UpdateUI();
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
        private void UpdateUI()
        {
            DefaultHudUpdateMessage message = new(weaponId, currentProjectileCount, isReloading);
            Logger.Debug("Sent client UI weapon update: {@Message}", message);
            DoPlayerUIUpdate(message);
        }
    }
}