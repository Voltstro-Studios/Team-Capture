using TMPro;
using UnityEngine;
using Weapons;

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
			if(clientUI == null || clientUI.WeaponManager == null)
				return;

			NetworkedWeapon netWeapon = clientUI.WeaponManager.CurrentWeaponStatus;
			if(netWeapon == null)
				return;

			TCWeapon weapon = netWeapon.GetTCWeapon();

			ammoText.text = netWeapon.CurrentBulletAmount.ToString();
			maxAmmoText.text = weapon.maxBullets.ToString();
			reloadTextGameObject.SetActive(netWeapon.IsReloading);
		}
	}
}