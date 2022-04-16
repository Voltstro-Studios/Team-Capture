// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Console;
using Team_Capture.Logging;
using Team_Capture.UI.Chat;
using Team_Capture.UserManagement;

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
                conn.Send(new ChatMessage(ChatFlag.TooLong), Channels.Unreliable);
                return;
            }

            IUser user = TCNetworkManager.Authenticator.GetAccount(conn.connectionId);
            if (user == null)
            {
                Logger.Warn("Got a message from the connection {ID} that has no auth user data!", conn.connectionId);
                return;
            }
            
            message.Player = user.UserName;
            message.Flag = ChatFlag.Message;
            SendChatMessage(message);
        }

        /// <summary>
        ///     Sends a <see cref="ChatMessage" /> to all clients
        /// </summary>
        /// <param name="message"></param>
        public static void SendChatMessage(ChatMessage message)
        {
            NetworkServer.SendToAll(message, Channels.Unreliable, true);
            if (message.Flag is ChatFlag.Message or ChatFlag.GenericError or ChatFlag.Server)
            {
                Logger.Info(string.IsNullOrEmpty(message.Player)
                    ? $"Chat: {message.Message}"
                    : $"Chat: [{message.Player}] {message.Message}");
            }
        }

        /// <summary>
        ///     Sends a <see cref="ChatMessage" /> to all clients
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flag"></param>
        /// <param name="player"></param>
        public static void SendChatMessage(string message, ChatFlag flag, string player = null)
        {
            SendChatMessage(new ChatMessage(flag, message, player));
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
            SendChatMessage(message, ChatFlag.Server);
        }
    }
}