using System.Collections.Generic;
using Core.Networking.Messages;
using Mirror;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Core.Networking
{
	public static class ServerPing
	{
		private static Dictionary<int, ExponentialMovingAverage> clientsPing;

		private static float lastPingTime;

		private static float PingFrequency = 2.0f;

		#region Server

		internal static void ServerSetup()
		{
			lastPingTime = Time.time - 1;
			clientsPing = new Dictionary<int, ExponentialMovingAverage>();
			NetworkServer.RegisterHandler<ClientPongMessage>(OnReceiveClientPongMessage);
		}

		internal static void ServerShutdown()
		{
			clientsPing.Clear();
			NetworkServer.UnregisterHandler<ClientPongMessage>();
		}

		internal static void ServerPingUpdate()
		{
			if (Time.time - lastPingTime >= PingFrequency)
			{
				PingClients();
				lastPingTime = Time.time;
			}
		}

		/// <summary>
		///		Pings all clients connected to the server
		/// </summary>
		internal static void PingClients()
		{
			NetworkServer.SendToAll(new ServerPingMessage());
		}

		/// <summary>
		///		Pings a client
		/// </summary>
		/// <param name="conn"></param>
		internal static void PingClient(NetworkConnection conn)
		{
			conn.Send(new ServerPingMessage());
		}

		private static void OnReceiveClientPongMessage(NetworkConnection conn, ClientPongMessage message)
		{
			ExponentialMovingAverage rtt;
			if (clientsPing.ContainsKey(conn.connectionId))
			{
				rtt = clientsPing[conn.connectionId];
			}
			else
			{
				rtt = new ExponentialMovingAverage(NetworkTime.PingWindowSize);
				clientsPing.Add(conn.connectionId, rtt);
			}

			double clientRttValue = NetworkTime.time - message.ClientTime;
			rtt.Add(clientRttValue);
			Logger.Debug("Got client {@ClientConnectionId}'s rtt of {@ClientRtt}ms", conn.connectionId, rtt.Value);
		}

		#endregion

		#region Client

		internal static void ClientSetup()
		{
			NetworkClient.RegisterHandler<ServerPingMessage>(OnReceiveServerPing);
		}

		internal static void ClientShutdown()
		{
			NetworkClient.UnregisterHandler<ServerPingMessage>();
		}

		private static void OnReceiveServerPing(NetworkConnection conn, ServerPingMessage message)
		{
			conn.Send(new ClientPongMessage
			{
				ClientTime = NetworkTime.time
			});
		}

		#endregion
	}
}