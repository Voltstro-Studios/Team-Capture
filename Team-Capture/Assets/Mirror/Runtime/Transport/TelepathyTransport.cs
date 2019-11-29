// wraps Telepathy for use as HLAPI TransportLayer

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using Telepathy;
using UnityEngine;
using UnityEngine.Serialization;
using EventType = Telepathy.EventType;
using Logger = Telepathy.Logger;

namespace Mirror
{
    [HelpURL("https://github.com/vis2k/Telepathy/blob/master/README.md")]
    public class TelepathyTransport : Transport
    {
        protected Client client = new Client();

        [Tooltip(
            "Protect against allocation attacks by keeping the max message size small. Otherwise an attacker host might send multiple fake packets with 2GB headers, causing the connected clients to run out of memory after allocating multiple large packets.")]
        [FormerlySerializedAs("MaxMessageSize")]
        public int clientMaxMessageSize = 16 * 1024;

        [Tooltip("Nagle Algorithm can be disabled by enabling NoDelay")]
        public bool NoDelay = true;

        public ushort port = 7777;
        protected Server server = new Server();

        [Tooltip(
            "Protect against allocation attacks by keeping the max message size small. Otherwise an attacker might send multiple fake packets with 2GB headers, causing the server to run out of memory after allocating multiple large packets.")]
        [FormerlySerializedAs("MaxMessageSize")]
        public int serverMaxMessageSize = 16 * 1024;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use MaxMessageSizeFromClient or MaxMessageSizeFromServer instead.")]
        public int MaxMessageSize
        {
            get => serverMaxMessageSize;
            set => serverMaxMessageSize = clientMaxMessageSize = value;
        }

        private void Awake()
        {
            // tell Telepathy to use Unity's Debug.Log
            Logger.Log = Debug.Log;
            Logger.LogWarning = Debug.LogWarning;
            Logger.LogError = Debug.LogError;

            // configure
            client.NoDelay = NoDelay;
            client.MaxMessageSize = clientMaxMessageSize;
            server.NoDelay = NoDelay;
            server.MaxMessageSize = serverMaxMessageSize;

            Debug.Log("TelepathyTransport initialized!");
        }

        public override bool Available()
        {
            // C#'s built in TCP sockets run everywhere except on WebGL
            return Application.platform != RuntimePlatform.WebGLPlayer;
        }

        // client
        public override bool ClientConnected()
        {
            return client.Connected;
        }

        public override void ClientConnect(string address)
        {
            client.Connect(address, port);
        }

        public override bool ClientSend(int channelId, ArraySegment<byte> segment)
        {
            // telepathy doesn't support allocation-free sends yet.
            // previously we allocated in Mirror. now we do it here.
            byte[] data = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);
            return client.Send(data);
        }

        private bool ProcessClientMessage()
        {
            if (client.GetNextMessage(out Message message))
            {
                switch (message.eventType)
                {
                    case EventType.Connected:
                        OnClientConnected.Invoke();
                        break;
                    case EventType.Data:
                        OnClientDataReceived.Invoke(new ArraySegment<byte>(message.data), Channels.DefaultReliable);
                        break;
                    case EventType.Disconnected:
                        OnClientDisconnected.Invoke();
                        break;
                    default:
                        // TODO:  Telepathy does not report errors at all
                        // it just disconnects,  should be fixed
                        OnClientDisconnected.Invoke();
                        break;
                }

                return true;
            }

            return false;
        }

        public override void ClientDisconnect()
        {
            client.Disconnect();
        }

        // IMPORTANT: set script execution order to >1000 to call Transport's
        //            LateUpdate after all others. Fixes race condition where
        //            e.g. in uSurvival Transport would apply Cmds before
        //            ShoulderRotation.LateUpdate, resulting in projectile
        //            spawns at the point before shoulder rotation.
        public void LateUpdate()
        {
            // note: we need to check enabled in case we set it to false
            // when LateUpdate already started.
            // (https://github.com/vis2k/Mirror/pull/379)
            while (enabled && ProcessClientMessage())
            {
            }

            while (enabled && ProcessServerMessage())
            {
            }
        }

        // server
        public override bool ServerActive()
        {
            return server.Active;
        }

        public override void ServerStart()
        {
            server.Start(port);
        }

        public override bool ServerSend(List<int> connectionIds, int channelId, ArraySegment<byte> segment)
        {
            // telepathy doesn't support allocation-free sends yet.
            // previously we allocated in Mirror. now we do it here.
            byte[] data = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);

            // send to all
            bool result = true;
            foreach (int connectionId in connectionIds)
                result &= server.Send(connectionId, data);
            return result;
        }

        public bool ProcessServerMessage()
        {
            if (server.GetNextMessage(out Message message))
            {
                switch (message.eventType)
                {
                    case EventType.Connected:
                        OnServerConnected.Invoke(message.connectionId);
                        break;
                    case EventType.Data:
                        OnServerDataReceived.Invoke(message.connectionId, new ArraySegment<byte>(message.data),
                            Channels.DefaultReliable);
                        break;
                    case EventType.Disconnected:
                        OnServerDisconnected.Invoke(message.connectionId);
                        break;
                    default:
                        // TODO handle errors from Telepathy when telepathy can report errors
                        OnServerDisconnected.Invoke(message.connectionId);
                        break;
                }

                return true;
            }

            return false;
        }

        public override bool ServerDisconnect(int connectionId)
        {
            return server.Disconnect(connectionId);
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            try
            {
                return server.GetClientAddress(connectionId);
            }
            catch (SocketException)
            {
                // using server.listener.LocalEndpoint causes an Exception
                // in UWP + Unity 2019:
                //   Exception thrown at 0x00007FF9755DA388 in UWF.exe:
                //   Microsoft C++ exception: Il2CppExceptionWrapper at memory
                //   location 0x000000E15A0FCDD0. SocketException: An address
                //   incompatible with the requested protocol was used at
                //   System.Net.Sockets.Socket.get_LocalEndPoint ()
                // so let's at least catch it and recover
                return "unknown";
            }
        }

        public override void ServerStop()
        {
            server.Stop();
        }

        // common
        public override void Shutdown()
        {
            Debug.Log("TelepathyTransport Shutdown()");
            client.Disconnect();
            server.Stop();
        }

        public override int GetMaxPacketSize(int channelId)
        {
            return serverMaxMessageSize;
        }

        public override string ToString()
        {
            if (server.Active && server.listener != null)
                // printing server.listener.LocalEndpoint causes an Exception
                // in UWP + Unity 2019:
                //   Exception thrown at 0x00007FF9755DA388 in UWF.exe:
                //   Microsoft C++ exception: Il2CppExceptionWrapper at memory
                //   location 0x000000E15A0FCDD0. SocketException: An address
                //   incompatible with the requested protocol was used at
                //   System.Net.Sockets.Socket.get_LocalEndPoint ()
                // so let's use the regular port instead.
                return "Telepathy Server port: " + port;
            if (client.Connecting || client.Connected)
                return "Telepathy Client ip: " + client.client.Client.RemoteEndPoint;
            return "Telepathy (inactive/disconnected)";
        }
    }
}