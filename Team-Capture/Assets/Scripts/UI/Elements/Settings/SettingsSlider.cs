using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements.Settings
{
	/// <summary>
	/// Slider for the settings menu
	/// </summary>
	internal class SettingsSlider : MonoBehaviour
	{
		/// <summary>
		/// The property text
		/// </summary>
		public TextMeshProUGUI propertyText;

		/// <summary>
		/// Text for the value
		/// </summary>
		public TextMeshProUGUI valueText;

		/// <summary>
		/// The actual slider
		/// </summary>
		public Slider slider;

		/// <summary>
		/// Sets up the slider
		/// </summary>
		/// <param name="initialValue"></param>
		public void Setup(float initialValue)
		{
			slider.onValueChanged.AddListener(SetSliderValueText);
			valueText.text = initialValue.ToString();
		}

		private void SetSliderValueText(float value)
		{
			valueText.text = Convert.ToDecimal($"{value:F2}").ToString();
		}
	}
}
