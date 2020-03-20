using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements.Settings
{
	public class SettingsSlider : MonoBehaviour
	{
		public TextMeshProUGUI propertyText;
		public TextMeshProUGUI valueText;

		public Slider slider;

		public void Setup(float initialValue)
		{
			slider.onValueChanged.AddListener(SetSliderValueText);
			valueText.text = initialValue.ToString();
		}

		private void SetSliderValueText(float value)
		{
			int intValue = (int)Math.Round(value);
			valueText.text = intValue.ToString();
		}
	}
}
