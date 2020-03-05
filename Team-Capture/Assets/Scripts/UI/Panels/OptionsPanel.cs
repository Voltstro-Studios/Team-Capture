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
			GameObject titleObject = Instantiate(settingsTitlePrefab, panel.transform, false);
			titleObject.GetComponent<TextMeshProUGUI>().text = title;
			return titleObject;
		}

		public Toggle AddToggleToPanel(GameObject panel, string toggleText)
		{
			GameObject toggleObject = Instantiate(settingsTogglePrefab, panel.transform, false);
			toggleObject.GetComponentInChildren<TextMeshProUGUI>().text = toggleText;

			return toggleObject.GetComponent<Toggle>();
		}

		public Slider AddSliderToPanel(GameObject panel, string sideText, float currentValue, bool wholeNumbers = false, float min = 0, float max = 100)
		{
			GameObject sliderObject = Instantiate(settingsSliderPrefab, panel.transform, false);
			sliderObject.GetComponentInChildren<TextMeshProUGUI>().text = sideText;

			Slider slider = sliderObject.GetComponentInChildren<Slider>();
			slider.wholeNumbers = wholeNumbers;
			slider.minValue = min;
			slider.maxValue = max;
			slider.value = currentValue;

			return slider;
		}
		
		#endregion
	}
}