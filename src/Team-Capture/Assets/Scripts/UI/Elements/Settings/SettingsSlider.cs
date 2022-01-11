// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI.Elements.Settings
{
    /// <summary>
    ///     Slider for the settings menu
    /// </summary>
    internal class SettingsSlider : MonoBehaviour
    {
        /// <summary>
        ///     The property text
        /// </summary>
        public TextMeshProUGUI propertyText;

        /// <summary>
        ///     Text for the value
        /// </summary>
        public TextMeshProUGUI valueText;

        /// <summary>
        ///     The actual slider
        /// </summary>
        public Slider slider;

        private bool wholeNumbers;

        /// <summary>
        ///     Sets up the slider
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="wholeNums"></param>
        public void Setup(float initialValue, bool wholeNums)
        {
            wholeNumbers = wholeNums;
            slider.onValueChanged.AddListener(SetTextValue);
            SetTextValue(initialValue);
        }

        private void SetTextValue(float value)
        {
            if(wholeNumbers)
                valueText.text = Convert.ToDecimal($"{value}").ToString(CultureInfo.InvariantCulture);
            else
                valueText.text = Convert.ToDecimal($"{value:F2}").ToString(CultureInfo.InvariantCulture);
        }
    }
}