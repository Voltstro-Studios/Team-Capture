﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Mirror;
using Team_Capture.Console;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
    /// <summary>
    ///     Provides a way to get ping of clients
    /// </summary>
    public static class PingManager
    {
        [ConVar("sv_pingfrequency", "How often will the server ping the clients")]
        private static readonly double PingFrequency = 2.0f;

        private static double lastPingTime;

        private static Dictionary<int, ExponentialMovingAverage> clientsPing;

        internal struct PingClientMessage : NetworkMessage
        {
            /// <summary>
            ///     The current time of the client
            /// </summary>
            public double ClientTime;
        }

        internal struct PingServerMessage : NetworkMessage
        {
        }

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
            lastPingTime = NetworkTime.time - 1;
            clientsPing = new Dictionary<int, ExponentialMovingAverage>();
            NetworkServer.RegisterHandler<PingClientMessage>(OnReceiveClientPingMessage, false);
        }

        /// <summary>
        ///     Shutdown the server side of the <see cref="PingManager" />
        /// </summary>
        internal static void ServerShutdown()
        {
            clientsPing?.Clear();
            NetworkServer.UnregisterHandler<PingClientMessage>();
        }

        /// <summary>
        ///     Call this every frame
        /// </summary>
        internal static void ServerPingUpdate()
        {
            if (NetworkTime.time - lastPingTime >= PingFrequency)
            {
                PingClients();
                lastPingTime = NetworkTime.time;
            }
        }

        /// <summary>
        ///     Pings all clients connected to the server
        /// </summary>
        internal static void PingClients()
        {
            NetworkServer.SendToAll(new PingServerMessage(), sendToReadyOnly: true);
        }

        /// <summary>
        ///     Pings a client
        /// </summary>
        /// <param name="conn"></param>
        internal static void PingClient(NetworkConnection conn)
        {
            conn.Send(new PingServerMessage());
        }

        internal static ExponentialMovingAverage AddClient(NetworkConnection conn)
        {
            ExponentialMovingAverage rtt = new(NetworkTime.PingWindowSize);
            try
            {
                rtt.Add(0);
                clientsPing.Add(conn.connectionId, rtt);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error adding client to client pings!");
            }
            return rtt;
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
                rtt = AddClient(conn);
            }

            double clientRttValue = NetworkTime.time - message.ClientTime;
            rtt.Add(clientRttValue);
            Logger.Debug("Got client {ClientConnectionId}'s rtt of {ClientRtt}ms", conn.connectionId, rtt.Value);
        }

        #endregion

        #region Client

        /// <summary>
        ///     Sets up the client side of the <see cref="PingManager" />
        /// </summary>
        internal static void ClientSetup()
        {
            NetworkClient.RegisterHandler<PingServerMessage>(OnReceiveServerPingMessage, false);
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
    }
}