using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Mirror.Runtime.Transport.LiteNetLib4Mirror
{
	public static class LiteNetLib4MirrorServer
	{
		public static NetPeer[] Peers;
		
		public static string Code { get; internal set; }
		internal static string DisconnectMessage = null;
		private static readonly NetDataWriter Writer = new NetDataWriter();
		private static string lastMessage;
		private static int maxId;

		public static int GetPing(int id)
		{
			return Peers[id].Ping;
		}

		internal static bool IsActive()
		{
			return LiteNetLib4MirrorCore.State == LiteNetLib4MirrorCore.States.Server;
		}

		internal static void StartServer(string code)
		{
			try
			{
				Code = code;
				EventBasedNetListener listener = new EventBasedNetListener();
				LiteNetLib4MirrorCore.Host = new NetManager(listener);
				listener.ConnectionRequestEvent += OnConnectionRequest;
				listener.PeerDisconnectedEvent += OnPeerDisconnected;
				listener.NetworkErrorEvent += OnNetworkError;
				listener.NetworkReceiveEvent += OnNetworkReceive;
				listener.PeerConnectedEvent += OnPeerConnected;

				LiteNetLib4MirrorCore.SetOptions(true);
				if (LiteNetLib4MirrorTransport.Singleton.useUpnP)
				{
					LiteNetLib4MirrorUtils.ForwardPort();
				}
#if DISABLE_IPV6
				LiteNetLib4MirrorCore.Host.Start(LiteNetLib4MirrorUtils.Parse(LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress), IPAddress.IPv6None, LiteNetLib4MirrorTransport.Singleton.port);
#else
				LiteNetLib4MirrorCore.Host.Start(LiteNetLib4MirrorUtils.Parse(LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress), LiteNetLib4MirrorUtils.Parse(LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress), LiteNetLib4MirrorTransport.Singleton.port);
#endif
				Peers = new NetPeer[LiteNetLib4MirrorTransport.Singleton.maxConnections * 2];
				LiteNetLib4MirrorTransport.Polling = true;
				LiteNetLib4MirrorCore.State = LiteNetLib4MirrorCore.States.Server;
			}
			catch (Exception ex)
			{
				LiteNetLib4MirrorCore.State = LiteNetLib4MirrorCore.States.Idle;
				Debug.LogException(ex);
			}
		}

		private static void OnPeerConnected(NetPeer peer)
		{
			if (peer.Id + 1 > Peers.Length)
			{
				Array.Resize(ref Peers, Peers.Length * 2);
			}

			Peers[peer.Id + 1] = peer;
			if (peer.Id + 1 > maxId)
				maxId = peer.Id + 1;
			LiteNetLib4MirrorTransport.Singleton.OnServerConnected.Invoke(peer.Id + 1);
		}

		private static void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
		{
			LiteNetLib4MirrorTransport.Singleton.OnServerDataReceived.Invoke(peer.Id + 1, reader.GetRemainingBytesSegment(), -1);
			reader.Recycle();
		}

		private static void OnNetworkError(IPEndPoint endpoint, SocketError socketError)
		{
			LiteNetLib4MirrorCore.LastError = socketError;
			for (int i = 0; i < maxId; i++)
			{
				NetPeer peer = Peers[i];
				if (peer != null && peer.EndPoint.Equals(endpoint))
				{
					LiteNetLib4MirrorTransport.Singleton.OnServerError.Invoke(peer.Id + 1, new SocketException((int)socketError));
					LiteNetLib4MirrorTransport.Singleton.onServerSocketError.Invoke(peer.Id + 1, socketError);
					return;
				}
			}
		}

		private static void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			LiteNetLib4MirrorCore.LastDisconnectError = disconnectInfo.SocketErrorCode;
			LiteNetLib4MirrorCore.LastDisconnectReason = disconnectInfo.Reason;
			LiteNetLib4MirrorTransport.Singleton.OnServerDisconnected.Invoke(peer.Id + 1);
		}

		private static void OnConnectionRequest(ConnectionRequest request)
		{
			try
			{
				LiteNetLib4MirrorTransport.Singleton.ProcessConnectionRequest(request);
			}
			catch (Exception ex)
			{
				Debug.LogError("Malformed join request! Rejecting... Error:" + ex.Message + "\n" + ex.StackTrace);
				request.Reject();
			}
		}

		internal static bool Send(int connectionId, DeliveryMethod method, byte[] data, int start, int length, byte channelNumber)
		{
			try
			{
				Peers[connectionId].Send(data, start, length, channelNumber, method);
				return true;
			}
			catch
			{
				return false;
			}
		}

		internal static bool Disconnect(int connectionId)
		{
			try
			{
				if (DisconnectMessage == null)
				{
					Peers[connectionId].Disconnect();
				}
				else
				{
					Peers[connectionId].Disconnect(LiteNetLib4MirrorUtils.ReusePut(Writer, DisconnectMessage, ref lastMessage));
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		internal static string GetClientAddress(int connectionId)
		{
			return Peers[connectionId].EndPoint.Address.ToString();
		}
	}
}