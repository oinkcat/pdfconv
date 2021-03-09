using System;
using System.Collections.Generic;

namespace PdfConverter.Simple.StreamDecoding
{
    /// <summary>
    /// Decodes data encoded as Base-85
    /// </summary>
    /// <remarks>Not fully implemented yet</remarks>
    public class ASCII85Decoder : IStreamDecoder
    {
        private readonly int[] multipliers = { 52200625, 614125, 7225, 85, 1 };

        private const char FourZerosChar = 'z';

        /// <summary>
        /// Decode Base-85 encoded data
        /// </summary>
        /// <param name="inputData">Encoded data bytes</param>
        /// <returns>Decoded data bytes</returns>
        public byte[] Decode(byte[] inputData)
        {
            var decodedData = new List<byte>();

            uint i32Number = 0;
            int digitIdx = 0;

            foreach(byte ib in inputData)
            {
                if(Char.IsWhiteSpace((char)ib)) { continue; }
                
                if((char)ib == FourZerosChar)
                {
                	for(int i = 0; i < 4; i++)
                	{
                		decodedData.Add(0);
                	}
                	continue;
                }
                
                byte inByte = (byte)(ib - 33);
                i32Number += (uint)(inByte * multipliers[digitIdx++]);

                if(digitIdx == 5)
                {
                    for(int i = 3; i >= 0; i--)
                    {
                        int shift = 8 * i;
                        byte ob = (byte)((i32Number & (0xff << shift)) >> shift);
                        decodedData.Add(ob);
                    }

                    digitIdx = 0;
                    i32Number = 0;
                }
            }

            return decodedData.ToArray();
        }
    }
}