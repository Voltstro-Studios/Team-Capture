// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.Helper;
using Team_Capture.LagCompensation;
using Team_Capture.Player;
using Team_Capture.Pooling;
using Team_Capture.SceneManagement;
using Team_Capture.Weapons.Effects;
using Team_Capture.Weapons.UI;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;
using Random = UnityEngine.Random;

namespace Team_Capture.Weapons
{
    [CreateAssetMenu(fileName = "New Default Type Weapon", menuName = "Team-Capture/Weapons/Default (Raycast)")]
    public class WeaponDefault : WeaponBase
    {
        public int weaponDamage = 15;

        public float weaponFireRate = 10;
        
        //TODO: Lets remove range and add damage dropoff
        public int range = 50;

        public int bulletsPerShot = 1;
        
        public int maxBullets = 16;

        public float spreadFactor = 0.05f;
        public WeaponFireMode weaponFireMode;

        public int weaponReloadTime = 2000;
        public WeaponReloadMode weaponReloadMode;

        private int currentBulletCount;
        private bool isReloading;
        
        private float nextTimeToFire;
        private WeaponGraphics weaponGraphics;
        
        private CancellationTokenSource reloadCancellation;
        private CancellationTokenSource shootRepeatedlyCancellation;

        private GameObjectPool tracerPool;
        private GameObjectPool bulletHolesPool;
        
        public override WeaponType WeaponType => WeaponType.Default;

        public override void OnPerform(bool buttonDown)
        {
            if(buttonDown && weaponFireMode == WeaponFireMode.Semi)
                ShootWeapon();
            if (buttonDown && weaponFireMode == WeaponFireMode.Auto)
            {
                shootRepeatedlyCancellation?.Cancel();
                shootRepeatedlyCancellation = new CancellationTokenSource();
                TimeHelper.InvokeRepeatedly(ShootWeapon, 1f / weaponFireRate, shootRepeatedlyCancellation.Token).Forget();
            }

            if (!buttonDown && weaponFireMode == WeaponFireMode.Auto)
            {
                if (shootRepeatedlyCancellation != null)
                {
                    shootRepeatedlyCancellation.Cancel();
                    shootRepeatedlyCancellation = null;
                }
            }
        }

        private void ShootWeapon()
        {
            //We out of bullets, reload
            if (currentBulletCount <= 0)
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
                if(currentBulletCount <= 0)
                    return;
                
                CancelReload();
            }

            nextTimeToFire = Time.time + 1f / weaponFireRate;
            currentBulletCount--;
            try
            {
                SimulationHelper.SimulateCommand(weaponManager.playerManager, WeaponRayCast);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occured while simulating weapon shooting!");
            }
            
            UpdateUI();
        }

        public override void OnReload()
        {
            if(isReloading)
                return;
            
            if(reloadCancellation != null)
                CancelReload();
            
            isReloading = true;
            reloadCancellation = new CancellationTokenSource();
            ReloadTask().Forget();
        }
        
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

