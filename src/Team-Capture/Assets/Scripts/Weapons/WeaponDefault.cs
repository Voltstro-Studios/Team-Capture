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
using Team_Capture.AddressablesAddons;
using Team_Capture.Helper;
using Team_Capture.LagCompensation;
using Team_Capture.Player;
using Team_Capture.Pooling;
using Team_Capture.SceneManagement;
using Team_Capture.UI;
using Team_Capture.Weapons.Effects;
using Team_Capture.Weapons.Jobs;
using Team_Capture.Weapons.UI;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;
using Random = UnityEngine.Random;

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     A simple type of weapon that uses ray casting to know who it hits
    /// </summary>
    [CreateAssetMenu(fileName = "New Default Type Weapon", menuName = "Team-Capture/Weapons/Default (Raycast)")]
    public class WeaponDefault : WeaponBase
    {
        /// <summary>
        ///     How much damage the weapon does
        /// </summary>
        [Header("Weapon Damage")] [Tooltip("How much damage the weapon does")]
        public int weaponDamage = 15;

        /// <summary>
        ///     Based on distance, what is the weapon's damage dropoff.
        ///     <para>
        ///         Dropoff is calculated by a curve that is evaluated.
        ///         The evaluated value is then multiplied with the weapon's damage.
        ///     </para>
        ///     <para>X is distance. Y is multiplier.</para>
        /// </summary>
        [Tooltip("Based on distance, what is the weapon's damage dropoff.\n" +
                 "Dropoff is calculated by a curve that is evaluated. The evaluated value is then multiplied with the weapon's damage.\n" +
                 "X is distance. Y is multiplier.")]
        public AnimationCurve weaponDamageDropOff = new(
            new Keyframe(0, 1, 0, 0),
            new Keyframe(8, 1, 0, -0.01403509f, 0.5f, 0.322204f),
            new Keyframe(65, 0.2f, -0.01403509f, -0.01377508f, 0.3634869f, 0.5f));

        /// <summary>
        ///     The fire rate of the weapon
        /// </summary>
        [Header("Weapon Raycast Settings")] [Tooltip("The fire rate of the weapon")]
        public float weaponFireRate = 10;

        /// <summary>
        ///     How many bullets to do when the weapon is shot
        /// </summary>
        [Tooltip("How many bullets to do when the weapon is shot")]
        public int bulletsPerShot = 1;

        /// <summary>
        ///     The max amount of bullets to hold
        /// </summary>
        [Tooltip("The max amount of bullets to hold")]
        public int maxBullets = 16;

        /// <summary>
        ///     The weapon's spread factor
        /// </summary>
        [Tooltip("The weapon's spread factor")]
        public float spreadFactor = 0.05f;

        /// <summary>
        ///     The <see cref="WeaponFireMode" />
        /// </summary>
        [Tooltip("The weapon's fire mode")] public WeaponFireMode weaponFireMode;

        /// <summary>
        ///     How long it takes for the weapon to reload (in milliseconds)
        /// </summary>
        [Header("Weapon Reloading")] [Tooltip("How long it takes for the weapon to reload (in milliseconds)")]
        public int weaponReloadTime = 2000;

        /// <summary>
        ///     What the weapon's <see cref="WeaponDefaultReloadMode" /> is
        /// </summary>
        [Tooltip("What the weapon's reload mode is")]
        public WeaponDefaultReloadMode weaponReloadMode;

        public CachedAddressable<GameObject> weaponTracer;
        public CachedAddressable<GameObject> weaponBulletHole;
        
        private int currentBulletCount;
        private bool isReloading;

        private float nextTimeToFire;

        private CancellationTokenSource reloadCancellation;
        private CancellationTokenSource shootRepeatedlyCancellation;

        private GameObjectPoolBase bulletHolesPool;
        private GameObjectPoolBase tracerPool;
        
        private WeaponGraphics weaponGraphics;

        public override WeaponType WeaponType => WeaponType.Default;

        public override bool IsReloadable => true;

        public override void OnPerform(WeaponManager weaponManager, bool buttonDown)
        {
            if (buttonDown && weaponFireMode == WeaponFireMode.Semi)
                ShootWeapon(weaponManager);
            if (buttonDown && weaponFireMode == WeaponFireMode.Auto)
            {
                shootRepeatedlyCancellation?.Cancel();
                shootRepeatedlyCancellation = new CancellationTokenSource();
                TimeHelper.InvokeRepeatedly(() => ShootWeapon(weaponManager), 1f / weaponFireRate, shootRepeatedlyCancellation.Token)
                    .Forget();
            }

            if (!buttonDown && weaponFireMode == WeaponFireMode.Auto)
                if (shootRepeatedlyCancellation != null)
                {
                    shootRepeatedlyCancellation.Cancel();
                    shootRepeatedlyCancellation = null;
                }
        }

        public override void OnReload(WeaponManager weaponManager)
        {
            if (isReloading)
                return;

            if (reloadCancellation != null)
                CancelReload();

            if (currentBulletCount == maxBullets)
                return;

            isReloading = true;
            reloadCancellation = new CancellationTokenSource();
            ReloadTask(weaponManager).Forget();
        }

        public override void OnWeaponEffects(WeaponManager weaponManager, IEffectsMessage effectsMessage)
        {
            if (effectsMessage is DefaultEffectsMessage defaultEffects)
            {
                //Muzzle flash and recoil
                weaponGraphics.muzzleFlash.Play();
                
                weaponManager.WeaponRecoil.OnWeaponFire(weaponRecoilRotation);
                weaponManager.CameraEffects.OnWeaponFire(weaponCameraRecoilAmount);

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

        public override void OnUIUpdate(WeaponManager weaponManager, IHudUpdateMessage hudUpdateMessage)
        {
            if (hudUpdateMessage is DefaultHudUpdateMessage defaultHudUpdateMessage)
            {
                HudAmmoControls controls = weaponManager.playerManager.PlayerUIManager.HudAmmoControls;

                controls.ammoText.text = defaultHudUpdateMessage.CurrentBullets.ToString();
                controls.maxAmmoText.text = maxBullets.ToString();
                controls.reloadTextGameObject.SetActive(defaultHudUpdateMessage.IsReloading);

                isReloading = defaultHudUpdateMessage.IsReloading;
            }
        }

        public override KeyValuePair<CachedAddressable<GameObject>, bool>[] GetObjectsNeededToBePooled()
        {
            return new[] { KeyValuePair.Create(weaponBulletHole, false), KeyValuePair.Create(weaponTracer, false) };
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

            //Start reloading again if we switch to and we our out of bullets
            if (currentBulletCount <= 0)
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
            currentBulletCount = maxBullets;
            isReloading = false;

            if (weaponManager == null)
            {
                Logger.Error("The weapon manager is null!");
                throw new NullReferenceException("The weapon manager is null!");
            }

            if (isLocalClient)
                OnUIUpdate(weaponManager, new DefaultHudUpdateMessage(null, currentBulletCount, isReloading));

            tracerPool = GameSceneManager.Instance.GetPoolByObject(weaponTracer);
            bulletHolesPool = GameSceneManager.Instance.GetPoolByObject(weaponBulletHole);

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
        private void UpdateUI(WeaponManager weaponManager)
        {
            DefaultHudUpdateMessage message = new(weaponId, currentBulletCount, isReloading);
            DoPlayerUIUpdate(weaponManager, message);
        }

        #region Weapon Shooting

        [Server]
        private void ShootWeapon(WeaponManager weaponManager)
        {
            //We out of bullets, reload
            if (currentBulletCount <= 0)
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
                if (currentBulletCount <= 0)
                    return;

                CancelReload();
            }

            nextTimeToFire = Time.time + 1f / weaponFireRate;
            currentBulletCount--;
            try
            {
                LagCompensationManager.Simulate(weaponManager.playerManager, () => WeaponRayCast(weaponManager));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occured while simulating weapon shooting!");
            }

            UpdateUI(weaponManager);
        }

        [Server]
        private void WeaponRayCast(WeaponManager weaponManager)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Transform playerFacingDirection = weaponManager.localPlayerCamera;
            List<Vector3> targets = new();
            List<Vector3> targetsNormal = new();

            //Calculate directions first
            Stopwatch directionSetup = Stopwatch.StartNew();
            NativeArray<Vector3> directions = new(bulletsPerShot, Allocator.TempJob);
            CreateDirectionsJob createDirectionsJob = new()
            {
                Directions = directions,
                SpreadFactor = spreadFactor,
                Random = new Unity.Mathematics.Random((uint) Random.Range(uint.MinValue, uint.MaxValue))
            };
            createDirectionsJob.Schedule(bulletsPerShot, 32).Complete();

            directionSetup.Stop();
            Logger.Debug("Took {Milliseconds} to setup directions.", directionSetup.Elapsed.TotalMilliseconds);

            for (int i = 0; i < bulletsPerShot; i++)
            {
                //Calculate random spread
                Vector3 direction = playerFacingDirection.forward;
                direction += playerFacingDirection.TransformDirection(directions[i]);

                //Do our raycast
                RaycastHit[] hits = RaycastHelper.RaycastAllSorted(playerFacingDirection.position, direction,
                    float.MaxValue, weaponManager.raycastLayerMask);
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
                    if (hitPlayer == null)
                        break;

                    //Calculate damage
                    float damageMultiplier = weaponDamageDropOff.Evaluate(hit.distance);
                    int damage = Mathf.FloorToInt(weaponDamage * damageMultiplier);

                    hitPlayer.TakeDamage(damage, weaponManager.transform.name);
                    break;
                }
            }

            directions.Dispose();

            weaponManager.CameraEffects.OnWeaponFire(weaponCameraRecoilAmount);
            DoWeaponEffects(weaponManager, new DefaultEffectsMessage(targets.ToArray(), targetsNormal.ToArray()));

            stopwatch.Stop();
            Logger.Debug("Took {Milliseconds} to fire weapon.", stopwatch.Elapsed.TotalMilliseconds);
        }

        #endregion

        #region Weapon Reloading

        [Server]
        private async UniTask ReloadTask(WeaponManager weaponManager)
        {
            UpdateUI(weaponManager);

            switch (weaponReloadMode)
            {
                case WeaponDefaultReloadMode.Clip:
                    await UniTask.Delay(weaponReloadTime, cancellationToken: reloadCancellation.Token);

                    currentBulletCount = maxBullets;
                    FinishReload(weaponManager);
                    break;
                case WeaponDefaultReloadMode.Shells:
                    await TimeHelper.CountUp(
                        maxBullets - currentBulletCount, weaponReloadTime, tick =>
                        {
                            //Backup
                            if (currentBulletCount == maxBullets)
                                return;

                            //Increase bullets
                            currentBulletCount++;
                            UpdateUI(weaponManager);
                        }, reloadCancellation.Token);

                    FinishReload(weaponManager);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Server]
        private void FinishReload(WeaponManager weaponManager)
        {
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

        #endregion

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
            WeaponDefault weaponResource = WeaponsResourceManager.GetWeapon(weaponId) as WeaponDefault;
            WeaponDefault newWeapon = Instantiate(weaponResource);
            newWeapon.currentBulletCount = reader.ReadInt();
            newWeapon.isReloading = reader.ReadBool();
            return newWeapon;
        }

        #endregion
    }
}