using UnityEngine;
using Weapons;

namespace Player
{
	public class PlayerInput : MonoBehaviour
	{
		private WeaponManager weaponManager;

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
		}

		private void Update()
		{
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

			if(selectedWeaponIndex != weaponManager.selectedWeaponIndex)
				weaponManager.CmdSetWeaponIndex(selectedWeaponIndex);
		}
	}
}