        private async UniTask ReloadTask()
        {
            Logger.Debug("Reloading player's weapon.");
            UpdateUI();

            switch (weaponReloadMode)
            {
                case WeaponReloadMode.Clip:
                    await UniTask.Delay(weaponReloadTime, cancellationToken: reloadCancellation.Token);

                    currentBulletCount = maxBullets;
                    FinishReload();
                    break;
                case WeaponReloadMode.Shells:
                    await TimeHelper.CountUp(
                        maxBullets - currentBulletCount, weaponReloadTime, tick =>
                        {
                            //Backup
                            if (currentBulletCount == maxBullets)
                                return;

                            //Increase bullets
                            currentBulletCount++;
                            UpdateUI();
                        }, reloadCancellation.Token);
                    
                    FinishReload();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FinishReload()
        {
            isReloading = false;
            UpdateUI();
            reloadCancellation.Dispose();
            reloadCancellation = null;
        }

        public override void OnWeaponEffects(IEffectsMessage effectsMessage)
        {
            if (effectsMessage is DefaultEffectsMessage defaultEffects)
            {
                weaponGraphics.muzzleFlash.Play();
                
                for (int i = 0; i < defaultEffects.Targets.Length; i++)
                {
                    //Do bullet tracer
                    GameObject tracerObject = tracerPool.GetPooledObject();
                    tracerObject.transform.position = weaponGraphics.bulletTracerPosition.position;
                    tracerObject.transform.rotation = weaponGraphics.bulletTracerPosition.rotation;

                    BulletTracer tracer = tracerObject.GetComponent<BulletTracer>();
                    tracer.Play(defaultEffects.Targets[i]);

                    //Do bullet holes
                    GameObject bulletHole = bulletHolesPool.GetPooledObject();
                    bulletHole.transform.position = defaultEffects.Targets[i];
                    bulletHole.transform.rotation = Quaternion.LookRotation(defaultEffects.TargetsNormals[i]);
                }
            }
        }

        public override void OnUIUpdate(IHudUpdateMessage hudUpdateMessage)
        {
            if (hudUpdateMessage is DefaultHudUpdateMessage defaultHudUpdateMessage)
            {
                HudAmmoControls.ammoText.text = defaultHudUpdateMessage.CurrentBullets.ToString();
                HudAmmoControls.maxAmmoText.text = maxBullets.ToString();
                HudAmmoControls.reloadTextGameObject.SetActive(defaultHudUpdateMessage.IsReloading);
            }
        }

        public override void OnSwitchOnTo()
        {
            //Start reloading again if we switch to and we our out of bullets
            if (currentBulletCount <= 0)
                OnReload();
            
            UpdateUI();
        }

        public override void OnSwitchOff()
        {
            CancelReload();
            shootRepeatedlyCancellation?.Cancel();
        }

        protected override void OnAdd()
        {
            if (isServer)
            {
                nextTimeToFire = 0f;
                currentBulletCount = maxBullets;
                isReloading = false;
                UpdateUI();
            }

            if (isLocalClient)
            {
                OnUIUpdate(new DefaultHudUpdateMessage(null, currentBulletCount, isReloading));
            }

            tracerPool = GameSceneManager.Instance.tracersEffectsPool;
            bulletHolesPool = GameSceneManager.Instance.bulletHolePool;
            
            weaponGraphics = weaponObjectInstance.GetComponent<WeaponGraphics>();
            if (weaponGraphics == null)
                Logger.Error("Weapon model doesn't contain a weapon graphics!");
        }

        public override void OnRemove()
        {
            shootRepeatedlyCancellation?.Cancel();
            CancelReload();
        }

        [Server]
        private void WeaponRayCast()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Transform playerFacingDirection = weaponManager.localPlayerCamera;
            List<Vector3> targets = new();
            List<Vector3> targetsNormal = new();

            for (int i = 0; i < bulletsPerShot; i++)
            {
                //Calculate random spread
                Vector3 direction = playerFacingDirection.forward;
                direction += playerFacingDirection.TransformDirection(new Vector3(
                    Random.Range(-spreadFactor, spreadFactor),
                    Random.Range(-spreadFactor, spreadFactor),
                    Random.Range(-spreadFactor, spreadFactor)));
                
                //Do our raycast
                RaycastHit[] hits = RaycastHelper.RaycastAllSorted(playerFacingDirection.position, direction,
                    range, weaponManager.raycastLayerMask);
                foreach (RaycastHit hit in hits)
                {
                    //Don't count if we hit the shooting player
                    if (hit.collider.name == weaponManager.transform.name)
                        continue;

                    //Do impact effect on all clients
                    targets.Add(hit.point);
                    targetsNormal.Add(hit.normal);

                    //So if we hit a player then do damage
                    PlayerManager hitPlayer = hit.collider.GetComponent<PlayerManager>();
                    if (hitPlayer == null) break;

                    hitPlayer.TakeDamage(weaponDamage, weaponManager.transform.name);
                    break;
                }
            }
            
            DoWeaponEffects(new DefaultEffectsMessage(targets.ToArray(), targetsNormal.ToArray()));
            
            stopwatch.Stop();
            Logger.Debug("Took {Milliseconds} to fire weapon.", stopwatch.Elapsed.TotalMilliseconds);
        }

        private void UpdateUI()
        {
            DefaultHudUpdateMessage message = new(weaponId, currentBulletCount, isReloading);
            Logger.Debug("Sent client UI weapon update: {@Message}", message);
            DoPlayerUIUpdate(message);
        }

        #region Networking

        protected override void OnSerialize(NetworkWriter writer)
        {
            writer.WriteString(weaponId);
            writer.WriteInt(currentBulletCount);
            writer.WriteBool(isReloading);
        }

        internal static WeaponDefault OnDeserialize(NetworkReader reader)
        {
            string weaponId = reader.ReadString();
            WeaponDefault weapon = WeaponsResourceManager.GetWeapon(weaponId) as WeaponDefault;
            weapon.currentBulletCount = reader.ReadInt();
            weapon.isReloading = reader.ReadBool();
            return weapon;
        }

        #endregion
    }
}