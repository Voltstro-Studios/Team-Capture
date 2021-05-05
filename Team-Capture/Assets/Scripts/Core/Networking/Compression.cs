using System;
using System.Text;
using JetBrains.Annotations;
using K4os.Compression.LZ4;

namespace Team_Capture.Core.Networking
{
    /// <summary>
    ///     Provides the ability to compress shit with LZ4
    /// </summary>
    [PublicAPI]
    public static class Compression
    {
        private static readonly Encoding Encoder = Encoding.UTF8;
        
        /// <summary>
        ///     Encodes a <see cref="string"/> and compresses it
        /// </summary>
        /// <param name="string">The <see cref="string"/> to compress</param>
        /// <param name="length">The length of the uncompressed data</param>
        /// <returns></returns>
        public static Span<byte> CompressString([NotNull] string @string, out int length)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));
            
            ReadOnlySpan<byte> data = Encoder.GetBytes(@string);
            return Compress(data, out length);
        }

        /// <summary>
        ///     Compresses a <see cref="ReadOnlySpan{T}"/> of data
        /// </summary>
        /// <param name="data">The data to compress</param>
        /// <param name="length">The length of the uncompressed data</param>
        /// <returns></returns>
        public static Span<byte> Compress(ReadOnlySpan<byte> data, out int length)
        {
            length = data.Length;
            Span<byte> target = new byte[LZ4Codec.MaximumOutputSize(data.Length)];
            int compressedLength = LZ4Codec.Encode(data, target);
            Span<byte> compressed = target.Slice(0, compressedLength);
            return compressed;
        }

        /// <summary>
        ///     Decompresses and decodes a <see cref="string"/>
        /// </summary>
        /// <param name="data">The compressed string</param>
        /// <param name="length">The length of the uncompressed data</param>
        /// <returns></returns>
        public static string DecompressString(ReadOnlySpan<byte> data, int length)
        {
            return Encoder.GetString(Decompress(data, length).ToArray());
        }

        /// <summary>
        ///     Decompresses a <see cref="ReadOnlySpan{T}"/>
        /// </summary>
        /// <param name="data">The data to decompress</param>
        /// <param name="length">The length of the uncompressed data</param>
        /// <returns></returns>
        public static Span<byte> Decompress(ReadOnlySpan<byte> data, int length)
        {
            Span<byte> decompressed = new byte[length];
            LZ4Codec.Decode(data, decompressed);
            return decompressed;
        }
    }
}