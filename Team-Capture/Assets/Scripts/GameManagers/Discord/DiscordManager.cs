using System;
using Core.Logger;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using SceneManagement;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace GameManagers.Discord
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

		private void Start()
		{
			if (Instance != null)
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
				Logger.Log("The discord client is already running!", LogVerbosity.Error);
				return;
			}

			//Setup our Discord logger to work with our custom logger
			TCDiscordLogger logger = new TCDiscordLogger {Level = logLevel};

			//Setup the Discord client
			client = new DiscordRpcClient(clientId, -1, logger, false, new UnityNamedPipe());

			client.OnError += ClientError;
			client.OnReady += ClientReady;
			client.OnConnectionFailed += ClientConnectionFailed;

			client.Initialize();

			TCScenesManager.PreparingSceneLoadEvent += PreparingSceneLoad;
			TCScenesManager.OnSceneLoadedEvent += SceneLoaded;

			SceneLoaded(TCScenesManager.GetActiveScene());
		}

		private void ClientConnectionFailed(object sender, ConnectionFailedMessage args)
		{
			Logger.Log($"Error communicating with Discord: Pipe: `{args.FailedPipe}`. Is discord running?", LogVerbosity.Error);
		}

		private void ClientReady(object sender, ReadyMessage args)
		{
			Logger.Log("Client ready: " + args.User.Username);
		}

		private void ClientError(object sender, ErrorMessage args)
		{
			Logger.Log($"Error with Discord RPC: {args.Code}:{args.Message}");
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
					Logger.Log("You CANNOT have a online scene and a main menu scene!", LogVerbosity.Error);

				UpdatePresence(presence);
			}
		}

		#endregion
	}
}