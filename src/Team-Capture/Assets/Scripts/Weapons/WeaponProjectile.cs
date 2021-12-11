// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
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
            Transform rocketSpawnPoint = weaponGraphics.bulletTracerPosition;

            GameObject newProjectile = Instantiate(projectilePrefab, rocketSpawnPoint.position, rocketSpawnPoint.rotation);
            NetworkServer.Spawn(newProjectile);
        }
    }
}