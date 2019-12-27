using Mirror;
using UnityEngine;
using Weapons;

namespace Player
{
	public class PlayerInput : NetworkBehaviour
	{
		[SerializeField] private KeyCode suicideKey = KeyCode.P;

		private WeaponManager weaponManager;
		private PlayerManager playerManager;

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
		}

		private void Update()
		{
			if(!isLocalPlayer) return;

			if (!playerManager.IsDead && Input.GetKeyDown(suicideKey))
				playerManager.CmdSuicide();

			int selectedWeaponIndex = weaponManager.selectedWeaponIndex;
			int weaponHolderChildCount = weaponManager.weaponsHolderSpot.childCount - 1;

			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				if (selectedWeaponIndex >= weaponHolderChildCount)
					selectedWeaponIndex = 0;
				else
					selectedWeaponIndex++;
			}

			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				if (selectedWeaponIndex <= 0)
					selectedWeaponIndex = weaponHolderChildCount;
				else
					selectedWeaponIndex--;
			}

			if (selectedWeaponIndex != weaponManager.selectedWeaponIndex)
				weaponManager.CmdSetWeaponIndex(transform.name, selectedWeaponIndex);
		}

		
	}
}