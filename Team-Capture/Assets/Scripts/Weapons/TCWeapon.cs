using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
    public class TCWeapon : ScriptableObject
    {
        public GameObject baseWeaponPrefab;

        public int damage;
        public int range;
        public string weapon;
        public string weaponName;
    }
}