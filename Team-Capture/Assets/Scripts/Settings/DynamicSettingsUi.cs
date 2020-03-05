using Attributes;
using Core.Logger;
using Helper.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TMPro;
using UI.Elements.Settings;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Logger.Logger;

namespace Settings
{
	//TODO: A lot is gonna change in this... I am gonna have fun rewriting this, thanks Rowan...
	[RequireComponent(typeof(OptionsPanel))]
	public class DynamicSettingsUi : MonoBehaviour
	{
		private OptionsPanel optionsPanel;

		private void Start()
		{
			optionsPanel = GetComponent<OptionsPanel>();
			UpdateUI();
		}

		//TODO: The sub-functions need to update the UI element based on the reflected value on startup/settings reload
		private void UpdateUI()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			optionsPanel.ClearPanels();

			//TODO: Holy fucking hell this is ugly
			//Loop over each setting menu and all the sub-settings
			foreach (PropertyInfo settingInfo in GameSettings.GetSettingClasses())
			{
				object settingGroupInstance = settingInfo.GetStaticValue<object>();

				//If it has the SettingsMenuFormatAttribute then use the format name of that
				string settingGroupName = settingInfo.Name;
				if (Attribute.GetCustomAttribute(settingInfo, typeof(SettingsMenuFormatAttribute)) is SettingsMenuFormatAttribute attribute)
					settingGroupName = attribute.MenuNameFormat;

				//Create a menu module
				Menu settingMenu = new Menu(settingGroupName);
				GameObject panel = optionsPanel.AddPanel(settingMenu);

				//Get each property in the settings
				FieldInfo[] menuFields = settingInfo.PropertyType.GetFields(BindingFlags.Instance | BindingFlags.Public);
				foreach (FieldInfo settingField in menuFields)
				{
					Type fieldType = settingField.FieldType;

					//TODO: Considering the unity slider uses a float, could we have 1 overload with a bool for int or float mode?
					if (fieldType == typeof(int))
					{
						//If it's an int or float, we need to check if it has a range attribute
						RangeAttribute rangeAttribute = settingField.GetCustomAttribute<RangeAttribute>();

						//If it has a range attribute, create a slider, otherwise use a input field
						if (rangeAttribute != null)
							CreateIntSlider(settingField.GetValue<int>(settingGroupInstance), (int) rangeAttribute.min, (int) rangeAttribute.max,
								settingField, settingMenu);
						else
							CreateIntField(settingField.GetValue<int>(settingGroupInstance), settingField, settingMenu);
					}
					else if (fieldType == typeof(float))
					{
						RangeAttribute rangeAttribute = settingField.GetCustomAttribute<RangeAttribute>();
						if (rangeAttribute != null)
							CreateFloatSlider(settingField.GetValue<float>(settingGroupInstance), rangeAttribute.min,
								rangeAttribute.max,
								settingField, settingMenu);
						else
							CreateFloatField(settingField.GetValue<float>(settingGroupInstance), settingField,
								settingMenu);
					}
					else if (fieldType == typeof(bool))
					{
						CreateBoolToggle(settingField.GetValue<bool>(settingGroupInstance), settingField, settingMenu, panel);
					}
					else if (fieldType == typeof(string))
					{
						CreateStringField(settingField.GetValue<string>(settingGroupInstance), settingField, settingMenu);
					}
					//TODO: Finish these
					else if (fieldType.IsEnum)
					{
						if (fieldType == typeof(KeyCode))
							//We don't do a dropdown, we create a button and do some complicated shit that i'll steal from one of my other games
							CreateKeybindButton(settingField.GetValue<KeyCode>(settingGroupInstance),
								settingField,
								settingMenu);
						//Just a normal enum popup
						else
							CreateEnumDropdown(settingField.GetValue<int>(settingGroupInstance), settingField, settingMenu, panel);
					}
					else
					{
						Logger.Log($"UI Element for setting of type {fieldType.FullName} could not be created", LogVerbosity.Error);
					}
				}
			}

			stopwatch.Stop();
			Logger.Log($"Time taken to update UI: {stopwatch.Elapsed.TotalMilliseconds:n} ms", LogVerbosity.Debug);
		}

	#region Graphic designer functions

		// ReSharper disable ParameterHidesMember
		// ReSharper disable MemberCanBeMadeStatic.Local
		// ReSharper disable UnusedParameter.Local

