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

		public void UpdateAmmoUI(NetworkedWeapon syncMessage)
		{
			TCWeapon weapon = clientUI.WeaponManager.GetActiveWeapon().GetTCWeapon();

			if (syncMessage == null)
			{
				ammoText.text = clientUI.WeaponManager.GetActiveWeapon().CurrentBulletAmount.ToString();
				maxAmmoText.text = weapon.maxBullets.ToString();
				reloadTextGameObject.SetActive(clientUI.WeaponManager.GetActiveWeapon().IsReloading);

				return;
			}

			ammoText.text = syncMessage.CurrentBulletAmount.ToString();
			maxAmmoText.text = weapon.maxBullets.ToString();
			reloadTextGameObject.SetActive(syncMessage.IsReloading);
		}
	}
}