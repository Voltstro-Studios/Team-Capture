using TMPro;
using UnityEngine;
using Weapons;

namespace UI
{
	public class Hud : MonoBehaviour
	{
		[Header("Health")]
		public TextMeshProUGUI healthText;

		[Header("Ammo")] 
		public TextMeshProUGUI ammoText;
		public TextMeshProUGUI maxAmmoText;
		public GameObject reloadTextGameObject;
	
		[HideInInspector] public ClientUI clientUi;

		public void UpdateHealthUi()
		{
			healthText.text = clientUi.player.GetHealth.ToString();
		}

		public void UpdateAmmoUi(WeaponManager weaponManager)
		{
			TCWeapon activeWeapon = weaponManager.GetActiveWeapon();
			if(activeWeapon == null) return;

			ammoText.text = activeWeapon.currentBulletsAmount.ToString();
			maxAmmoText.text = activeWeapon.maxBullets.ToString();

			reloadTextGameObject.SetActive(activeWeapon.isReloading);
		}
	}
}
