using UI;
using UnityEngine;
using Weapons;

namespace Player
{
	public class PlayerUIManager : MonoBehaviour
	{
		private Hud hud;

		public void Setup(ClientUI clientUI)
		{
			hud = clientUI.hud;
			GetComponent<PlayerManager>().PlayerDamaged += OnPlayerDamaged;
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