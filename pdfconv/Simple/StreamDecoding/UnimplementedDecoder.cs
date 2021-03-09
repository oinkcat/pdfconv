using System;

namespace PdfConverter.Simple.StreamDecoding
{
    /// <summary>
    /// Stream decoder that was not implemented
    /// </summary>
    public class UnimplementedDecoder : IStreamDecoder
    {
        /// <summary>
        /// Name of unimplemented decoder
        /// </summary>
        public string DecoderName { get; }

        /// <summary>
        /// Decoding is not implemented
        /// </summary>
        /// <param name="inputData">Encoded data</param>
        /// <returns>Nothing</returns>
        public byte[] Decode(byte[] inputData)
        {
            throw new NotImplementedException($"Decoder {DecoderName} is not available");
        }

        public UnimplementedDecoder(string name) => DecoderName = name;
    }
}