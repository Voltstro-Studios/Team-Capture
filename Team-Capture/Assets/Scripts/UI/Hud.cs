using TMPro;
using UnityEngine;
using Weapons;

namespace UI
{
	public class Hud : MonoBehaviour
	{
		[Header("Ammo")] public TextMeshProUGUI ammoText;

		[HideInInspector] public ClientUI clientUi;

		[Header("Health")] public TextMeshProUGUI healthText;

		public TextMeshProUGUI maxAmmoText;
		public GameObject reloadTextGameObject;

		public void UpdateHealthUi()
		{
			healthText.text = clientUi.player.Health.ToString();
		}

		public void UpdateAmmoUi(WeaponManager weaponManager)
		{
			TCWeapon activeWeapon = weaponManager.GetActiveWeapon();
			if (activeWeapon == null) return;

			ammoText.text = activeWeapon.currentBulletsAmount.ToString();
			maxAmmoText.text = activeWeapon.maxBullets.ToString();

			reloadTextGameObject.SetActive(activeWeapon.isReloading);
		}
	}
}