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
        [ConVar("sv_chat_characters", "The maximum amount of characters in a single chat message")]
        public static int MaxCharacters = 70;
        
        /// <summary>
        ///     When we get a chat message from a client
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="message"></param>
        internal static void ReceivedChatMessage(NetworkConnection conn, ChatMessage message)
        {
            if (!CheckMessageLenght(message.Message))
            {
                conn.Send(new ChatMessage($"Sorry, but your message is greater than {MaxCharacters} in lenght!"), Channels.Unreliable);
                return;
            }
            
            message.Player = TCNetworkManager.Authenticator.GetAccount(conn.connectionId).UserName;
            SendChatMessage(message);
        }

        /// <summary>
        ///     Sends a <see cref="ChatMessage" /> to all clients
        /// </summary>
        /// <param name="message"></param>
        public static void SendChatMessage(ChatMessage message)
        {
            NetworkServer.SendToAll(message, Channels.Unreliable, true);
            Logger.Info($"Chat: {message.Player}: {message.Message.String}");
        }

        /// <summary>
        ///     Sends a <see cref="ChatMessage" /> to all clients
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

        /// <summary>
        ///     Checks to make sure a message is within lenght
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns true if it is all good</returns>
        public static bool CheckMessageLenght(string message)
        {
            if (message.Length > MaxCharacters)
                return false;

            return true;
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