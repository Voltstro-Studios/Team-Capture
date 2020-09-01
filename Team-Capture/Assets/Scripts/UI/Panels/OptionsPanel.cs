using System.Collections.Generic;
using System.Linq;
using Settings;
using TMPro;
using UI.Elements.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	/// <summary>
	/// The panel for options
	/// </summary>
	public class OptionsPanel : MainMenuPanelBase
	{
		private readonly List<GameObject> settingPanels = new List<GameObject>();

		[Header("Object Locations")] 
		[SerializeField] private GameObject appliedSettingsPanel;
		[SerializeField] private Transform panelsLocation;
		[SerializeField] private Transform buttonLocation;

		[Header("Object Prefabs")]
		[SerializeField] private GameObject panelPrefab;
		[SerializeField] private GameObject settingsTitlePrefab;
		[SerializeField] private GameObject settingsButtonPrefab;
		[SerializeField] private GameObject settingsTogglePrefab;
		[SerializeField] private GameObject settingsSliderPrefab;
		[SerializeField] private GameObject settingsDropdownPrefab;

		private void Start()
		{
			//Tell our dynamic settings UI to generate all the options
			GetComponent<DynamicSettingsUi>().UpdateUI();

			//Close all panels
			ClosePanels();

			//Set the first settings panel to open
			if(settingPanels.Count != 0)
				settingPanels[0].SetActive(true);

			//Close the applied settings panel
			CloseAppliedSettingsPanel();
		}

		/// <summary>
		/// Saves the settings
		/// </summary>
		public void SaveSettings()
		{
			GameSettings.Save();

			appliedSettingsPanel.SetActive(true);
		}

		/// <summary>
		/// Closes the applied settings panel
		/// </summary>
		public void CloseAppliedSettingsPanel()
		{
			appliedSettingsPanel.SetActive(false);
		}

		#region Panel Management

		/// <summary>
		/// Adds a new panel
		/// </summary>
		/// <param name="menu"></param>
		/// <returns></returns>
		public GameObject AddPanel(Menu menu)
		{
			//The panel it self
			GameObject panel = Instantiate(panelPrefab, panelsLocation, false);
			panel.name = menu.Name;
			AddTitleToPanel(panel, menu.Name);

			//Button
			Button button = Instantiate(settingsButtonPrefab, buttonLocation, false).GetComponent<Button>();
			button.onClick.AddListener((delegate { OpenPanel(menu.Name); }));
			button.GetComponentInChildren<TextMeshProUGUI>().text = menu.Name;

			settingPanels.Add(panel);

			return panel;
		}

		/// <summary>
		/// Opens a panel
		/// </summary>
		/// <param name="panelName"></param>
		public void OpenPanel(string panelName)
		{
			//Close all other panels
			foreach (GameObject panel in settingPanels)
				panel.SetActive(false);

			GetMenuPanel(panelName).SetActive(true);
		}

		/// <summary>
		/// Removes all panels
		/// </summary>
		public void ClearPanels()
		{
			//Remove all panels
			foreach (GameObject panel in settingPanels)
				Destroy(panel);

			settingPanels.Clear();
		}

		/// <summary>
		/// Closes all panels
		/// </summary>
		private void ClosePanels()
		{
			//Close all panels
			foreach (GameObject panel in settingPanels)
				panel.SetActive(false);
		}

		/// <summary>
		/// Gets a <see cref="GameObject"/> for a menu
		/// </summary>
		/// <param name="panelName"></param>
		/// <returns></returns>
		private GameObject GetMenuPanel(string panelName)
		{
			IEnumerable<GameObject> result = from a in settingPanels
				where a.name == panelName
				select a;

			return result.FirstOrDefault();
		}
		
		#endregion

		#region Add UI Elemements

		private void AddTitleToPanel(GameObject panel, string title)
		{
			//Create new title object
			GameObject titleObject = Instantiate(settingsTitlePrefab, GetPanelItemArea(panel), false);
			titleObject.GetComponent<TextMeshProUGUI>().text = title;
		}

		public Toggle AddToggleToPanel(GameObject panel, string toggleText, bool currentValue)
		{
			//Create new toggle object
			GameObject toggleObject = Instantiate(settingsTogglePrefab, GetPanelItemArea(panel), false);
			toggleObject.GetComponentInChildren<TextMeshProUGUI>().text = toggleText;

			//Set if it is on or off
			Toggle toggle = toggleObject.GetComponent<Toggle>();
			toggle.isOn = currentValue;

			return toggle;
		}

		public Slider AddSliderToPanel(GameObject panel, string sideText, float currentValue, bool wholeNumbers = false, float min = 0, float max = 100)
		{
			//Create new slider object
			GameObject sliderObject = Instantiate(settingsSliderPrefab, GetPanelItemArea(panel), false);
			SettingsSlider sliderSettings = sliderObject.GetComponent<SettingsSlider>();

			//Set up the existing value
			sliderSettings.Setup(currentValue);

			//Set the text
			sliderSettings.propertyText.text = sideText;

			//Set up the slider itself
			Slider slider = sliderSettings.slider;
			slider.wholeNumbers = wholeNumbers;
			slider.minValue = min;
			slider.maxValue = max;
			slider.value = currentValue;

			return slider;
		}

		public TMP_Dropdown AddDropdownToPanel(GameObject panel, string sideText, string[] options, int currentIndex)
		{
			//Create dropdown object
			GameObject dropDownObject = Instantiate(settingsDropdownPrefab, GetPanelItemArea(panel), false);
			SettingDropdown dropdownSettings = dropDownObject.GetComponent<SettingDropdown>();
			dropdownSettings.settingsName.text = sideText;

			//Create options
			List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>(options.Length);
			optionDatas.AddRange(options.Select(text => new TMP_Dropdown.OptionData(text)));

			//Add options and set current index
			dropdownSettings.dropdown.options = optionDatas;
			dropdownSettings.dropdown.value = currentIndex;
			return dropdownSettings.dropdown;
		}
		
		#endregion

		private static Transform GetPanelItemArea(GameObject panel)
		{
			return panel.GetComponent<SettingsMenuPanel>().GetScrollingArea;
		}
	}
}