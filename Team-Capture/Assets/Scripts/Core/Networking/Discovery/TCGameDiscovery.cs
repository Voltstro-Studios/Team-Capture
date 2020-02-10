using System;
using System.Net;
using Core.Logger;
using Mirror.Discovery;
using SceneManagement;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Networking.Discovery
{
	[RequireComponent(typeof(TCNetworkManager))]
	public class TCGameDiscovery : NetworkDiscoveryBase<TCServerRequest, TCServerResponse>
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

			Logger.Logger.Log("Game discovery is ready!");
		}

		private void OnDestroy()
		{
			Logger.Logger.Log("Game discovery has been destroyed.");
		}

		protected override TCServerResponse ProcessRequest(TCServerRequest request, IPEndPoint endpoint)
		{
			Logger.Logger.Log($"Processing discovery request from `{endpoint.Address}`...", LogVerbosity.Debug);

			try
			{
				return new TCServerResponse
				{
					GameName = "WIP", //TODO: Do game name stuff
					MaxPlayers = netManager.maxConnections,
					CurrentAmountOfPlayers = netManager.numPlayers,
					SceneName = TCScenesManager.GetActiveScene().name
				};
			}
			catch (NotImplementedException)
			{
				Logger.Logger.Log("Current transport does not support network discovery!", LogVerbosity.Error);
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

		protected override TCServerRequest GetRequest() => new TCServerRequest();

		#endregion
	}
}