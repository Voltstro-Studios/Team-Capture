// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
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
        internal ChatMessage(string message)
            : this()
        {
            Message = new CompressedNetworkString(message);
        }
        
        internal ChatMessage(ChatFlag flag, string message, string player = null)
            : this()
        {
            Flag = flag;
            if(!string.IsNullOrEmpty(message))
                Message = new CompressedNetworkString(message);
            Player = player;
        }

        internal ChatMessage(ChatFlag flag)
            : this()
        {
            Flag = flag;
        }

        /// <summary>
        ///     The <see cref="ChatFlag"/> for this message
        /// </summary>
        public ChatFlag Flag;

        /// <summary>
        ///     Their message
        /// </summary>
        public CompressedNetworkString Message;
        
        /// <summary>
        ///     The player or thing that sent this message
        /// </summary>
        public string Player;
    }

    [Preserve]
    internal static class ChatMessageNetwork
    {
        public static void WriteChatMessage(this NetworkWriter writer, ChatMessage message)
        {
            //Write flags first
            writer.WriteByte((byte)message.Flag);
            
            //If it a message type, write the message
            if(message.Flag is ChatFlag.Message or ChatFlag.Server or ChatFlag.GenericError)
                message.Message.Write(writer);
            if(message.Flag is ChatFlag.Connected or ChatFlag.Disconnected or ChatFlag.Message)
                writer.WriteString(message.Player);
        }

        public static ChatMessage ReadChatMessage(this NetworkReader reader)
        {
            //Read flag first
            ChatFlag flag = (ChatFlag) reader.ReadByte();
            ChatMessage message = new(flag);

            if (flag is ChatFlag.Message or ChatFlag.Server or ChatFlag.GenericError)
                message.Message = TCNetworkManager.IsServer
                    ? CompressedNetworkString.Read(reader, true)
                    : CompressedNetworkString.Read(reader);
            if (flag is ChatFlag.Connected or ChatFlag.Disconnected or ChatFlag.Message)
                message.Player = reader.ReadString();
            
            return message;
        }
    }
}