using System;
using System.IO;
using BootManagement;
using Core;
using DiscordRPC;
using DiscordRPC.Message;
using Helper;
using SceneManagement;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Integrations.Discord
{
	/// <summary>
	/// Handles communicating with Discord's RPC
	/// </summary>
	public class DiscordManager : MonoBehaviour, IStartOnBoot
	{
		private DiscordRpcClient client;

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
		public string settingsLocation = "/Resources/DiscordRPC.json";

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

		private void FixedUpdate()
		{
			client?.Invoke();
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

			//Setup our Discord logger to work with our custom logger
			TCDiscordLogger logger = new TCDiscordLogger {Level = settings.logLevel};

			//Setup the Discord client
			client = new DiscordRpcClient(settings.clientId, -1, logger, false, new UnityNamedPipe());

			client.OnError += ClientError;
			client.OnReady += ClientReady;
			client.OnConnectionFailed += ClientConnectionFailed;
			client.OnClose += ClientOnClose;

			client.Initialize();

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
		public void UpdatePresence(RichPresence presence)
		{
			client.SetPresence(presence);
		}

		#region Client Events

		private void ClientOnClose(object sender, CloseMessage args)
		{
			Logger.Info("Discord RPC was closed: {@Code}:{@Reason}", args.Code, args.Reason);
		}

		private void ClientConnectionFailed(object sender, ConnectionFailedMessage args)
		{
			Logger.Error("Error communicating with Discord: Pipe: `{FailedPipe}`. Is Discord running?", args.FailedPipe);

			client.Deinitialize();
		}

		private void ClientReady(object sender, ReadyMessage args)
		{
			Logger.Info("Client ready: {@Username}", args.User.Username);
		}

		private void ClientError(object sender, ErrorMessage args)
		{
			Logger.Error("Error with Discord RPC: {@Code}:{@Message}", args.Code, args.Message);
		}

		#endregion

		#region Scene Discord RPC Stuff

		private void PreparingSceneLoad(TCScene scene)
		{
			//Update our RPC to show we are loading
			if (client.IsInitialized)
			{
				UpdatePresence(new RichPresence
				{
					Assets = new Assets
					{
						LargeImageKey = scene.largeImageKey,
						LargeImageText = scene.largeImageKeyText
					},
					Details = $"Loading into {scene.displayName}",
					State = "Loading..."
				});
			}
		}

		private void SceneLoaded(TCScene scene)
		{
			if (client.IsInitialized)
			{
				RichPresence presence = new RichPresence
				{
					Assets = new Assets
					{
						LargeImageKey = scene.largeImageKey,
						LargeImageText = scene.largeImageKeyText
					}
				};

				if(scene.showStartTime)
					presence.Timestamps = new Timestamps(DateTime.UtcNow, null);

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