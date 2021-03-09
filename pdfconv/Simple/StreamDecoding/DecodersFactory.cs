using System;
using System.Collections.Generic;

namespace PdfConverter.Simple.StreamDecoding
{
    /// <summary>
    /// Creates decoders for compression filters
    /// </summary>
    public class DecodersFactory
    {
        /// <summary>
        /// Single factory instance
        /// </summary>
        public static DecodersFactory Instance { get; }

        private Dictionary<string, IStreamDecoder> knownDecoders;

        /// <summary>
        /// Check if can get decoder for given encoding filter
        /// </summary>
        /// <param name="decodingFilterName">Encoding filter name</param>
        /// <returns>Can use decoder</returns>
        public bool HasDecoder(string decodingFilterName)
        {
            return knownDecoders.ContainsKey(decodingFilterName);
        }

        /// <summary>
        /// Get decoder for compression filter
        /// </summary>
        /// <param name="decodingFilterName">Encoding filter name</param>
        /// <returns>Decoder for given filter</returns>
        public IStreamDecoder GetDecoder(string decodingFilterName)
        {
            return HasDecoder(decodingFilterName)
                ? knownDecoders[decodingFilterName]
                : new UnimplementedDecoder(decodingFilterName);
        }

        private DecodersFactory()
        {
            knownDecoders = new Dictionary<string, IStreamDecoder> {
                ["FlateDecode"] = new FlateDecoder(),
                ["LZWDecode"] = new LzwDecoder(),
                ["ASCII85Decode"] = new ASCII85Decoder()
            };
        }

        static DecodersFactory()
        {
            Instance = new DecodersFactory();
        }
    }
}