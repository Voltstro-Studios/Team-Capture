using TMPro;
using UnityEngine;

namespace Team_Capture.UI.Elements.Settings
{
	/// <summary>
	///     A dropdown for the settings menu
	/// </summary>
	internal class SettingDropdown : MonoBehaviour
	{
		/// <summary>
		///     The dropdown itself
		/// </summary>
		public TMP_Dropdown dropdown;

		/// <summary>
		///     The settings name text
		/// </summary>
		public TextMeshProUGUI settingsName;
	}
}