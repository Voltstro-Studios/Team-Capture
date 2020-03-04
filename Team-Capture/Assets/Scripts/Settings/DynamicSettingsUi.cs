using System;
using System.Diagnostics;
using System.Reflection;
using Attributes;
using Helper.Extensions;
using Settings.SettingClasses;
using System.Configuration;
using System.Linq;
using UI.Elements.Settings;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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

		private void UpdateUI()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			optionsPanel.ClearPanels();

			//TODO: Holy fucking hell this is ugly
			//Loop over each setting menu and all the sub-settings
			foreach (PropertyInfo settingInfo in GameSettings.GetSettingClasses())
			{
				object menuInstance = settingInfo.GetStaticValue<object>();

				//If it has the SettingsMenuFormatAttribute then use the format name of that
				string settingMenuName = settingInfo.Name;
				if(Attribute.GetCustomAttribute(settingInfo, typeof(SettingsMenuFormatAttribute)) is SettingsMenuFormatAttribute attribute)
					settingMenuName = attribute.MenuNameFormat;

				//Create a menu module
				Menu settingMenu = new Menu(settingMenuName);
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
							CreateIntSlider(settingField.GetValue<int>(menuInstance), (int)rangeAttribute.min, (int)rangeAttribute.max, settingField, settingMenu);
						else
							CreateIntField(settingField.GetValue<int>(menuInstance), settingField, settingMenu);
					}
					else if (fieldType == typeof(float))
					{
						RangeAttribute rangeAttribute = settingField.GetCustomAttribute<RangeAttribute>();
						if (rangeAttribute != null)
							CreateFloatSlider(settingField.GetValue<float>(menuInstance), rangeAttribute.min,
								rangeAttribute.max,
								settingField, settingMenu);
						else
							CreateFloatField(settingField.GetValue<float>(menuInstance), settingField,
								settingMenu);
					}
					else if (fieldType == typeof(bool))
					{
						CreateBoolToggle(settingField.GetValue<bool>(menuInstance), settingField, settingMenu, panel);
					}
					else if (fieldType == typeof(string))
					{
						CreateStringField(settingField.GetValue<string>(menuInstance), settingField, settingMenu);
					}
					//TODO: Finish these
					else if (fieldType.IsEnum)
					{
						if (fieldType == typeof(KeyCode))
							//We don't do a dropdown, we create a button and do some complicated shit that i'll steal from one of my other games
							CreateKeybindButton(settingField.GetValue<KeyCode>(menuInstance),
								settingField,
								settingMenu);
						//Just a normal enum popup
						else
							CreateEnumDropdown(settingField.GetValue<int>(menuInstance), settingField,
								settingMenu);
					}
				}
			}

			stopwatch.Stop();
			Debug.Log($"Time taken to update UI: {stopwatch.Elapsed.TotalMilliseconds:n} ms");
		}

		#region Graphic designer functions

		// ReSharper disable ParameterHidesMember
		// ReSharper disable MemberCanBeMadeStatic.Local
		// ReSharper disable UnusedParameter.Local

		//Use onValueChanged.AddListener so that every time one of the graphics gets updated, so does our setting
		private void CreateFloatSlider(float val, float min, float max, FieldInfo field, Menu menu)
		{
			Debug.Log(
				$"\tCreating float slider for {field.Name} in {menu.Name}. Range is {min} to {max}, current is {val}");
//            new Slider().onValueChanged.AddListener(f => field.SetValue(null, f));
		}

		private void CreateIntSlider(int val, int min, int max, FieldInfo field, Menu menu)
		{
			Debug.Log(
				$"\tCreating int slider for {field.Name} in {menu.Name}. Range is {min} to {max}, current is {val}");
//            new Slider().onValueChanged.AddListener(f => field.SetValue(null,(int) f));
		}

		private void CreateFloatField(float val, FieldInfo field, Menu menu)
		{
			Debug.Log($"\tCreating float field for {field.Name} in {menu.Name}. Current is {val}");
//            new FloatField().RegisterValueChangedCallback(c => field.SetValue(null, c.newValue));
		}

		private void CreateIntField(int val, FieldInfo field, Menu menu)
		{
			Debug.Log($"\tCreating int field for {field.Name} in {menu.Name}. Current is {val}");
//            new IntegerField().RegisterValueChangedCallback(c => field.SetValue(null, c.newValue));
		}

		private void CreateBoolToggle(bool val, FieldInfo field, Menu menu, GameObject panel)
		{
			Debug.Log(
				$"\tCreating bool toggle for {field.Name} in {menu.Name}. Current is {val}");

			Toggle toggle = optionsPanel.AddToggleToPanel(panel, field.Name);

			object settingInstance = GetSettingObject(field);
			
			toggle.onValueChanged.AddListener(b => field.SetValue(settingInstance, b));
		}

		private void CreateStringField(string val, FieldInfo field, Menu menu)
		{
			Debug.Log($"\tCreating string field for {field.Name} in {menu.Name}. Current is {val}");
//            new TMP_InputField().onValueChanged.AddListener(s => field.SetValue(null, s));
		}

		private void CreateEnumDropdown(int val, FieldInfo field, Menu menu)
		{
			Debug.Log(
				$"\tCreating enum dropdown for {field.Name} in {menu.Name}. Current is {val}, options are {string.Join(", ", Enum.GetNames(field.FieldType))}");
//            new Dropdown().onValueChanged.AddListener(i => field.SetValue(null, i));
		}

		private void CreateKeybindButton(KeyCode val, FieldInfo field, Menu menu)
		{
			Debug.Log($"\tCreating keybind button for {field.Name} in {menu.Name}. Current is {val}");
//            new Button().onClick.AddListener(  () => field.SetValue(null, WaitForKeyPressAndReturnKeycode()));
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