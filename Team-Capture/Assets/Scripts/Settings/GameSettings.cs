using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Attributes;
using Core;
using Helper;
using Newtonsoft.Json;
using Settings.SettingClasses;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Settings
{
	public static class GameSettings
	{
		private const string SettingsFileExtension = ".json";
		private static string settingsSaveDirectory;

		#region Settings

		[SettingsPropertyFormatName("Video Settings")]
		public static VideoSettingsClass VideoSettings { get; } = new VideoSettingsClass();

		[SettingsPropertyFormatName("Advance Settings")]
		public static AdvSettingsClass AdvSettings { get; } = new AdvSettingsClass();

		#endregion

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
			foreach (PropertyInfo settingProp in GetSettingClasses())
			{
				Logger.Debug("Saved {@Name}", settingProp.Name);
				ObjectSerializer.SaveJson(settingProp.GetValue(null), settingsSaveDirectory, settingProp.Name);
			}

			SettingsLoaded?.Invoke();
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
			settingsSaveDirectory = Game.GetGameConfigPath();

			foreach (PropertyInfo settingProp in GetSettingClasses())
			{
				string name = settingProp.Name;
				Logger.Debug("Got settings `{@Name}`", name);

				if (File.Exists(settingsSaveDirectory + name + SettingsFileExtension))
					//This will enable us to use internal setters on our settings to avoid anyone being able to edit them
					ObjectSerializer.LoadJsonOverwrite(settingProp.GetValue(null), settingsSaveDirectory, name,
						SettingsFileExtension,
						new JsonSerializerSettings {ContractResolver = new NonPublicPropertiesResolver()});
			}

			HasBeenLoaded = true;
			Logger.Debug("Loaded settings");

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

	#region Setting classes

	/// <summary>
	/// A class that all settings must inherit from
	/// </summary>
	public abstract class Setting
	{
	}

	#endregion
}