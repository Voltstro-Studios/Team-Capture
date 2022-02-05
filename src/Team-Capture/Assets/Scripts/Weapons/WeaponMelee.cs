// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
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

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     Melee based weapon
    /// </summary>
    [CreateAssetMenu(fileName = "New Melee Type Weapon", menuName = "Team-Capture/Weapons/Melee")]
    internal class WeaponMelee : WeaponBase
    {
        /// <summary>
        ///     How much damage does the weapon do per hit
        /// </summary>
        [Header("Weapon Damage")] [Tooltip("How much damage does the weapon do per hit")]
        public int weaponDamage = 25;

        /// <summary>
        ///     The fire rate of the weapon
        /// </summary>
        [Header("Weapon Raycast Settings")] [Tooltip("The fire rate of the weapon")]
        public float weaponFireRate = 10;

        /// <summary>
        ///     How far does the weapon hit
        /// </summary>
        [Tooltip("How far does the weapon hit")]
        public float weaponRange = 25;

        private GameObjectPool bulletHolesPool;

        private float nextTimeToFire;
        private CancellationTokenSource shootRepeatedlyCancellation;

        public override WeaponType WeaponType => WeaponType.Melee;

        public override bool IsReloadable => false;

        public override void OnPerform(bool buttonDown)
        {
            //If the button is pressed or held down, perform
            if (buttonDown)
            {
                shootRepeatedlyCancellation?.Cancel();
                shootRepeatedlyCancellation = new CancellationTokenSource();
                TimeHelper.InvokeRepeatedly(SwingWeapon, 1f / weaponFireRate, shootRepeatedlyCancellation.Token)
                    .Forget();
            }
            else
            {
                if (shootRepeatedlyCancellation != null)
                {
                    shootRepeatedlyCancellation.Cancel();
                    shootRepeatedlyCancellation = null;
                }
            }
        }

        public override void OnReload()
        {
        }

        public override void OnSwitchOnTo()
        {
        }

        public override void OnSwitchOff()
        {
            shootRepeatedlyCancellation?.Cancel();
        }

        protected override void OnAdd()
        {
            bulletHolesPool = GameSceneManager.Instance.bulletHolePool;
            nextTimeToFire = 0f;
        }

        public override void OnRemove()
        {
            shootRepeatedlyCancellation?.Cancel();
        }

        public override void OnWeaponEffects(IEffectsMessage effectsMessage)
        {
            if (effectsMessage is MeleeEffectsMessage meleeEffectsMessage)
            {
                if (!meleeEffectsMessage.HitNormal.HasValue || !meleeEffectsMessage.HitPoint.HasValue)
                    return;

                //Do hole (your mum's hole)
                GameObject weaponHitHole = bulletHolesPool.GetPooledObject();
                weaponHitHole.transform.position = meleeEffectsMessage.HitPoint.Value;
                weaponHitHole.transform.rotation = Quaternion.LookRotation(meleeEffectsMessage.HitNormal.Value);
            }
        }

        public override void OnUIUpdate(IHudUpdateMessage hudUpdateMessage)
        {
        }

        protected override void OnSerialize(NetworkWriter writer)
        {
            writer.WriteString(weaponId);
        }

        private void SwingWeapon()
        {
            if (Time.time < nextTimeToFire)
                return;

            nextTimeToFire = Time.time + 1f / weaponFireRate;

            try
            {
                LagCompensationManager.Simulate(weaponManager.playerManager, WeaponRayCast);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occured while simulating weapon shooting!");
            }
        }

        private void WeaponRayCast()
        {
            Transform playerFacingDirection = weaponManager.localPlayerCamera;

            Vector3 direction = playerFacingDirection.forward;

            RaycastHit[] hits = RaycastHelper.RaycastAllSorted(playerFacingDirection.position, direction, weaponRange,
                weaponManager.raycastLayerMask);

            Vector3? hitPoint = null;
            Vector3? hitNormal = null;

            //We need to filter through each hit
            foreach (RaycastHit hit in hits)
            {
                //Don't count if we hit the shooting player
                if (hit.collider.name == weaponManager.transform.name)
                    continue;

                hitPoint = hit.point;
                hitNormal = hit.normal;

                //So if we hit a player then do damage
                PlayerManager hitPlayer = hit.collider.GetComponent<PlayerManager>();
                if (hitPlayer == null)
                    break;

                hitPlayer.TakeDamage(weaponDamage, weaponManager.transform.name);
                break;
            }

            DoWeaponEffects(new MeleeEffectsMessage(hitPoint, hitNormal));
        }

        internal static WeaponMelee OnDeserialize(NetworkReader reader)
        {
            string weaponId = reader.ReadString();
            WeaponMelee weapon = WeaponsResourceManager.GetWeapon(weaponId) as WeaponMelee;
            return weapon;
        }
    }
}