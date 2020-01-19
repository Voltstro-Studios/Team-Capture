using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Global;
using UnityEngine;
using Logger = Global.Logger;

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

		private bool isDuplicate;

		public LogLevel logLevel = LogLevel.Warning;

		private void Start()
		{
			if (Instance != null)
			{
				isDuplicate = true;
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
			if (!isDuplicate)
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

			client.Initialize();

			//Update our rich presence to just have the default
			UpdatePresence(new RichPresence
			{
				Assets = new Assets
				{
					LargeImageKey = defaultLargeImage
				},
				Details = defaultGameDetail,
				State = defaultGameState
			});
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
	}
}