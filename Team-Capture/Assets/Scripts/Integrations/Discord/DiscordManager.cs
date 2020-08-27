using System;
using Core;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using SceneManagement;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Integrations.Discord
{
	public class DiscordManager : MonoBehaviour
	{
		public static DiscordManager Instance;

		private DiscordRpcClient client;

		public string clientId;

		public string defaultGameDetail = "Loading...";
		public string defaultGameState = "Loading...";
		public string defaultLargeImage = "tc_icon";

		public LogLevel logLevel = LogLevel.Warning;

		private void Awake()
		{
			if (Instance != null || Game.IsHeadless)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(this);

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
			TCDiscordLogger logger = new TCDiscordLogger {Level = logLevel};

			//Setup the Discord client
			client = new DiscordRpcClient(clientId, -1, logger, false, new UnityNamedPipe());

			client.OnError += ClientError;
			client.OnReady += ClientReady;
			client.OnConnectionFailed += ClientConnectionFailed;
			client.OnClose += ClientOnClose;

			client.Initialize();

			TCScenesManager.PreparingSceneLoadEvent += PreparingSceneLoad;
			TCScenesManager.OnSceneLoadedEvent += SceneLoaded;

			SceneLoaded(TCScenesManager.GetActiveScene());
		}

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

		public void UpdatePresence(RichPresence presence)
		{
			client.SetPresence(presence);
		}

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