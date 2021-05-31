// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Console;
using Team_Capture.Core.Compression;
using Team_Capture.Logging;
using Team_Capture.UI.Chat;

namespace Team_Capture.Core.Networking
{
    /// <summary>
    ///     Responsible for the server's chat
    /// </summary>
    public static class ServerChat
    {
        /// <summary>
        ///     When we get a chat message from a client
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="message"></param>
        internal static void ReceivedChatMessage(NetworkConnection conn, ChatMessage message)
        {
            message.Player = TCNetworkManager.Authenticator.GetAccount(conn.connectionId).AccountName;
            SendChatMessage(message);
        }

        /// <summary>
        ///     Sends a <see cref="ChatMessage"/> to all clients
        /// </summary>
        /// <param name="message"></param>
        public static void SendChatMessage(ChatMessage message)
        {
            NetworkServer.SendToAll(message, Channels.Unreliable, true);
            Logger.Info($"Chat: {message.Player}: {message.Message.String}");
        }

        /// <summary>
        ///     Sends a <see cref="ChatMessage"/> to all clients
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        public static void SendChatMessage(string name, string message)
        {
            NetworkServer.SendToAll(new ChatMessage
            {
                Player = name,
                Message = new CompressedNetworkString(message)
            });
        }
        
        [ConCommand("send", "Sends a message to the chat", CommandRunPermission.ServerOnly)]
        internal static void SendMessageCommand(string[] args)
        {
            string message = string.Join(" ", args);
            SendChatMessage(new ChatMessage(message)
            {
                Player = "Server"
            });
        }
    }
}