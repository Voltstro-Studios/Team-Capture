using System.Collections.Generic;
using System.Linq;
using Team_Capture.Localization;
using Team_Capture.Settings;
using Team_Capture.UI.Elements.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI.Panels
{
	/// <summary>
	///     The panel for options
	/// </summary>
	internal class OptionsPanel : MainMenuPanelBase
	{
		[Header("Object Locations")] [SerializeField]
		private GameObject appliedOptionsPanel;

		[SerializeField] private Transform panelsLocation;
		[SerializeField] private Transform buttonLocation;

		[Header("Object Prefabs")] [SerializeField]
		private GameObject panelPrefab;

		[SerializeField] private GameObject settingsTitlePrefab;
		[SerializeField] private GameObject settingsButtonPrefab;
		[SerializeField] private GameObject settingsTogglePrefab;
		[SerializeField] private GameObject settingsSliderPrefab;
		[SerializeField] private GameObject settingsDropdownPrefab;
		private readonly Dictionary<Button, GameObject> settingPanels = new Dictionary<Button, GameObject>();

		private void Start()
		{
			//Tell our dynamic settings UI to generate all the options
			GetComponent<DynamicSettingsUi>().UpdateUI();

			//Close all panels
			ClosePanels();

			//Set the first settings panel to open
			if (settingPanels.Count != 0)
			{
				settingPanels.Values.ToArray()[0].SetActive(true);
				settingPanels.Keys.ToArray()[0].interactable = false;
			}

			//Close the applied settings panel
			CloseAppliedOptionsPanel();
		}

		/// <summary>
		///     Saves the settings
		/// </summary>
		public void SaveSettings()
		{
			GameSettings.Save();

			appliedOptionsPanel.SetActive(true);
		}

		/// <summary>
		///     Closes the applied settings panel
		/// </summary>
		public void CloseAppliedOptionsPanel()
		{
			appliedOptionsPanel.SetActive(false);
		}

		private static Transform GetPanelItemArea(GameObject panel)
		{
			return panel.GetComponent<SettingsMenuPanel>().scrollingArea;
		}

		#region Panel Management

		/// <summary>
		///     Adds a new panel
		/// </summary>
		/// <param name="optionsMenu"></param>
		/// <returns></returns>
		public GameObject AddPanel(OptionsMenu optionsMenu)
		{
			//The panel it self
			GameObject panel = Instantiate(panelPrefab, panelsLocation, false);
			panel.name = optionsMenu.Name;
			AddTitleToPanel(panel, optionsMenu.Name);

			//Button
			Button button = Instantiate(settingsButtonPrefab, buttonLocation, false).GetComponent<Button>();
			button.onClick.AddListener(delegate { OpenPanel(optionsMenu.Name); });
			TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
			buttonText.text = GameUILocale.ResolveString(optionsMenu.Name);
			buttonText.text = buttonText.text.Replace(" ", "\n");

			settingPanels.Add(button, panel);

			return panel;
		}

		/// <summary>
		///     Opens a panel
		/// </summary>
		/// <param name="panelName"></param>
		public void OpenPanel(string panelName)
		{
			//Close all other panels
			foreach (KeyValuePair<Button, GameObject> panel in settingPanels)
			{
				panel.Value.SetActive(false);
				panel.Key.interactable = true;
			}

			KeyValuePair<Button, GameObject> menuPanel = GetMenuPanel(panelName);
			menuPanel.Value.SetActive(true);
			menuPanel.Key.interactable = false;
		}

		/// <summary>
		///     Removes all panels
		/// </summary>
		public void ClearPanels()
		{
			//Remove all panels
			foreach (GameObject panel in settingPanels.Values)
				Destroy(panel);

			settingPanels.Clear();
		}

		/// <summary>
		///     Closes all panels
		/// </summary>
		private void ClosePanels()
		{
			//Close all panels
			foreach (GameObject panel in settingPanels.Values)
				panel.SetActive(false);
		}

		/// <summary>
		///     Gets a <see cref="GameObject" /> for a menu
		/// </summary>
		/// <param name="panelName"></param>
		/// <returns></returns>
		private KeyValuePair<Button, GameObject> GetMenuPanel(string panelName)
		{
			IEnumerable<KeyValuePair<Button, GameObject>> result = from a in settingPanels
				where a.Value.name == panelName
				select a;

			return result.FirstOrDefault();
		}

		#endregion

		#region Add UI Elemements

		private void AddTitleToPanel(GameObject panel, string title)
		{
			//Create new title object
			GameObject titleObject = Instantiate(settingsTitlePrefab, GetPanelItemArea(panel), false);
			titleObject.GetComponent<TextMeshProUGUI>().text = GameUILocale.ResolveString(title);
		}

		public Toggle AddToggleToPanel(GameObject panel, string toggleText, bool currentValue)
		{
			//Create new toggle object
			GameObject toggleObject = Instantiate(settingsTogglePrefab, GetPanelItemArea(panel), false);
			TextMeshProUGUI label = toggleObject.GetComponentInChildren<TextMeshProUGUI>();
			label.text = GameUILocale.ResolveString(toggleText);

			//Set if it is on or off
			Toggle toggle = toggleObject.GetComponent<Toggle>();
			toggle.isOn = currentValue;

			return toggle;
		}

		public Slider AddSliderToPanel(GameObject panel, string sideText, float currentValue, bool wholeNumbers = false,
			float min = 0, float max = 100)
		{
			//Create new slider object
			GameObject sliderObject = Instantiate(settingsSliderPrefab, GetPanelItemArea(panel), false);
			SettingsSlider sliderSettings = sliderObject.GetComponent<SettingsSlider>();

			//Set up the existing value
			sliderSettings.Setup(currentValue);

			//Set the text
			sliderSettings.propertyText.text = GameUILocale.ResolveString(sideText);

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
			dropdownSettings.settingsName.text = GameUILocale.ResolveString(sideText);

			//Create options
			List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>(options.Length);
			optionDatas.AddRange(options.Select(text => new TMP_Dropdown.OptionData(text)));

			//Add options and set current index
			dropdownSettings.dropdown.options = optionDatas;
			dropdownSettings.dropdown.value = currentIndex;
			return dropdownSettings.dropdown;
		}

		#endregion
	}
}