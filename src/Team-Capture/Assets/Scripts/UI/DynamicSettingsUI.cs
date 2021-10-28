// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Team_Capture.Helper.Extensions;
using Team_Capture.Settings;
using Team_Capture.UI.Elements.Settings;
using Team_Capture.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI
{
    #region Attributes

    /// <summary>
    ///     Tells the <see cref="DynamicSettingsUI" /> what the text should say next to the element, instead of just using
    ///     property
    ///     name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class SettingsPropertyDisplayTextAttribute : PreserveAttribute
    {
        internal readonly TableEntryReference tableEntryReference;

        internal readonly TableReference tableReference;

        public SettingsPropertyDisplayTextAttribute(string table, string tableEntry)
        {
            tableReference = table;
            tableEntryReference = tableEntry;
        }
    }

    /// <summary>
    ///     Tells the <see cref="DynamicSettingsUI" /> not to show this object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    internal class SettingsDontShowAttribute : PreserveAttribute
    {
    }

    #endregion

    /// <summary>
    ///     Generates a setting menu based on available options
    /// </summary>
    [RequireComponent(typeof(SettingsPanel))]
    internal class DynamicSettingsUI : MonoBehaviour
    {
        private SettingsPanel settingsPanel;

        private void Awake()
        {
            settingsPanel = GetComponent<SettingsPanel>();
        }

        /// <summary>
        ///     Generates the settings menu
        /// </summary>
        //TODO: The sub-functions need to update the UI element based on the reflected value on startup/settings reload
        public void UpdateUI()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            settingsPanel.ClearPanels();

            //TODO: Holy fucking hell this is ugly
            //Loop over each setting menu and all the sub-settings
            foreach (PropertyInfo settingInfo in GameSettings.GetSettingClasses())
            {
                //If it has the don't show attribute, then, well... don't show it
                if (settingInfo.DontShowObject())
                    continue;

                object settingGroupInstance = settingInfo.GetStaticValue<object>();

                //Get display text
                string settingGroupName = settingInfo.GetObjectDisplayText();

                //Create a menu module
                OptionsMenu optionOptionsMenu = new(settingGroupName);
                GameObject panel = settingsPanel.AddPanel(optionOptionsMenu);

                //Get each property in the settings
                var menuFields =
                    settingInfo.PropertyType.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                       BindingFlags.NonPublic);
                foreach (FieldInfo settingField in menuFields)
                {
                    //If it has the don't show attribute, then, well... don't show it
                    if (settingField.DontShowObject())
                        continue;

                    Type fieldType = settingField.FieldType;

                    if (fieldType == typeof(int))
                        CreateIntSlider(settingField.GetValue<int>(settingGroupInstance), settingField, panel);
                    else if (fieldType == typeof(float))
                        CreateFloatSlider(settingField.GetValue<float>(settingGroupInstance), settingField, panel);
                    else if (fieldType == typeof(bool))
                        CreateBoolToggle(settingField.GetValue<bool>(settingGroupInstance), settingField, panel);
                    else if (fieldType == typeof(string))
                        CreateStringField(settingField.GetValue<string>(settingGroupInstance), settingField,
                            optionOptionsMenu);
                    else if (fieldType == typeof(Resolution))
                        CreateResolutionDropdown(settingField.GetValue<Resolution>(settingGroupInstance), settingField,
                            panel);
                    else if (fieldType.IsEnum)
                        CreateEnumDropdown(settingField.GetValue<int>(settingGroupInstance), settingField, panel);
                    else
                        Logger.Error("UI Element for setting of type {FullName} is not supported!",
                            fieldType.FullName);
                }
            }

            stopwatch.Stop();
            Logger.Debug("Time taken to update settings UI: {TotalMilliseconds}ms",
                stopwatch.Elapsed.TotalMilliseconds);
        }

        #region Graphic designer functions

        // ReSharper disable ParameterHidesMember
        // ReSharper disable MemberCanBeMadeStatic.Local
        // ReSharper disable UnusedParameter.Local

        private void CreateIntSlider(int val, FieldInfo field, GameObject panel)
        {
            //If it's an int or a float, we need to check if it has a range attribute
            RangeAttribute rangeAttribute = field.GetFieldRange();
            if (rangeAttribute == null)
            {
                Logger.Error("{SettingField} doesn't have a Range attribute!", field.Name);
                return;
            }

            Slider slider = settingsPanel.AddSliderToPanel(panel, field.GetObjectDisplayText(), val, true,
                (int) rangeAttribute.min, (int) rangeAttribute.max);
            slider.onValueChanged.AddListener(f => field.SetValue(GetSettingObject(field), (int) f));
        }

        private void CreateFloatSlider(float val, FieldInfo field, GameObject panel)
        {
            //If it's an int or a float, we need to check if it has a range attribute
            RangeAttribute rangeAttribute = field.GetFieldRange();
            if (rangeAttribute == null)
            {
                Logger.Error("{SettingField} doesn't have a Range attribute!", field.Name);
                return;
            }

            Slider slider = settingsPanel.AddSliderToPanel(panel, field.GetObjectDisplayText(), val, false,
                rangeAttribute.min, rangeAttribute.max);
            slider.onValueChanged.AddListener(f => field.SetValue(GetSettingObject(field), f));
        }

        private void CreateBoolToggle(bool val, FieldInfo field, GameObject panel)
        {
            Toggle toggle = settingsPanel.AddToggleToPanel(panel, field.GetObjectDisplayText(), val);
            toggle.onValueChanged.AddListener(b => field.SetValue(GetSettingObject(field), b));
        }

        private void CreateStringField(string val, FieldInfo field, OptionsMenu optionsMenu)
        {
            Logger.Debug($"\tCreating string field for {field.Name} in {optionsMenu.Name}. Current is {val}");
            //            new TMP_InputField().onValueChanged.AddListener(s => field.SetValue(GetSettingObject(field), s));
        }

        private void CreateResolutionDropdown(Resolution currentRes, FieldInfo field, GameObject panel)
        {
            var resolutions = Screen.resolutions;
            var resolutionsText = new List<string>();
            int activeResIndex = 0;

            //Find the active current resolution, as well as add each resolution option to the list of resolutions text
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentRes.width && resolutions[i].width == currentRes.width)
                    activeResIndex = i;

                resolutionsText.Add(resolutions[i].ToString());
            }

            //Create the dropdown, with all of our resolutions
            TMP_Dropdown dropdown =
                settingsPanel.AddDropdownToPanel(panel, field.GetObjectDisplayText(), resolutionsText.ToArray(),
                    activeResIndex);
            dropdown.onValueChanged.AddListener(index =>
            {
                field.SetValue(GetSettingObject(field), resolutions[index]);
            });
        }

        private void CreateEnumDropdown(int val, FieldInfo field, GameObject panel)
        {
            string[] names = Enum.GetNames(field.FieldType);
            val = names.ToList().IndexOf(Enum.GetName(field.FieldType, val));

            TMP_Dropdown dropdown = settingsPanel.AddDropdownToPanel(panel, field.GetObjectDisplayText(), names, val);

            dropdown.onValueChanged.AddListener(index =>
            {
                // ReSharper disable once LocalVariableHidesMember
                string name = dropdown.options[index].text;
                int value = (int) Enum.Parse(field.FieldType, name);
                field.SetValue(GetSettingObject(field), value);
            });
        }

        private object GetSettingObject(FieldInfo field)
        {
            //Find the first setting group where the group type matches that of the field's declaring type
            PropertyInfo settingGroup =
                GameSettings.GetSettingClasses().First(p => p.PropertyType == field.DeclaringType);
            return settingGroup.GetValue(null);
        }

        // ReSharper restore UnusedParameter.Local
        // ReSharper restore MemberCanBeMadeStatic.Local
        // ReSharper restore ParameterHidesMember

        #endregion
    }

    /// <summary>
    ///     Helper functions for <see cref="DynamicSettingsUI" />
    /// </summary>
    internal static class DynamicSettingsUIHelper
    {
	    /// <summary>
	    ///     Don't show this object
	    /// </summary>
	    /// <param name="info"></param>
	    /// <returns></returns>
	    public static bool DontShowObject(this MemberInfo info)
        {
            return Attribute.GetCustomAttribute(info, typeof(SettingsDontShowAttribute)) != null;
        }

	    /// <summary>
	    ///     Gets the display text
	    /// </summary>
	    /// <param name="info"></param>
	    /// <returns></returns>
	    public static string GetObjectDisplayText(this MemberInfo info)
        {
            string text = info.Name;
            if (Attribute.GetCustomAttribute(info, typeof(SettingsPropertyDisplayTextAttribute)) is
                SettingsPropertyDisplayTextAttribute attribute)
                text = new LocalizedString(attribute.tableReference, attribute.tableEntryReference)
                    .GetLocalizedString();

            return text;
        }

        public static RangeAttribute GetFieldRange(this FieldInfo field)
        {
            RangeAttribute rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            return rangeAttribute;
        }
    }
}