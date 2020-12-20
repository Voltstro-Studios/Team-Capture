using Team_Capture.Weapons;
using TMPro;
using UnityEngine;

namespace Team_Capture.UI
{
	/// <summary>
	///     Hud, or 'Heads Up Display', it displays your health and ammo as well as other stuff
	/// </summary>
	internal class Hud : MonoBehaviour
	{
		/// <summary>
		///     The ammo text
		/// </summary>
		[Tooltip("The ammo text")] public TextMeshProUGUI ammoText;

		/// <summary>
		///     The max ammo text
		/// </summary>
		[Tooltip("The max ammo text")] public TextMeshProUGUI maxAmmoText;

		/// <summary>
		///     The reload text
		/// </summary>
		[Tooltip("The reload text")] public GameObject reloadTextGameObject;

		/// <summary>
		///     The health text
		/// </summary>
		[Tooltip("The health text")] public TextMeshProUGUI healthText;

		private ClientUI clientUI;

		/// <summary>
		///     Sets up the <see cref="Hud" />
		/// </summary>
		/// <param name="ui"></param>
		public void Setup(ClientUI ui)
		{
			clientUI = ui;
		}

		/// <summary>
		///     Updates the health text
		/// </summary>
		public void UpdateHealthUI()
		{
			healthText.text = clientUI.PlayerManager.Health.ToString();
		}

		/// <summary>
		///     Updates ammo text
		/// </summary>
		/// <param name="netWeapon"></param>
		public void UpdateAmmoUI(NetworkedWeapon netWeapon)
		{
			if (clientUI == null || clientUI.WeaponManager == null)
				return;

			TCWeapon weapon = netWeapon.GetTCWeapon();

			ammoText.text = netWeapon.CurrentBulletAmount.ToString();
			maxAmmoText.text = weapon.maxBullets.ToString();
			reloadTextGameObject.SetActive(netWeapon.IsReloading);
		}
	}
}