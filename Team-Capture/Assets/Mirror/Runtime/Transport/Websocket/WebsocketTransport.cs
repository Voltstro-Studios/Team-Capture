using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Mirror.Websocket
{
    public class WebsocketTransport : Transport
    {
        public string CertificatePassword;
        public string CertificatePath;

        protected Client client = new Client();

        [Tooltip("Nagle Algorithm can be disabled by enabling NoDelay")]
        public bool NoDelay = true;

        public int port = 7778;

        public bool Secure;
        protected Server server = new Server();

        public WebsocketTransport()
        {
            // dispatch the events from the server
            server.Connected += connectionId => OnServerConnected.Invoke(connectionId);
            server.Disconnected += connectionId => OnServerDisconnected.Invoke(connectionId);
            server.ReceivedData += (connectionId, data) =>
                OnServerDataReceived.Invoke(connectionId, data, Channels.DefaultReliable);
            server.ReceivedError += (connectionId, error) => OnServerError.Invoke(connectionId, error);

            // dispatch events from the client
            client.Connected += () => OnClientConnected.Invoke();
            client.Disconnected += () => OnClientDisconnected.Invoke();
            client.ReceivedData += data => OnClientDataReceived.Invoke(data, Channels.DefaultReliable);
            client.ReceivedError += error => OnClientError.Invoke(error);

            // configure
            client.NoDelay = NoDelay;
            server.NoDelay = NoDelay;

            Debug.Log("Websocket transport initialized!");
        }

        public override bool Available()
        {
            // WebSockets should be available on all platforms, including WebGL (automatically) using our included JSLIB code
            return true;
        }

        // client
        public override bool ClientConnected()
        {
            return client.IsConnected;
        }

        public override void ClientConnect(string host)
        {
            if (Secure)
                client.Connect(new Uri($"wss://{host}:{port}"));
            else
                client.Connect(new Uri($"ws://{host}:{port}"));
        }

        public override bool ClientSend(int channelId, ArraySegment<byte> segment)
        {
            client.Send(segment);
            return true;
        }

        public override void ClientDisconnect()
        {
            client.Disconnect();
        }

        // server
        public override bool ServerActive()
        {
            return server.Active;
        }

        public override void ServerStart()
        {
            server._secure = Secure;
            if (Secure)
            {
                server._secure = Secure;
                server._sslConfig = new Server.SslConfiguration
                {
                    Certificate = new X509Certificate2(
                        Path.Combine(Application.dataPath, CertificatePath),
                        CertificatePassword),
                    ClientCertificateRequired = false,
                    CheckCertificateRevocation = false,
                    EnabledSslProtocols = SslProtocols.Default
                };
            }

            _ = server.Listen(port);
        }

        public override bool ServerSend(List<int> connectionIds, int channelId, ArraySegment<byte> segment)
        {
            // send to all
            foreach (int connectionId in connectionIds)
                server.Send(connectionId, segment);
            return true;
        }

        public override bool ServerDisconnect(int connectionId)
        {
            return server.Disconnect(connectionId);
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            return server.GetClientAddress(connectionId);
        }

        public override void ServerStop()
        {
            server.Stop();
        }

        // common
        public override void Shutdown()
        {
            client.Disconnect();
            server.Stop();
        }

        public override int GetMaxPacketSize(int channelId)
        {
            // Telepathy's limit is Array.Length, which is int
            return int.MaxValue;
        }

        public override string ToString()
        {
            if (client.Connecting || client.IsConnected) return client.ToString();
            if (server.Active) return server.ToString();
            return "";
        }
    }
}