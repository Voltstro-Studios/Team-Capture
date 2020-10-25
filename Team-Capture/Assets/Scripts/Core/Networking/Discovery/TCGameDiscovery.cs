using System;
using System.Net;
using Mirror.Discovery;
using SceneManagement;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Networking.Discovery
{
	[RequireComponent(typeof(TCNetworkManager))]
	internal class TCGameDiscovery : NetworkDiscoveryBase<TCServerRequest, TCServerResponse>
	{
		public class ServerFoundUnityEvent : UnityEvent<TCServerResponse> { }

		/// <summary>
		/// Invoked when a new server was found, if discovering is happening
		/// </summary>
		public ServerFoundUnityEvent onServerFound = new ServerFoundUnityEvent();

		/// <summary>
		/// The active network manager
		/// </summary>
		private TCNetworkManager netManager;

		private void Awake()
		{
			//Get our network manager
			netManager = GetComponent<TCNetworkManager>();

			//Set the active game discovery to this discovery object
			netManager.gameDiscovery = this;

			Logging.Logger.Debug("Game discovery is ready!");
		}

		private void OnDestroy()
		{
			Logging.Logger.Debug("Game discovery has been destroyed.");
		}

		protected override TCServerResponse ProcessRequest(TCServerRequest request, IPEndPoint endpoint)
		{
			Logging.Logger.Debug("Processing discovery request from `{@Address}`...", endpoint.Address);

			try
			{
				if (request.ApplicationVersion != Application.version)
					return null;

				return new TCServerResponse
				{
					GameName = TCNetworkManager.Instance.serverConfig.gameName,
					MaxPlayers = netManager.maxConnections,
					CurrentAmountOfPlayers = netManager.numPlayers,
					SceneName = TCScenesManager.GetActiveScene().name
				};
			}
			catch (NotImplementedException)
			{
				Logging.Logger.Error("Current transport does not support network discovery!");
				throw;
			}
		}

		#region Client

		protected override void ProcessResponse(TCServerResponse response, IPEndPoint endpoint)
		{
			if(response == null)
				return;

			//So we found a server, invoke the onServerFound event
			response.EndPoint = endpoint;
			onServerFound.Invoke(response);
		}

		protected override TCServerRequest GetRequest() => new TCServerRequest
		{
			ApplicationVersion = Application.version
		};

		#endregion
	}
}