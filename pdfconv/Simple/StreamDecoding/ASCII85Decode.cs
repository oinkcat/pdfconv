using System;
using System.Collections.Generic;

namespace PdfConverter.Simple.StreamDecoding
{
    /// <summary>
    /// Decodes data encoded as Base-85
    /// </summary>
    /// <remarks>Not fully implemented yet</remarks>
    public class ASCII85Decode : IStreamDecoder
    {
        private readonly int[] multipliers = { 52200625, 614125, 7225, 85, 1 };

        /// <summary>
        /// Decode Base-85 encoded data
        /// </summary>
        /// <param name="inputData">Encoded data bytes</param>
        /// <returns>Decoded data bytes</returns>
        public byte[] Decode(byte[] inputData)
        {
            var decodedData = new List<byte>();

            uint i32Number = 0;
            int idx = 0;
            int digitIdx = 0;

            while(idx < inputData.Length)
            {
                byte inByte = (byte)(inputData[idx++] - 33);
                if(Char.IsWhiteSpace((char)inByte)) { continue; }

                i32Number += (uint)(inByte * multipliers[digitIdx++]);

                if(digitIdx == 4)
                {
                    for(int i = 3; i >= 0; i--)
                    {
                        int shift = 8 * i;
                        byte b = (byte)((i32Number & (0xff << shift)) >> shift);
                        decodedData.Add(b);
                    }

                    digitIdx = 0;
                    i32Number = 0;
                }
            }

            return decodedData.ToArray();
        }
    }
}