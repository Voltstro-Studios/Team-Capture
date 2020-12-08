using System;
using System.IO;
using BootManagement;
using Core;
using Core.Logging;
using Discord.GameSDK;
using Discord.GameSDK.Activities;
using Helper;
using SceneManagement;

namespace Integrations
{
	/// <summary>
	///     Handles communicating with Discord's game SDK
	/// </summary>
	internal class DiscordManager : SingletonMonoBehaviour<DiscordManager>, IStartOnBoot
	{
		/// <summary>
		///     Where to load the settings from
		/// </summary>
		public string settingsLocation = "/Resources/Integrations/DiscordRPC.json";

		/// <summary>
		///     Settings for the Discord manager to use
		/// </summary>
		public DiscordManagerSettings settings;

		private ActivityManager activityManager;
		private Discord.GameSDK.Discord client;

		private void Update()
		{
			client?.RunCallbacks();
		}

		public void Init()
		{
		}

		protected override void SingletonAwakened()
		{
		}

		protected override void SingletonStarted()
		{
			if (Game.IsHeadless)
			{
				Destroy(gameObject);
				return;
			}

			LoadSettings();
			Initialize();
		}

		protected override void SingletonDestroyed()
		{
			Logger.Debug("Destroying discord integration...");
			//Using Null Propagation seems to crash Unity...
			// ReSharper disable once UseNullPropagation
			if (client != null)
				client.Dispose();
		}

		private void Initialize()
		{
			if (client != null)
			{
				Logger.Error("The discord client is already running!");
				return;
			}

			try
			{
				client = new Discord.GameSDK.Discord(long.Parse(settings.clientId), CreateFlags.NoRequireDiscord);
				client.Init();
			}
			catch (ResultException ex)
			{
				Logger.Error("Failed to connect with Discord! {@Message} {@ResultCode", ex.Message, ex.Result);
				client = null;
				Destroy(gameObject);
				return;
			}

			client?.SetLogHook(settings.logLevel, (level, message) =>
			{
				switch (level)
				{
					case LogLevel.Error:
						Logger.Error(message);
						break;
					case LogLevel.Warn:
						Logger.Warn(message);
						break;
					case LogLevel.Info:
						Logger.Info(message);
						break;
					case LogLevel.Debug:
						Logger.Debug(message);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(level), level, null);
				}
			});
			activityManager = client?.GetActivityManager();

			TCScenesManager.PreparingSceneLoadEvent += PreparingSceneLoad;
			TCScenesManager.OnSceneLoadedEvent += SceneLoaded;

			SceneLoaded(TCScenesManager.GetActiveScene());
		}

		private void LoadSettings()
		{
			if (string.IsNullOrWhiteSpace(settingsLocation))
				return;

			settings = ObjectSerializer.LoadJson<DiscordManagerSettings>(
				Path.GetDirectoryName($"{Game.GetGameExecutePath()}{settingsLocation}"),
				$"/{Path.GetFileNameWithoutExtension(settingsLocation)}");
		}

		/// <summary>
		///     Updates the active Discord activity that is shown (AkA the Rich Presence)
		/// </summary>
		/// <param name="activity"></param>
		public static void UpdateActivity(Activity activity)
		{
			if (Instance == null) return;
			if (Instance.client == null) return;

			Instance.activityManager.UpdateActivity(activity,
				result => { Logger.Debug($"[Discord Presence] Updated activity: {result}"); });
		}

		#region Scene Discord RPC Stuff

		private void PreparingSceneLoad(TCScene scene)
		{
			//Update our RPC to show we are loading
			if (client != null)
				UpdateActivity(new Activity
				{
					Assets = new ActivityAssets
					{
						LargeImage = scene.largeImageKey,
						LargeText = scene.LargeImageKeyTextLocalized
					},
					Details = $"Loading into {scene.DisplayNameLocalized}",
					State = "Loading..."
				});
		}

		private void SceneLoaded(TCScene scene)
		{
			if (client != null)
			{
				Activity presence = new Activity
				{
					Assets = new ActivityAssets
					{
						LargeImage = scene.largeImageKey,
						LargeText = scene.LargeImageKeyTextLocalized
					}
				};

				if (scene.showStartTime)
					presence.Timestamps = new ActivityTimestamps
					{
						Start = TimeHelper.UnixTimeNow()
					};

				if (scene.isOnlineScene)
				{
					presence.Details = TCScenesManager.GetActiveScene().DisplayNameLocalized;
					presence.State = "Team Capture";
				}
				else if (scene.isMainMenu)
				{
					presence.Details = "Main Menu";
				}
				else if (!scene.isMainMenu && !scene.isOnlineScene)
				{
					presence.Details = "Loading...";
				}
				else
				{
					Logger.Error("You CANNOT have a online scene and a main menu scene!");
				}

				UpdateActivity(presence);
			}
		}

		#endregion
	}
}