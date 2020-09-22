using System;
using System.IO;
using BootManagement;
using Core;
using Helper;
using SceneManagement;
using UnityEngine;
using Discord;
using Logger = Core.Logging.Logger;

namespace Integrations
{
	/// <summary>
	/// Handles communicating with Discord's RPC
	/// </summary>
	public class DiscordManager : MonoBehaviour, IStartOnBoot
	{
		private Discord.Discord client;
		private ActivityManager activityManager;

		/// <summary>
		/// The active running instance
		/// </summary>
		public static DiscordManager Instance;

		/// <summary>
		/// Load the settings on start?
		/// </summary>
		public bool loadSettingsFromFile = true;

		/// <summary>
		/// Where to load the settings from
		/// </summary>
		public string settingsLocation = "/Resources/Integrations/DiscordRPC.json";

		/// <summary>
		/// Settings for the Discord manager to use
		/// </summary>
		public DiscordManagerSettings settings;

		public void Init()
		{
			if (Instance != null || Game.IsHeadless)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(this);

			if(loadSettingsFromFile)
				LoadSettings();

			Initialize();
		}

		private void Update()
		{
			client?.RunCallbacks();
		}

		private void OnDestroy()
		{
			if (Instance == this)
				client.Dispose();
		}

		private void Initialize()
		{
			if (client != null)
			{
				Logger.Error("The discord client is already running!");
				return;
			}

			client = new Discord.Discord(long.Parse(settings.clientId),
				(ulong) CreateFlags.Default);
			client.SetLogHook(settings.logLevel, (level, message) =>
			{
				Logger.Info(message);
			});
			activityManager = client.GetActivityManager();

			TCScenesManager.PreparingSceneLoadEvent += PreparingSceneLoad;
			TCScenesManager.OnSceneLoadedEvent += SceneLoaded;

			SceneLoaded(TCScenesManager.GetActiveScene());
		}

		private void LoadSettings()
		{
			if(string.IsNullOrWhiteSpace(settingsLocation))
				return;

			settings = ObjectSerializer.LoadJson<DiscordManagerSettings>(Path.GetDirectoryName($"{Game.GetGameExecutePath()}{settingsLocation}"),
				$"/{Path.GetFileNameWithoutExtension(settingsLocation)}");
		}

		/// <summary>
		/// Set the rich presence
		/// </summary>
		/// <param name="presence"></param>
		public void UpdatePresence(Activity presence)
		{
			if(client == null) return;

			activityManager.UpdateActivity(presence, result =>
			{
			});
		}

		#region Scene Discord RPC Stuff

		private void PreparingSceneLoad(TCScene scene)
		{
			//Update our RPC to show we are loading
			if (client != null)
			{
				UpdatePresence(new Activity
				{
					Assets = new ActivityAssets
					{
						LargeImage = scene.largeImageKey,
						LargeText = scene.largeImageKeyText
					},
					Details = $"Loading into {scene.displayName}",
					State = "Loading..."
				});
			}
		}

		private void SceneLoaded(TCScene scene)
		{
			if (client != null)
			{
				Activity presence = new Activity
				{
					Assets = new ActivityAssets()
					{
						LargeImage = scene.largeImageKey,
						LargeText = scene.largeImageKeyText
					}
				};

				if(scene.showStartTime)
					presence.Timestamps = new ActivityTimestamps
					{
						Start = DateTime.UtcNow.Millisecond
					};

				if (scene.isOnlineScene)
				{
					presence.Details = TCScenesManager.GetActiveScene().displayName;
					presence.State = "Team Capture";
				}
				else if (scene.isMainMenu)
					presence.Details = "Main Menu";
				else if(!scene.isMainMenu && !scene.isOnlineScene)
					presence.Details = "Loading...";
				else
					Logger.Error("You CANNOT have a online scene and a main menu scene!");

				UpdatePresence(presence);
			}
		}

		#endregion
	}
}