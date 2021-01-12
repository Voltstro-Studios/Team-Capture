using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Team_Capture.Core;
using Team_Capture.Helper;
using Team_Capture.Settings.SettingClasses;
using Team_Capture.UI;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Settings
{
	/// <summary>
	///     Handles game settings
	/// </summary>
	public static class GameSettings
	{
		private const string SettingsFileExtension = ".json";
		private static string settingsSaveDirectory;

		/// <summary>
		///     Invoked when the settings are altered in some way
		/// </summary>
		public static event Action SettingsUpdated;

		#region Settings

		[SettingsPropertyDisplayText("Settings_Mouse")]
		internal static MouseSettingsClass MouseSettings { get; } = new MouseSettingsClass();

		[SettingsPropertyDisplayText("Settings_Video")]
		internal static VideoSettingsClass VideoSettings { get; } = new VideoSettingsClass();

		[SettingsPropertyDisplayText("Settings_Adv")]
		internal static AdvSettingsClass AdvSettings { get; } = new AdvSettingsClass();

		[SettingsPropertyDisplayText("Settings_Multiplayer")]
		internal static MultiplayerSettingsClass MultiplayerSettings { get; } = new MultiplayerSettingsClass();

		#endregion

		#region Saving, loading and resetting setting functions

		//We have this as internal so that our dynamic settings ui generator script can access it too
		/// <summary>
		///     Gets all settings
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<PropertyInfo> GetSettingClasses()
		{
			//Find all of the setting sub-types, and their instances in out main class
			IEnumerable<Type> settingTypes = ReflectionHelper.GetInheritedTypes<Setting>();

			//Get a list of all of the properties in our settings class
			PropertyInfo[] settingProps = typeof(GameSettings).GetTypeInfo()
				.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

			//Only add properties that are one of our inherited setting types
			IEnumerable<PropertyInfo> foundSettings = settingProps.Where(p => settingTypes.Contains(p.PropertyType));

			return foundSettings;
		}

		/// <summary>
		///     Saves the settings
		/// </summary>
		public static void Save()
		{
			foreach (PropertyInfo settingProp in GetSettingClasses())
			{
				Logger.Debug("Saved {@Name}", settingProp.Name);
				ObjectSerializer.SaveJson(settingProp.GetValue(null), settingsSaveDirectory, settingProp.Name);
			}

			SettingsUpdated?.Invoke();
		}

		/// <summary>
		///     Loads the settings
		/// </summary>
		//Assemblies aren't always reloaded in the editor, so we have to do it just before the scene is loaded
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType
			.BeforeSceneLoad)] //This now gets called before the scene is loaded.
#else
		//Otherwise, we're in a build, so we can run once the assemblies are loaded
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType
			.AfterAssembliesLoaded)] //This now gets called as soon as the assemblies are loaded
#endif
		public static void Load()
		{
			settingsSaveDirectory = Game.GetGameConfigPath();

			foreach (PropertyInfo settingProp in GetSettingClasses())
				try
				{
					string name = settingProp.Name;
					Logger.Debug("Got settings `{@Name}`", name);

					if (File.Exists(settingsSaveDirectory + name + SettingsFileExtension))
						//This will enable us to use internal setters on our settings to avoid anyone being able to edit them
						ObjectSerializer.LoadJsonOverwrite(settingProp.GetValue(null), settingsSaveDirectory, name,
							SettingsFileExtension,
							new JsonSerializerSettings {ContractResolver = new NonPublicPropertiesResolver()});
				}
				catch
				{
					// ignored
				}

			Logger.Debug("Loaded settings");

			//Notify other classes that settings have updated
			SettingsUpdated?.Invoke();
		}

		/// <summary>
		///     Resets the settings to their default values
		/// </summary>
		public static void Reset()
		{
			//Now loop over all the settings we found
			foreach (PropertyInfo settingProp in GetSettingClasses())
				//Default constructor
				//settingProp.PropertyType.GetConstructor(Type.EmptyTypes);
				//Create an instance using only the default constructor
				settingProp.SetValue(null, Activator.CreateInstance(settingProp.PropertyType, false));

			//Invoke the reload event to ensure everything updates
			SettingsUpdated?.Invoke();
		}

		#endregion
	}

	#region Setting classes

	/// <summary>
	///     A class that all settings must inherit from
	/// </summary>
	public abstract class Setting
	{
	}

	#endregion
}