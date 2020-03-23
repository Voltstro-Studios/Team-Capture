using Core.Logger;
using TMPro;
using UnityEngine;
using Weapons;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class Hud : MonoBehaviour
	{
		public TextMeshProUGUI ammoText;
		public TextMeshProUGUI maxAmmoText;
		public GameObject reloadTextGameObject;

		public TextMeshProUGUI healthText;

		private ClientUI clientUI;

		internal void Setup(ClientUI ui)
		{
			clientUI = ui;
		}

		public void UpdateHealthUI()
		{
			healthText.text = clientUI.PlayerManager.Health.ToString();
		}

		public void UpdateAmmoUI()
		{
			NetworkedWeapon activeWeapon = clientUI.WeaponManager.GetActiveWeapon();
			if (activeWeapon == null) return;

			ammoText.text = activeWeapon.currentBulletAmount.ToString();
			//maxAmmoText.text = activeWeapon.maxBullets.ToString();

			reloadTextGameObject.SetActive(activeWeapon.IsReloading);

			Logger.Log("Updated ammo UI", LogVerbosity.Debug);
		}
	}
}