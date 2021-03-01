using System;
using System.IO;
using System.IO.Compression;

namespace PdfConverter.Simple.StreamDecoding
{
    /// <summary>
    /// Decodes compressed data using Flate algorithm
    /// </summary>
    public class FlateDecoder : IStreamDecoder
    {
        /// <summary>
        /// Decode Flate compressed data
        /// </summary>
        /// <param name="inputData">Compresed data bytes</param>
        /// <returns>Decompressed data bytes</returns>
        public byte[] Decode(byte[] inputData)
        {
            using var compressedStream = new MemoryStream(inputData[2..]);

            var decoder = new DeflateStream(compressedStream, CompressionMode.Decompress);
            var decompressedStream = new MemoryStream();
            decoder.CopyTo(decompressedStream);
            decompressedStream.Position = 0;

            return decompressedStream.GetBuffer();
        }
    }
}