// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Helper;
using Team_Capture.Helper.Extensions;
using Team_Capture.Weapons.Effects;
using Team_Capture.Weapons.UI;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Weapons
{
    [CreateAssetMenu(fileName = "New Projectile Type Weapon", menuName = "Team-Capture/Weapons/Projectile")]
    public class WeaponProjectile : WeaponBase
    {
        public GameObject projectilePrefab;

        public float projectileAutoAimMinRange = 10f;
        
        public override WeaponType WeaponType => WeaponType.Projectile;
        public override bool IsReloadable => true;
        
        private WeaponGraphics weaponGraphics;
        
        public override void OnPerform(bool buttonDown)
        {
            if (buttonDown)
            {
                FireWeapon();
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
            
        }

        protected override void OnAdd()
        {
            weaponGraphics = weaponObjectInstance.GetComponent<WeaponGraphics>();
            if (weaponGraphics == null)
                Logger.Error("Weapon model doesn't contain a weapon graphics!");
        }

        public override void OnRemove()
        {
            
        }

        public override void OnWeaponEffects(IEffectsMessage effectsMessage)
        {
            
        }

        public override void OnUIUpdate(IHudUpdateMessage hudUpdateMessage)
        {
            
        }

        protected override void OnSerialize(NetworkWriter writer)
        {
            writer.WriteString(weaponId);
        }

        public static WeaponProjectile OnDeserialize(NetworkReader reader)
        {
            string weaponId = reader.ReadString();
            WeaponProjectile weapon = WeaponsResourceManager.GetWeapon(weaponId) as WeaponProjectile;
            return weapon;
        }

        [Server]
        private void FireWeapon()
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
            GameObject newProjectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            if (raycastHit.HasValue)
            {
                RaycastHit hit = raycastHit.Value;
                if (hit.distance > projectileAutoAimMinRange)
                {
                    newProjectile.transform.LookAt(hit.point);
                    Logger.Debug("Pointing player's projectile at cross-hair.");
                }
            }
            
            NetworkServer.Spawn(newProjectile);
        }
    }
}