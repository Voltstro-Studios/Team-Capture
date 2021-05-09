using Mirror;
using Team_Capture.Core.Compression;
using Team_Capture.Core.Networking;
using UnityEngine.Scripting;

namespace Team_Capture.UI.Chat
{
    public struct ChatMessage : NetworkMessage
    {
        internal ChatMessage(string message)
        : this()
        {
            Message = new CompressedNetworkString(message);
        }
        
        public string Player;
        
        public CompressedNetworkString Message;
    }

    [Preserve]
    internal static class ChatMessageNetwork
    {
        public static void WriteChatMessage(this NetworkWriter writer, ChatMessage message)
        {
            writer.WriteString(message.Player);
            message.Message.Write(writer);
        }

        public static ChatMessage ReadChatMessage(this NetworkReader reader)
        {
            ChatMessage message = new ChatMessage
            {
                Player = reader.ReadString(),
                Message = TCNetworkManager.IsServer
                    ? CompressedNetworkString.Read(reader, true)
                    : CompressedNetworkString.Read(reader)
            };

            return message;
        }
    }
}