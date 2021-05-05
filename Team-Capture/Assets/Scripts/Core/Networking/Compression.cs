using System;
using System.Text;
using K4os.Compression.LZ4;

namespace Team_Capture.Core.Networking
{
    public static class Compression
    {
        private static readonly Encoding Encoder = Encoding.UTF8;
        
        public static Span<byte> CompressString(string @string, out int length)
        {
            ReadOnlySpan<byte> data = Encoder.GetBytes(@string);
            return Compress(data, out length);
        }

        public static Span<byte> Compress(ReadOnlySpan<byte> data, out int length)
        {
            length = data.Length;
            Span<byte> target = new byte[LZ4Codec.MaximumOutputSize(data.Length)];
            int compressedLength = LZ4Codec.Encode(data, target);
            Span<byte> compressed = target.Slice(0, compressedLength);
            return compressed;
        }

        public static string DecompressString(ReadOnlySpan<byte> data, int length)
        {
            return Encoder.GetString(Decompress(data, length).ToArray());
        }

        public static Span<byte> Decompress(ReadOnlySpan<byte> data, int length)
        {
            Span<byte> decompressed = new byte[length];
            LZ4Codec.Decode(data, decompressed);
            return decompressed;
        }
    }
}