using Mirror;
using Team_Capture.Console;
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
            message.Player = conn.connectionId.ToString();
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