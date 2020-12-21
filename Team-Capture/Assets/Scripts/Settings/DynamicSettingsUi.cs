using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Team_Capture.Helper.Extensions;
using Team_Capture.UI.Elements.Settings;
using Team_Capture.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Logger = Team_Capture.Core.Logging.Logger;

namespace Team_Capture.Settings
{
	#region Attributes

	/// <summary>
	///     Tells the <see cref="DynamicSettingsUi" /> what the text should say next to the element, instead of just using
	///     property
	///     name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	internal class SettingsPropertyFormatNameAttribute : PreserveAttribute
	{
		public SettingsPropertyFormatNameAttribute(string menuFormat)
		{
			MenuNameFormat = menuFormat;
		}

		public string MenuNameFormat { get; }
	}

	/// <summary>
	///     Tells the <see cref="DynamicSettingsUi" /> not to show this object
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	internal class SettingsDontShowAttribute : PreserveAttribute
	{
	}

	#endregion

	/// <summary>
	///     Generates a setting menu based on available options
	/// </summary>
	[RequireComponent(typeof(OptionsPanel))]
	internal class DynamicSettingsUi : MonoBehaviour
	{
		private OptionsPanel optionsPanel;

		private void Awake()
		{
			optionsPanel = GetComponent<OptionsPanel>();
		}

		/// <summary>
		///     Generates the settings menu
		/// </summary>
		//TODO: The sub-functions need to update the UI element based on the reflected value on startup/settings reload
		public void UpdateUI()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			optionsPanel.ClearPanels();

			//TODO: Holy fucking hell this is ugly
			//Loop over each setting menu and all the sub-settings
			foreach (PropertyInfo settingInfo in GameSettings.GetSettingClasses())
			{
				//If it has the don't show attribute, then, well... don't show it
				if (Attribute.GetCustomAttribute(settingInfo, typeof(SettingsDontShowAttribute)) != null)
					continue;

				object settingGroupInstance = settingInfo.GetStaticValue<object>();

				//If it has the SettingsPropertyFormatNameAttribute then use the format name of that
				string settingGroupName = settingInfo.Name;
				if (Attribute.GetCustomAttribute(settingInfo, typeof(SettingsPropertyFormatNameAttribute)) is
					SettingsPropertyFormatNameAttribute attribute)
					settingGroupName = attribute.MenuNameFormat;

				//Create a menu module
				OptionsMenu optionOptionsMenu = new OptionsMenu(settingGroupName);
				GameObject panel = optionsPanel.AddPanel(optionOptionsMenu);

				//Get each property in the settings
				FieldInfo[] menuFields =
					settingInfo.PropertyType.GetFields(BindingFlags.Instance | BindingFlags.Public |
					                                   BindingFlags.NonPublic);
				foreach (FieldInfo settingField in menuFields)
				{
					//If it has the don't show attribute, then, well... don't show it
					if (Attribute.GetCustomAttribute(settingField, typeof(SettingsDontShowAttribute)) != null)
						continue;

					Type fieldType = settingField.FieldType;

					//TODO: Considering the unity slider uses a float, could we have 1 overload with a bool for int or float mode?
					if (fieldType == typeof(int))
					{
						//If it's an int or a float, we need to check if it has a range attribute
						RangeAttribute rangeAttribute = settingField.GetCustomAttribute<RangeAttribute>();

						//If it has a range attribute, create a slider, otherwise use a input field
						if (rangeAttribute != null)
							CreateIntSlider(settingField.GetValue<int>(settingGroupInstance), (int) rangeAttribute.min,
								(int) rangeAttribute.max,
								settingField, panel);
						else
							CreateIntField(settingField.GetValue<int>(settingGroupInstance), settingField,
								optionOptionsMenu);
					}
					else if (fieldType == typeof(float))
					{
						//If it's an int or a float, we need to check if it has a range attribute
						RangeAttribute rangeAttribute = settingField.GetCustomAttribute<RangeAttribute>();

						//If it has a range attribute, create a slider, otherwise use a input field
						if (rangeAttribute != null)
							CreateFloatSlider(settingField.GetValue<float>(settingGroupInstance), rangeAttribute.min,
								rangeAttribute.max, settingField, panel);
						else
							CreateFloatField(settingField.GetValue<float>(settingGroupInstance), settingField,
								optionOptionsMenu);
					}
					else if (fieldType == typeof(bool))
					{
						CreateBoolToggle(settingField.GetValue<bool>(settingGroupInstance), settingField, panel);
					}
					else if (fieldType == typeof(string))
					{
						CreateStringField(settingField.GetValue<string>(settingGroupInstance), settingField,
							optionOptionsMenu);
					}
					else if (fieldType == typeof(Resolution))
					{
						//For a resolution property, we will create a dropdown with all available resolutions and select the active one
						CreateResolutionDropdown(settingField.GetValue<Resolution>(settingGroupInstance), settingField,
							panel);
					}
					//TODO: Finish these
					else if (fieldType.IsEnum)
					{
						if (fieldType == typeof(KeyCode))
							//We don't do a dropdown, we create a button and do some complicated shit that i'll steal from one of my other games
							CreateKeybindButton(settingField.GetValue<KeyCode>(settingGroupInstance),
								settingField,
								optionOptionsMenu);
						//Just a normal enum popup
						else
							CreateEnumDropdown(settingField.GetValue<int>(settingGroupInstance), settingField, panel);
					}
					else
					{
						Logger.Error("UI Element for setting of type {@FullName} could not be created",
							fieldType.FullName);
					}
				}
			}

