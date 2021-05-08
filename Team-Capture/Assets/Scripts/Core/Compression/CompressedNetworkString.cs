using Mirror;

namespace Team_Capture.Core.Compression
{
    /// <summary>
    ///     A <see cref="string"/> designed to be sent over the network.
    ///     <para>
    ///         This can be very handy for <see cref="string"/>s that are quite long.
    ///         Shorter <see cref="string"/>s may actually be larger due to the overhead.
    ///     </para>
    ///     <para>Uses LZ4 compression.</para>
    /// </summary>
    public struct CompressedNetworkString
    {
        /// <summary>
        ///     Creates a new <see cref="CompressedNetworkString"/> instance
        /// </summary>
        /// <param name="string">The <see cref="string"/> to compress</param>
        public CompressedNetworkString(string @string)
        : this()
        {
            String = @string;
        }

        private byte[] compressedString;
        private string @string;
        
        /// <summary>
        ///     Gets the uncompressed <see cref="string"/>
        ///     or sets sets the compressed string
        /// </summary>
        public string String
        {
            get => @string;
            set
            {
                @string = value;
                compressedString = Compression.CompressString(value, out int _).ToArray();
            }
        }

        /// <summary>
        ///     Writes the <see cref="CompressedNetworkString"/>
        /// </summary>
        /// <param name="writer"><see cref="NetworkWriter"/> to write to</param>
        public void Write(NetworkWriter writer)
        {
            writer.WriteArray(compressedString);
            writer.WriteInt32(@string.Length);
        }

        /// <summary>
        ///     Reads the <see cref="CompressedNetworkString"/>
        /// </summary>
        /// <param name="reader"><see cref="NetworkReader"/> to read from</param>
        /// <returns></returns>
        public static CompressedNetworkString Read(NetworkReader reader)
        {
            return new CompressedNetworkString
            {
                @string = Compression.DecompressString(reader.ReadArray<byte>(), reader.ReadInt32())
            };
        }

        public override string ToString()
        {
            return @string;
        }
    }
}