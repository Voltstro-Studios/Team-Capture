using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Global;
using Helper;
using Newtonsoft.Json;
using UnityEngine;
using Logger = Global.Logger;

namespace Settings
{
	public static class GameSettings
	{
		public static event Action SettingsLoaded;

		public static Input Input { get; private set; } = new Input();
		public static bool HasBeenLoaded { get; private set; }

		#region Saving, loading and resetting setting functions

		public static void Save()
		{
			//Create directory if it doesn't exist
			if (!Directory.Exists(Paths.SettingsSaveDirectory))
			{
				Directory.CreateDirectory(Paths.SettingsSaveDirectory);
			}

			//Find all of the setting sub-types, and their instances in out main class
			IEnumerable<Type> settingTypes = ReflectionHelper.GetInheritedTypes<Setting>();
			//Get a list of all of the properties in our settings class
			PropertyInfo[] settingProps = typeof(GameSettings).GetTypeInfo().GetProperties();
			//Only add properties that are one of our inherited setting types
			IEnumerable<PropertyInfo> foundSettings = settingProps.Where(p => settingTypes.Contains(p.PropertyType));
			foreach (PropertyInfo settingProp in foundSettings)
			{
				ObjectSerializer.SaveJson(settingProp.GetValue(null), Paths.SettingsSaveDirectory, settingProp.Name,
					extension: Paths.SettingFileExtension);
			}
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

			//Find all of the setting sub-types, and their instances in out main class
			IEnumerable<Type> settingTypes = ReflectionHelper.GetInheritedTypes<Setting>();
			//Get a list of all of the properties in our settings class
			PropertyInfo[] settingProps = typeof(GameSettings).GetTypeInfo().GetProperties();
			//Only add properties that are one of our inherited setting types
			IEnumerable<PropertyInfo> foundSettings = settingProps.Where(p => settingTypes.Contains(p.PropertyType));

			foreach (PropertyInfo settingProp in foundSettings)
			{
				string name = settingProp.Name;

				if (File.Exists(Paths.SettingsSaveDirectory + name + Paths.SettingFileExtension))
				{
					//This will enable us to use internal setters on our settings to avoid anyone being able to edit them
					ObjectSerializer.LoadJsonOverwrite(settingProp.GetValue(null), Paths.SettingsSaveDirectory, name,
						Paths.SettingFileExtension,
						new JsonSerializerSettings {ContractResolver = new NonPublicPropertiesResolver()});
				}
			}

			HasBeenLoaded = true;
			Logger.Log("Loaded settings", LogVerbosity.INFO);

			//Notify other classes that settings have updated
			SettingsLoaded?.Invoke();
		}
		
		public static void Reset()
		{
			//Find all of the setting sub-types, and their instances in out main class
			IEnumerable<Type> settingTypes = ReflectionHelper.GetInheritedTypes<Setting>();
			//Get a list of all of the properties in our settings class
			PropertyInfo[] settingProps = typeof(GameSettings).GetTypeInfo().GetProperties();
			//Only add properties that are one of our inherited setting types
			IEnumerable<PropertyInfo> foundSettings = settingProps.Where(p => settingTypes.Contains(p.PropertyType));
			//Now loop over all the settings we found
			foreach (PropertyInfo settingProp in foundSettings)
			{
				//Default constructor
				//settingProp.PropertyType.GetConstructor(Type.EmptyTypes);
				//Create an instance using only the default constructor
				settingProp.SetValue(null, Activator.CreateInstance(settingProp.PropertyType, nonPublic: false));
			}

			//Invoke the reload event to ensure everything updates
			SettingsLoaded?.Invoke();
		}
		
		#endregion
	}

	public static class Paths
	{
		public static string SavedFilesDirectory => "./Saved";
		public static string SettingsSaveDirectory => $"{SavedFilesDirectory}/Settings";
		public static string SettingFileExtension => ".json";
	}

	#region Setting classes

	/// <summary>
	///     A class that all settings must inherit from
	/// </summary>
	public abstract class Setting
	{
	}

	public sealed class Input
	{
	}

	public sealed class Logging
	{
		public static bool InterceptUnityDebug = true;
		public static string LogFileExtension => ".log";
		public static string LogSaveDirectory => $"{Paths.SavedFilesDirectory}/Logs";
	}

	#endregion
}