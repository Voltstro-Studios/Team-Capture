using System;
using System.Collections.Generic;
using System.Linq;
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
		public class ServerFoundUnityEvent : UnityEvent<TCServerResponse> { };

		public static ServerFoundUnityEvent OnServerFound = new ServerFoundUnityEvent();

		private TCNetworkManager netManager;

		public override void Start()
		{
			base.Start();

			netManager = GetComponent<TCNetworkManager>();
		}

		protected override TCServerResponse ProcessRequest(TCServerRequest request, IPEndPoint endpoint)
		{
			Logger.Logger.Log($"Processing discovery request from `{endpoint.Address}`...", LogVerbosity.Debug);

			try
			{
				return new TCServerResponse
				{
					GameName = "WIP",
					MaxPlayers = netManager.maxConnections,
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

			response.EndPoint = endpoint;

			OnServerFound.Invoke(response);

			Logger.Logger.Log($"Found server at {endpoint.Address}", LogVerbosity.Debug);
		}

		protected override TCServerRequest GetRequest() => new TCServerRequest();

		#endregion
	}
}