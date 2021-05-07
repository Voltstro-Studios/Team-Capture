using Mirror;

namespace Team_Capture.Core.Networking
{
    public struct CompressedNetworkString
    {
        public CompressedNetworkString(string @string)
        : this()
        {
            String = @string;
        }

        private byte[] compressedString;
        private string @string;
        public string String
        {
            get => @string;
            set
            {
                @string = value;
                compressedString = Compression.CompressString(value, out int _).ToArray();
            }
        }

        public void Write(NetworkWriter writer)
        {
            writer.WriteArray(compressedString);
            writer.WriteInt32(@string.Length);
        }

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