// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.Logging;

namespace Team_Capture.Core.Compression
{
    /// <summary>
    ///     A <see cref="string" /> designed to be sent over the network.
    ///     <para>
    ///         This can be very handy for <see cref="string" />s that are quite long.
    ///         Shorter <see cref="string" />s may actually be larger due to the overhead.
    ///     </para>
    ///     <para>Uses LZ4 compression.</para>
    /// </summary>
    public struct CompressedNetworkString
    {
        /// <summary>
        ///     Creates a new <see cref="CompressedNetworkString" /> instance
        /// </summary>
        /// <param name="string">The <see cref="string" /> to compress</param>
        public CompressedNetworkString(string @string)
            : this()
        {
            String = @string;
        }

        private byte[] compressedString;
        private string @string;

        /// <summary>
        ///     Gets the uncompressed <see cref="string" />
        ///     or sets sets the compressed string
        /// </summary>
        public string String
        {
            get => @string;
            set
            {
                try
                {
                    compressedString = Compression.CompressString(value, out int _).ToArray();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "An error occured while compressing data!");
                    return;
                }

                @string = value;
            }
        }

        /// <summary>
        ///     Writes the <see cref="CompressedNetworkString" />
        /// </summary>
        /// <param name="writer"><see cref="NetworkWriter" /> to write to</param>
        public void Write(NetworkWriter writer)
        {
            writer.WriteArray(compressedString);
            writer.WriteInt(@string.Length);
        }

        /// <summary>
        ///     Reads the <see cref="CompressedNetworkString" />
        /// </summary>
        /// <param name="reader"><see cref="NetworkReader" /> to read from</param>
        /// <param name="reCompressString">
        ///     Will assign <see cref="CompressedNetworkString.compressedString" />. Use for security reasons,
        ///     such as if you want to send data to the server, then to all the clients, like a message.
        /// </param>
        /// <returns></returns>
        public static CompressedNetworkString Read(NetworkReader reader, bool reCompressString = false)
        {
            try
            {
                string rawString = Compression.DecompressString(reader.ReadArray<byte>(), reader.ReadInt());
                CompressedNetworkString compressedNetworkString;

                //You might want to do this in-case a client sends data, and that data goes to every client.
                //So in-case the client sending the data sends fucky data, we will probs catch it here.
                if (reCompressString)
                    compressedNetworkString = new CompressedNetworkString(rawString);
                else
                    compressedNetworkString = new CompressedNetworkString
                    {
                        @string = rawString
                    };

                return compressedNetworkString;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occured while decompressing data!");
                return new CompressedNetworkString
                {
                    String = null
                };
            }
        }

        public override string ToString()
        {
            return @string;
        }
        
        public static implicit operator string(CompressedNetworkString cachedLocalizedString) =>
            cachedLocalizedString.String;
    }
}