			stopwatch.Stop();
			Logger.Debug("Time taken to update settings UI: {@TotalMilliseconds}ms",
				stopwatch.Elapsed.TotalMilliseconds);
		}

		#region Graphic designer functions

		// ReSharper disable ParameterHidesMember
		// ReSharper disable MemberCanBeMadeStatic.Local
		// ReSharper disable UnusedParameter.Local

		private void CreateFloatSlider(float val, float min, float max, FieldInfo field, GameObject panel)
		{
			Slider slider = optionsPanel.AddSliderToPanel(panel, GetFieldFormatName(field), val, false, min, max);
			slider.onValueChanged.AddListener(f => field.SetValue(GetSettingObject(field), f));
		}

		private void CreateIntSlider(int val, int min, int max, FieldInfo field, GameObject panel)
		{
			Slider slider = optionsPanel.AddSliderToPanel(panel, GetFieldFormatName(field), val, true, min, max);
			slider.onValueChanged.AddListener(f => field.SetValue(GetSettingObject(field), (int) f));
		}

		private void CreateFloatField(float val, FieldInfo field, OptionsMenu optionsMenu)
		{
			Logger.Debug($"\tCreating float field for {field.Name} in {optionsMenu.Name}. Current is {val}");
			// new FloatField().RegisterValueChangedCallback(c => field.SetValue(GetSettingObject(field), c.newValue));
		}

		private void CreateIntField(int val, FieldInfo field, OptionsMenu optionsMenu)
		{
			Logger.Debug($"\tCreating int field for {field.Name} in {optionsMenu.Name}. Current is {val}");
			// new IntegerField().RegisterValueChangedCallback(c => field.SetValue(GetSettingObject(field), c.newValue));
		}

		private void CreateBoolToggle(bool val, FieldInfo field, GameObject panel)
		{
			Toggle toggle = optionsPanel.AddToggleToPanel(panel, GetFieldFormatName(field), val);
			toggle.onValueChanged.AddListener(b => field.SetValue(GetSettingObject(field), b));
		}

		private void CreateStringField(string val, FieldInfo field, OptionsMenu optionsMenu)
		{
			Logger.Debug($"\tCreating string field for {field.Name} in {optionsMenu.Name}. Current is {val}");
			//            new TMP_InputField().onValueChanged.AddListener(s => field.SetValue(GetSettingObject(field), s));
		}

		private void CreateResolutionDropdown(Resolution currentRes, FieldInfo field, GameObject panel)
		{
			Resolution[] resolutions = Screen.resolutions;
			List<string> resolutionsText = new List<string>();
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
				optionsPanel.AddDropdownToPanel(panel, GetFieldFormatName(field), resolutionsText.ToArray(),
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

			TMP_Dropdown dropdown = optionsPanel.AddDropdownToPanel(panel, GetFieldFormatName(field), names, val);

			dropdown.onValueChanged.AddListener(index =>
			{
				// ReSharper disable once LocalVariableHidesMember
				string name = dropdown.options[index].text;
				int value = (int) Enum.Parse(field.FieldType, name);
				field.SetValue(GetSettingObject(field), value);
			});
		}

		private void CreateKeybindButton(KeyCode val, FieldInfo field, OptionsMenu optionsMenu)
		{
			Logger.Debug($"\tCreating keybind button for {field.Name} in {optionsMenu.Name}. Current is {val}");
			//Sorry future Creepysin in case this freezes the game...
			//TODO: Need to create this function. Might need to start a coroutine/async void to avoid freezing the screen till a key is pressed
			//            new Button().onClick.AddListener(  () => field.SetValue(GetSettingObject(field), WaitForKeyPressAndReturnKeycode()));
		}

		private object GetSettingObject(FieldInfo field)
		{
			//Find the first setting group where the group type matches that of the field's declaring type
			PropertyInfo settingGroup =
				GameSettings.GetSettingClasses().First(p => p.PropertyType == field.DeclaringType);
			return settingGroup.GetValue(null);
		}

		private string GetFieldFormatName(MemberInfo field)
		{
			string sideName = field.Name;
			if (Attribute.GetCustomAttribute(field, typeof(SettingsPropertyFormatNameAttribute)) is
				SettingsPropertyFormatNameAttribute attribute)
				sideName = attribute.MenuNameFormat;

			return sideName;
		}

		// ReSharper restore UnusedParameter.Local
		// ReSharper restore MemberCanBeMadeStatic.Local
		// ReSharper restore ParameterHidesMember

		#endregion
	}
}