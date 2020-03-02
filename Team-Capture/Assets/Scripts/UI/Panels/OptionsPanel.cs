using Settings;
using UnityEngine;

namespace UI.Panels
{
	public class OptionsPanel : MainMenuPanelBase
	{
		[SerializeField] private GameObject keyboardMenu;
		[SerializeField] private GameObject videoMenu;
		[SerializeField] private GameObject advVideoMenu;
		[SerializeField] private GameObject otherMenu;

		private void Start()
		{
			OpenKeyboardMenu();
		}

		public void OpenKeyboardMenu()
		{
			CloseAll();
			keyboardMenu.SetActive(true);
		}

		public void OpenVideoMenu()
		{
			CloseAll();
			videoMenu.SetActive(true);
		}

		public void OpenAdvVideoMenu()
		{
			CloseAll();
			advVideoMenu.SetActive(true);
		}

		public void OpenOtherMenu()
		{
			CloseAll();
			otherMenu.SetActive(true);
		}

		public void SaveSettings()
		{
			GameSettings.Save();
		}

		private void CloseAll()
		{
			keyboardMenu.SetActive(false);
			videoMenu.SetActive(false);
			advVideoMenu.SetActive(false);
			otherMenu.SetActive(false);
		}
	}
}