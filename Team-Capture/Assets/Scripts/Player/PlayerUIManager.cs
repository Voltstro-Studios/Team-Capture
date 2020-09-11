using UI;
using UnityEngine;
using Weapons;

namespace Player
{
	public class PlayerUIManager : MonoBehaviour
	{
		private Hud hud;

		public void Start()
		{
			PlayerManager playerManager = GetComponent<PlayerManager>();
			hud = playerManager.ClientUi.hud;
			playerManager.PlayerDamaged += OnPlayerDamaged;
			GetComponent<WeaponManager>().WeaponUpdated += OnWeaponUpdated;
		}

		private void OnPlayerDamaged()
		{
			hud.UpdateHealthUI();
		}

		private void OnWeaponUpdated(NetworkedWeapon weapon)
		{
			hud.UpdateAmmoUI(weapon);
		}
	}
}