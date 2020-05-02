using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace VoltBuilder
{
	public static class ConfigManager
	{
		/// <summary>
		/// The config path
		/// </summary>
		private const string ConfigFile = "VoltBuild.json";

		/// <summary>
		/// The config
		/// </summary>
		public static Config Config;

		/// <summary>
		/// Handles config stuff
		/// </summary>
		static ConfigManager()
		{
			//Make sure the directory exists
			if (!Directory.Exists(GetSettingsPath()))
				Directory.CreateDirectory(GetSettingsPath());

			//Make sure the file exists
			if (!File.Exists(GetSettingsPath() + ConfigFile))
			{
				Config = NewConfig();

				SaveConfig();

				Debug.Log("Created new VoltBuilder config.");
				return;
			}

			string json = File.ReadAllText(GetSettingsPath() + ConfigFile);
			Config = JsonConvert.DeserializeObject<Config>(json, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
		}

		/// <summary>
		/// Saves the config (duh)
		/// </summary>
		public static void SaveConfig()
		{
			string json = JsonConvert.SerializeObject(Config, Formatting.Indented, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
			File.WriteAllText(GetSettingsPath() + ConfigFile, json);
		}

		/// <summary>
		/// Gets the custom config for build settings
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="config"></param>
		/// <returns></returns>
		public static bool GetBuildConfig<T>(out T config)
		{
			if (Config.BuildOptions is T options)
			{
				config = options;
				return true;
			}

			throw new InvalidCastException("T isn't the current build options!");
		}

		private static Config NewConfig()
		{
			return new Config
			{
				ProjectName = Application.productName,
				BuildDir = "Build/",
				Scenes = new List<Scene>(),
				BuildOptions = new DefaultBuildConfig
				{
					BuildTarget = EditorUserBuildSettings.activeBuildTarget,
					ZipFiles = false,
					CopyPDBFiles = false,
					DevBuild = false,
					ServerBuild = false,
					FilesToCopyOnBuild = new List<FileToCopy>()
				}
			};
		}

		/// <summary>
		/// Gets the path to the VoltBuild config directory
		/// </summary>
		/// <returns></returns>
		private static string GetSettingsPath()
		{
			return $"{Application.dataPath.Replace("Assets", "")}ProjectSettings/VoltBuild/" ;
		}
	}
}