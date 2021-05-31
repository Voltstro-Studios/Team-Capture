// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using Mirror;
using Team_Capture.Console;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///     Provides a way to get ping of clients
	/// </summary>
	public static class PingManager
	{
		[ConVar("sv_pingfrequency", "How often will the server ping the clients")]
		private static readonly float PingFrequency = 2.0f;

		private static float lastPingTime;

		private static Dictionary<int, ExponentialMovingAverage> clientsPing;

		#region Server

		/// <summary>
		///     Gets a client's ping
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static double GetClientPing(int connectionId)
		{
			if (!clientsPing.ContainsKey(connectionId))
				return 0;

			return clientsPing[connectionId].Value;
		}

		/// <summary>
		///     Sets up the server side of the <see cref="PingManager" />
		/// </summary>
		internal static void ServerSetup()
		{
			lastPingTime = Time.time - 1;
			clientsPing = new Dictionary<int, ExponentialMovingAverage>();
			NetworkServer.RegisterHandler<PingClientMessage>(OnReceiveClientPingMessage);
		}

		/// <summary>
		///     Shutdown the server side of the <see cref="PingManager" />
		/// </summary>
		internal static void ServerShutdown()
		{
			clientsPing.Clear();
			NetworkServer.UnregisterHandler<PingClientMessage>();
		}

		/// <summary>
		///     Call this every frame
		/// </summary>
		internal static void ServerPingUpdate()
		{
			if (Time.time - lastPingTime >= PingFrequency)
			{
				PingClients();
				lastPingTime = Time.time;
			}
		}

		/// <summary>
		///     Pings all clients connected to the server
		/// </summary>
		internal static void PingClients()
		{
			NetworkServer.SendToAll(new PingServerMessage());
		}

		/// <summary>
		///     Pings a client
		/// </summary>
		/// <param name="conn"></param>
		internal static void PingClient(NetworkConnection conn)
		{
			conn.Send(new PingServerMessage());
		}

		private static void OnReceiveClientPingMessage(NetworkConnection conn, PingClientMessage message)
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

		/// <summary>
		///     Sets up the client side of the <see cref="PingManager" />
		/// </summary>
		internal static void ClientSetup()
		{
			NetworkClient.RegisterHandler<PingServerMessage>(OnReceiveServerPingMessage);
		}

		/// <summary>
		///     Shutdown the client side of the <see cref="PingManager" />
		/// </summary>
		internal static void ClientShutdown()
		{
			NetworkClient.UnregisterHandler<PingServerMessage>();
		}

		private static void OnReceiveServerPingMessage(PingServerMessage message)
		{
			NetworkClient.connection.Send(new PingClientMessage
			{
				ClientTime = NetworkTime.time
			});
		}

		#endregion

		internal struct PingClientMessage : NetworkMessage
		{
			/// <summary>
			///		The current time of the client
			/// </summary>
			public double ClientTime;
		}

		internal struct PingServerMessage : NetworkMessage
		{
		}
	}
}