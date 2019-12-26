using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Helper;
using Newtonsoft.Json;
using UnityEngine;
using Logger = Global.Logger;

namespace Settings
{
	public static class GameSettings
	{
		public static TestOne TestOne { get; } = new TestOne();

		public static AnotherTest AnotherTest { get; } = new AnotherTest();

		public static AnotherTest AnotherAnotherTest { get; } = new AnotherTest
		{
			String = "I changed the string this time",
			ThisIsAKey = KeyCode.KeypadPeriod
		};

		public static bool HasBeenLoaded { get; private set; }

		public static event Action SettingsLoaded;

		#region Saving, loading and resetting setting functions

		//We have this as internal so that our dynamic settings ui generator script can access it too
		internal static IEnumerable<PropertyInfo> GetSettingClasses()
		{
			//Find all of the setting sub-types, and their instances in out main class
			IEnumerable<Type> settingTypes = ReflectionHelper.GetInheritedTypes<Setting>();
			//Get a list of all of the properties in our settings class
			PropertyInfo[] settingProps = typeof(GameSettings).GetTypeInfo().GetProperties();
			//Only add properties that are one of our inherited setting types
			IEnumerable<PropertyInfo> foundSettings = settingProps.Where(p => settingTypes.Contains(p.PropertyType));

			return foundSettings;
		}

		public static void Save()
		{
			//Create directory if it doesn't exist
			if (!Directory.Exists(Paths.SettingsSaveDirectory)) Directory.CreateDirectory(Paths.SettingsSaveDirectory);

			foreach (PropertyInfo settingProp in GetSettingClasses())
				ObjectSerializer.SaveJson(settingProp.GetValue(null), Paths.SettingsSaveDirectory, settingProp.Name,
					Paths.SettingFileExtension);
		}

		//Assemblies aren't always reloaded in the editor, so we have to do it just before the scene is loaded
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType
			.BeforeSceneLoad)] //This now gets called before the scene is loaded.
#else
		//Otherwise, we're in a build, so we can run once the assemblies are loaded
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] //This now gets called as soon as the assemblies are loaded
#endif
		public static void Load()
		{
			//If the directory doesn't exist, create it
			if (!Directory.Exists(Paths.SettingsSaveDirectory))
			{
				Directory.CreateDirectory(Paths.SettingsSaveDirectory);
				//Now quit
				return;
			}

			foreach (PropertyInfo settingProp in GetSettingClasses())
			{
				string name = settingProp.Name;

				if (File.Exists(Paths.SettingsSaveDirectory + name + Paths.SettingFileExtension))
					//This will enable us to use internal setters on our settings to avoid anyone being able to edit them
					ObjectSerializer.LoadJsonOverwrite(settingProp.GetValue(null), Paths.SettingsSaveDirectory, name,
						Paths.SettingFileExtension,
						new JsonSerializerSettings {ContractResolver = new NonPublicPropertiesResolver()});
			}

			HasBeenLoaded = true;
			Logger.Log("Loaded settings");

			//Notify other classes that settings have updated
			SettingsLoaded?.Invoke();
		}

		public static void Reset()
		{
			//Now loop over all the settings we found
			foreach (PropertyInfo settingProp in GetSettingClasses())
				//Default constructor
				//settingProp.PropertyType.GetConstructor(Type.EmptyTypes);
				//Create an instance using only the default constructor
				settingProp.SetValue(null, Activator.CreateInstance(settingProp.PropertyType, false));

			//Invoke the reload event to ensure everything updates
			SettingsLoaded?.Invoke();
		}

		#endregion
	}

	public static class Paths
	{
		public static string SavedFilesDirectory => $"{Environment.CurrentDirectory}/Saved";
		public static string SettingsSaveDirectory => $"{SavedFilesDirectory}/Settings";
		public static string SettingFileExtension => ".json";
	}

	#region Setting classes

	/// <summary>
	/// A class that all settings must inherit from
	/// </summary>
	public abstract class Setting
	{
	}

	public sealed class TestOne : Setting
	{
		public int Int = 1;
		[Range(1, 10)] public int RangeInt = 7;
		[Range(-10, 58)] public float Slider = 5;
	}

	public sealed class AnotherTest : Setting
	{
		public string String = "I'm a string!!";
		public KeyCode ThisIsAKey = KeyCode.T;
	}

	#endregion
}