using System;
using System.Net;
using Mirror;
using Mirror.Discovery;
using Team_Capture.Core.Compression;
using Team_Capture.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking.Discovery
{
	#region Messages

	internal class TCServerRequest : NetworkMessage
	{
		public string ApplicationVersion;
	}

	internal class TCServerResponse : NetworkMessage
	{
		public int CurrentAmountOfPlayers;

		public CompressedNetworkString GameName;

		public int MaxPlayers;

		public string SceneName;

		public IPEndPoint EndPoint { get; set; }
	}

	#endregion

	[Preserve]
	internal static class TCServerRequestNetwork
	{
		public static void WriteServerResponse(this NetworkWriter writer, TCServerResponse response)
		{
			writer.WriteInt(response.CurrentAmountOfPlayers);
			response.GameName.Write(writer);
			writer.WriteInt(response.MaxPlayers);
			writer.WriteString(response.SceneName);
		}

		public static TCServerResponse ReadServerResponse(this NetworkReader reader)
		{
			return new TCServerResponse
			{
				CurrentAmountOfPlayers = reader.ReadInt(),
				GameName = CompressedNetworkString.Read(reader),
				MaxPlayers = reader.ReadInt(),
				SceneName = reader.ReadString()
			};
		}
	}

	[RequireComponent(typeof(TCNetworkManager))]
	internal class TCGameDiscovery : NetworkDiscoveryBase<TCServerRequest, TCServerResponse>
	{
		/// <summary>
		///     Invoked when a new server was found, if discovering is happening
		/// </summary>
		public readonly ServerFoundUnityEvent OnServerFound = new ServerFoundUnityEvent();

		/// <summary>
		///     The active network manager
		/// </summary>
		private TCNetworkManager netManager;

		private void Awake()
		{
			//Get our network manager
			netManager = GetComponent<TCNetworkManager>();

			//Set the active game discovery to this discovery object
			netManager.gameDiscovery = this;

			if (!Game.IsGameQuitting)
				Logger.Debug("Game discovery is ready!");
		}

		protected override TCServerResponse ProcessRequest(TCServerRequest request, IPEndPoint endpoint)
		{
			Logger.Debug("Processing discovery request from `{Address}`...", endpoint.Address);

			try
			{
				if (request.ApplicationVersion != Application.version)
					return null;

				return new TCServerResponse
				{
					GameName = TCNetworkManager.Instance.serverConfig.GameName,
					MaxPlayers = netManager.maxConnections,
					CurrentAmountOfPlayers = netManager.numPlayers,
					SceneName = TCScenesManager.GetActiveScene().name
				};
			}
			catch (NotImplementedException)
			{
				Logger.Error("Current transport does not support network discovery!");
				throw;
			}
		}

		public class ServerFoundUnityEvent : UnityEvent<TCServerResponse>
		{
		}

		#region Client

		protected override void ProcessResponse(TCServerResponse response, IPEndPoint endpoint)
		{
			if (response == null)
				return;

			//So we found a server, invoke the onServerFound event
			response.EndPoint = endpoint;
			OnServerFound.Invoke(response);
		}

		protected override TCServerRequest GetRequest()
		{
			return new TCServerRequest
			{
				ApplicationVersion = Application.version
			};
		}

		#endregion
	}
}