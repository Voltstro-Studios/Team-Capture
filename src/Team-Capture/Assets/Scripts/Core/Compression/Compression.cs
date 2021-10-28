﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Text;
using JetBrains.Annotations;
using K4os.Compression.LZ4;
using Team_Capture.Logging;

namespace Team_Capture.Core.Compression
{
    /// <summary>
    ///     Provides the ability to compress shit with LZ4
    /// </summary>
    [PublicAPI]
    public static class Compression
    {
        private static readonly Encoding Encoder = Encoding.UTF8;

        /// <summary>
        ///     Encodes a <see cref="string" /> and compresses it
        /// </summary>
        /// <param name="string">The <see cref="string" /> to compress</param>
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
        ///     Compresses a <see cref="ReadOnlySpan{T}" /> of data
        /// </summary>
        /// <param name="data">The data to compress</param>
        /// <param name="length">The length of the uncompressed data</param>
        /// <returns></returns>
        public static Span<byte> Compress(ReadOnlySpan<byte> data, out int length)
        {
            length = data.Length;
            Span<byte> target = new byte[LZ4Codec.MaximumOutputSize(data.Length)];
            int compressedLength = LZ4Codec.Encode(data, target);
            var compressed = target.Slice(0, compressedLength);
            Logger.Debug("Compressed data from {UncompressedSize} to {CompressedSize}", length, compressed.Length);
            return compressed;
        }

        /// <summary>
        ///     Decompresses and decodes a <see cref="string" />
        /// </summary>
        /// <param name="data">The compressed string</param>
        /// <param name="length">The length of the uncompressed data</param>
        /// <returns></returns>
        public static string DecompressString(ReadOnlySpan<byte> data, int length)
        {
            return Encoder.GetString(Decompress(data, length).ToArray());
        }

        /// <summary>
        ///     Decompresses a <see cref="ReadOnlySpan{T}" />
        /// </summary>
        /// <param name="data">The data to decompress</param>
        /// <param name="length">The length of the uncompressed data</param>
        /// <returns></returns>
        public static Span<byte> Decompress(ReadOnlySpan<byte> data, int length)
        {
            Span<byte> decompressed = new byte[length];
            LZ4Codec.Decode(data, decompressed);
            Logger.Debug("Decompressed data from {DecompressedSize} {CompressedSize}", length, data.Length);
            return decompressed;
        }
    }
}