using System;
using NUnit.Framework;
using Team_Capture.Core.Compression;
using Team_Capture.Core.Networking;

namespace Team_Capture.Editor.Tests
{
    public class CompressionTests
    {
        private const string ShortString = "Hello World!";
        private const string LongString = "As he crossed toward the pharmacy at the corner he involuntarily turned his head because of a burst of light that had ricocheted from his temple, and saw, with that quick smile with which we greet a rainbow or a rose, a blindingly white parallelogram of sky being unloaded from the van—a dresser with mirrors across which, as across a cinema screen, passed a flawlessly clear reflection of boughs sliding and swaying not arboreally, but with a human vacillation, produced by the nature of those who were carrying this sky, these boughs, this gliding façade.";

        [Test]
        public void ShortStringCompressionTest()
        {
            //Compress it
            Span<byte> compressed = Compression.CompressString(ShortString, out int length);
            Assert.AreEqual(ShortString.Length + 1, compressed.Length);

            string decompressed = Compression.DecompressString(compressed, length);
            Assert.AreEqual(ShortString, decompressed);
        }

        [Test]
        public void LongStringCompressionTest()
        {
            //Compress it
            Span<byte> compressed = Compression.CompressString(LongString, out int length);
            Assert.Less(compressed.Length, LongString.Length);

            string decompressed = Compression.DecompressString(compressed, length);
            Assert.AreEqual(LongString, decompressed);
        }
    }
}
