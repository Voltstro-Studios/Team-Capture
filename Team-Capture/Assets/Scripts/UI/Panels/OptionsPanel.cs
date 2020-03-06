using System.Collections.Generic;
using System.Linq;
using Settings;
using TMPro;
using UI.Elements.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	public class OptionsPanel : MainMenuPanelBase
	{
		private readonly List<GameObject> settingPanels = new List<GameObject>();

		[Header("Object Locations")]
		[SerializeField] private Transform panelsLocation;
		[SerializeField] private Transform buttonLocation;

		[Header("Object Prefabs")]
		[SerializeField] private GameObject panelPrefab;
		[SerializeField] private GameObject settingsTitlePrefab;
		[SerializeField] private GameObject settingsButtonPrefab;
		[SerializeField] private GameObject settingsTogglePrefab;
		[SerializeField] private GameObject settingsSliderPrefab;
		[SerializeField] private GameObject settingsDropdownPrefab;

		public void SaveSettings()
		{
			GameSettings.Save();
		}

		#region Panel Management

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

		public void OpenPanel(string panelName)
		{
			foreach (GameObject panel in settingPanels)
			{
				panel.SetActive(false);
			}

			GetMenuPanel(panelName).SetActive(true);
		}

		public void ClearPanels()
		{
			foreach (GameObject panel in settingPanels)
			{
				Destroy(panel);
			}

			settingPanels.Clear();
		}

		private GameObject GetMenuPanel(string panelName)
		{
			IEnumerable<GameObject> result = from a in settingPanels
				where a.name == panelName
				select a;

			return result.FirstOrDefault();
		}
		
		#endregion

		#region Add UI Elemements

		private GameObject AddTitleToPanel(GameObject panel, string title)
		{
			GameObject titleObject = Instantiate(settingsTitlePrefab, GetPanelItemArea(panel), false);
			titleObject.GetComponent<TextMeshProUGUI>().text = title;
			return titleObject;
		}

		public Toggle AddToggleToPanel(GameObject panel, string toggleText, bool currentValue)
		{
			GameObject toggleObject = Instantiate(settingsTogglePrefab, GetPanelItemArea(panel), false);
			toggleObject.GetComponentInChildren<TextMeshProUGUI>().text = toggleText;

			Toggle toggle = toggleObject.GetComponent<Toggle>();
			toggle.isOn = currentValue;

			return toggle;
		}

		public Slider AddSliderToPanel(GameObject panel, string sideText, float currentValue, bool wholeNumbers = false, float min = 0, float max = 100)
		{
			GameObject sliderObject = Instantiate(settingsSliderPrefab, GetPanelItemArea(panel), false);
			SettingsSlider sliderSettings = sliderObject.GetComponent<SettingsSlider>();
			sliderSettings.Setup(currentValue);

			sliderSettings.propertyText.text = sideText;

			Slider slider = sliderSettings.slider;
			slider.wholeNumbers = wholeNumbers;
			slider.minValue = min;
			slider.maxValue = max;
			slider.value = currentValue;

			return slider;
		}

		public TMP_Dropdown AddDropdownToPanel(GameObject panel, string[] options, int currentIndex)
		{
			// ReSharper disable once LocalVariableHidesMember
			GameObject gameObject = Instantiate(settingsDropdownPrefab, GetPanelItemArea(panel), false);
			TMP_Dropdown dropdown = gameObject.GetComponentInChildren<TMP_Dropdown>();
			List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>(options.Length);
			for (int i = 0; i < options.Length; i++)
			{
				optionDatas.Add(new TMP_Dropdown.OptionData(options[i]));
			}
			dropdown.options = optionDatas;
			dropdown.value = currentIndex;
			return dropdown;
		}
		
		#endregion

		private Transform GetPanelItemArea(GameObject panel)
		{
			return panel.GetComponent<SettingsMenuPanel>().GetScrollingArea;
		}
	}
}