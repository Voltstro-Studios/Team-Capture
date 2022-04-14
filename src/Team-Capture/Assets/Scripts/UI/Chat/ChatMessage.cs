﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Core.Compression;
using Team_Capture.Core.Networking;
using UnityEngine.Scripting;

namespace Team_Capture.UI.Chat
{
    /// <summary>
    ///     Chat network message
    /// </summary>
    public struct ChatMessage : NetworkMessage
    {
        //TODO: We should have like 'metadata' or 'flags' to include stuff like if a message is a connect/disconnect related
        internal ChatMessage(string message)
            : this()
        {
            Message = new CompressedNetworkString(message);
        }

        /// <summary>
        ///     The player or thing that sent this message
        /// </summary>
        public string Player;

        /// <summary>
        ///     Their message
        /// </summary>
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
            ChatMessage message = new()
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