using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

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
            using var compressedStream = new MemoryStream(inputData);

            var inflateStream = new InflaterInputStream(compressedStream);
            var decompressedStream = new MemoryStream();
            inflateStream.CopyTo(decompressedStream);
            decompressedStream.Position = 0;

            return decompressedStream.GetBuffer();
        }
    }
}