using Core.Logger;
using Core.Networking.Messages;
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

		public void UpdateAmmoUI(WeaponSyncMessage syncMessage)
		{
			TCWeapon weapon = clientUI.WeaponManager.GetActiveWeapon().GetTCWeapon();

			if (syncMessage == null)
			{
				ammoText.text = clientUI.WeaponManager.GetActiveWeapon().currentBulletAmount.ToString();
				maxAmmoText.text = weapon.maxBullets.ToString();
				reloadTextGameObject.SetActive(clientUI.WeaponManager.GetActiveWeapon().IsReloading);

				return;
			}

			ammoText.text = syncMessage.CurrentBullets.ToString();
			maxAmmoText.text = weapon.maxBullets.ToString();
			reloadTextGameObject.SetActive(syncMessage.IsReloading);
		}
	}
}