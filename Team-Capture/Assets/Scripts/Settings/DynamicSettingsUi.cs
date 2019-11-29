using System;
using System.Diagnostics;
using System.Reflection;
using Helper.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Settings
{
    [ExecuteAlways]
    public class DynamicSettingsUi : MonoBehaviour
    {
        [Tooltip("This just updates our UI. ")]
        public bool update;

        //We use Update() not OnValidate because you can't destroy objects in OnValidate() while not in play mode (which is where we'll probably be testing)
        private void Update()
        {
            if (!update) return;
            update = false;
            UpdateUi();
        }

        private void ClearOldUi()
        {
            //Do some stuff here
        }

        private void UpdateUi()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ClearOldUi();

            stopwatch.Stop();

            Debug.Log($"Time to clear: {stopwatch.ElapsedMilliseconds:n} ms");

            //TODO: Holy fucking hell this is ugly
            //Loop over each setting menu and all the sub-settings
            stopwatch.Restart();
            foreach (PropertyInfo settingMenuInfo in GameSettings.GetSettingClasses())
            {
                object menuInstance = settingMenuInfo.GetStaticValue<object>();
                //Create a menu button
                //TODO: Make this split words in camel case e.g. "CamelCaseIsGood" => "Camel Case Is Good"
                Menu settingMenu = new Menu(settingMenuInfo.Name);
                CreateSettingMenu(settingMenu);

                FieldInfo[] subSettingInfos =
                    settingMenuInfo.PropertyType.GetFields(BindingFlags.Instance | BindingFlags.Public);

                for (int i = 0; i < subSettingInfos.Length; i++)
                {
                    FieldInfo subSettingField = subSettingInfos[i];

                    //TODO: Should we cache the return value of subSetting.FieldType?
                    //TODO: Considering the unity slider uses a float, could we have 1 overload with a bool for int or float mode?
                    if (subSettingField.FieldType == typeof(int))
                    {
                        //If it's an int or float, we need to check if it has a range attribute
                        RangeAttribute rangeAttribute = subSettingField.GetCustomAttribute<RangeAttribute>();
                        //If it has a range attribute, create a slider, otherwise use a input field
                        if (rangeAttribute != null)
                            CreateIntSlider(subSettingField.GetValue<int>(menuInstance), (int) rangeAttribute.min,
                                (int) rangeAttribute.max, subSettingField, settingMenu);
                        else
                            CreateIntField(subSettingField.GetValue<int>(menuInstance), subSettingField, settingMenu);
                    }
                    else if (subSettingField.FieldType == typeof(float))
                    {
                        RangeAttribute rangeAttribute = subSettingField.GetCustomAttribute<RangeAttribute>();
                        if (rangeAttribute != null)
                            CreateFloatSlider(subSettingField.GetValue<float>(menuInstance), rangeAttribute.min,
                                rangeAttribute.max,
                                subSettingField, settingMenu);
                        else
                            CreateFloatField(subSettingField.GetValue<float>(menuInstance), subSettingField,
                                settingMenu);
                    }
                    else if (subSettingField.FieldType == typeof(bool))
                    {
                        CreateBoolToggle(subSettingField.GetValue<bool>(menuInstance), subSettingField, settingMenu);
                    }
                    else if (subSettingField.FieldType == typeof(string))
                    {
                        CreateStringField(subSettingField.GetValue<string>(menuInstance), subSettingField, settingMenu);
                    }
                    //TODO: Finish these
                    else if (subSettingField.FieldType.IsEnum)
                    {
                        if (subSettingField.FieldType == typeof(KeyCode))
                            //We don't do a dropdown, we create a button and do some complicated shit that i'll steal from one of my other games
                            CreateKeybindButton(subSettingField.GetValue<KeyCode>(menuInstance),
                                subSettingField,
                                settingMenu);
                        //Just a normal enum popup
                        else
                            CreateEnumDropdown(subSettingField.GetValue<int>(menuInstance), subSettingField,
                                settingMenu);
                    }
                }
            }

            stopwatch.Stop();
            Debug.Log($"Time taken to update UI: {stopwatch.Elapsed.TotalMilliseconds:n} ms");
        }

        private class Menu
        {
            // ReSharper disable once NotAccessedField.Local
            // ReSharper disable once MemberCanBePrivate.Local
            internal readonly string Name;

            internal Menu(string name)
            {
                Name = name;
            }
        }

        #region Graphic designer functions

        // ReSharper disable ParameterHidesMember
        // ReSharper disable MemberCanBeMadeStatic.Local
        // ReSharper disable UnusedParameter.Local

        private void CreateSettingMenu(Menu menu)
        {
            Debug.Log($"Found setting menu {menu.Name}");
//            new GameObject(menu.Name);
        }

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

        private void CreateBoolToggle(bool val, FieldInfo field, Menu menu)
        {
            Debug.Log(
                $"\tCreating bool toggle for {field.Name} in {menu.Name}. Current is {val}");
//            new Toggle().onValueChanged.AddListener(b => field.SetValue(null, b));
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

        // ReSharper restore UnusedParameter.Local
        // ReSharper restore MemberCanBeMadeStatic.Local
        // ReSharper restore ParameterHidesMember

        #endregion
    }
}