using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Logger;
using Mirror.Discovery;
using SceneManagement;
using UnityEngine;

namespace Core.Networking.Discovery
{
	[RequireComponent(typeof(TCNetworkManager))]
	public class TCGameDiscovery : NetworkDiscoveryBase<TCServerRequest, TCServerResponse>
	{
		private TCNetworkManager netManager;

		private static readonly List<TCServerResponse> Servers = new List<TCServerResponse>();

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

			//If we already have this IP then ignore
			if(Servers.Any(x => Equals(x.EndPoint, endpoint)))
				return;

			response.EndPoint = endpoint;

			Servers.Add(response);

			Logger.Logger.Log($"Found server at {endpoint.Address}", LogVerbosity.Debug);
		}

		protected override TCServerRequest GetRequest() => new TCServerRequest();

			#endregion

		#region Server List Functions

		public static TCServerResponse[] GetServers()
		{
			return Servers.ToArray();
		}

		public static void ClearServers()
		{
			Servers.Clear();
		}

		#endregion
	}
}