		//Use onValueChanged.AddListener so that every time one of the graphics gets updated, so does our setting
		private void CreateFloatSlider(float val, float min, float max, FieldInfo field, Menu menu, GameObject panel)
		{
			Logger.Log(
				$"\tCreating float slider for {field.Name} in {menu.Name}. Range is {min} to {max}, current is {val}", LogVerbosity.Debug);
			// new Slider().onValueChanged.AddListener(f => field.SetValue(GetSettingObject(field), f));
		}

		private void CreateIntSlider(int val, int min, int max, FieldInfo field, Menu menu)
		{
			Logger.Log(
				$"\tCreating int slider for {field.Name} in {menu.Name}. Range is {min} to {max}, current is {val}", LogVerbosity.Debug);
			// new Slider().onValueChanged.AddListener(f => field.SetValue(GetSettingObject(field),(int) f));
		}

		private void CreateFloatField(float val, FieldInfo field, Menu menu)
		{
			Logger.Log($"\tCreating float field for {field.Name} in {menu.Name}. Current is {val}", LogVerbosity.Debug);
			// new FloatField().RegisterValueChangedCallback(c => field.SetValue(GetSettingObject(field), c.newValue));
		}

		private void CreateIntField(int val, FieldInfo field, Menu menu)
		{
			Logger.Log($"\tCreating int field for {field.Name} in {menu.Name}. Current is {val}", LogVerbosity.Debug);
			// new IntegerField().RegisterValueChangedCallback(c => field.SetValue(GetSettingObject(field), c.newValue));
		}

		private void CreateBoolToggle(bool val, FieldInfo field, Menu menu, GameObject panel)
		{
			Debug.Log($"\tCreating bool toggle for {field.Name} in {menu.Name}. Current is {val}");

			Toggle toggle = optionsPanel.AddToggleToPanel(panel, field.Name, val);
			
			toggle.onValueChanged.AddListener(b => field.SetValue(GetSettingObject(field), b));
		}

		private void CreateStringField(string val, FieldInfo field, Menu menu)
		{
			Logger.Log($"\tCreating string field for {field.Name} in {menu.Name}. Current is {val}", LogVerbosity.Debug);
			//            new TMP_InputField().onValueChanged.AddListener(s => field.SetValue(GetSettingObject(field), s));
		}

		private void CreateEnumDropdown(int val, FieldInfo field, Menu menu, GameObject panel)
		{
			Logger.Log(
				$"\tCreating enum dropdown for {field.Name} in {menu.Name}. Current is {Enum.GetName(field.FieldType, val)}, options are {string.Join(", ", Enum.GetNames(field.FieldType))}",
				LogVerbosity.Debug);
			string[] names = Enum.GetNames(field.FieldType);
			val = names.ToList().IndexOf(Enum.GetName(field.FieldType, val));
			TMP_Dropdown dropdown = optionsPanel.AddDropdownToPanel(panel, names , val);
			
			dropdown.onValueChanged.AddListener(index =>
			{
				// ReSharper disable once LocalVariableHidesMember
				string name = dropdown.options[index].text;
				int value = (int) Enum.Parse(field.FieldType, name);
				field.SetValue(GetSettingObject(field), value);
			});
		}

		private void CreateKeybindButton(KeyCode val, FieldInfo field, Menu menu)
		{
			Logger.Log($"\tCreating keybind button for {field.Name} in {menu.Name}. Current is {val}", LogVerbosity.Debug);
			//Sorry future Creepysin in case this freezes the game...
			//TODO: Need to create this function. Might need to start a coroutine/async void to avoid freezing the screen till a key is pressed
			//            new Button().onClick.AddListener(  () => field.SetValue(GetSettingObject(field), WaitForKeyPressAndReturnKeycode()));
		}

		private object GetSettingObject(FieldInfo field)
		{
			//Find the first setting group where the group type matches that of the field's declaring type
			PropertyInfo settingGroup = GameSettings.GetSettingClasses().First(p => p.PropertyType == field.DeclaringType);
			return settingGroup.GetValue(null);
		}

		// ReSharper restore UnusedParameter.Local
		// ReSharper restore MemberCanBeMadeStatic.Local
		// ReSharper restore ParameterHidesMember

	#endregion
	}
}