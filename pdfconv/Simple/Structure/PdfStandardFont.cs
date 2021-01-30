using System.Text;
using System.Globalization;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// PDF font with specified encoding
    /// </summary>
    public class PdfStandardFont : PdfFont
    {
        /// <summary>
        /// Decode string with current encoding
        /// </summary>
        /// <param name="input">Encoded hex string</param>
        /// <returns>Decoded string</returns>
        public override string DecodeString(string input)
        {
            var buffer = new StringBuilder();

            // Not yet implemented
            
            return buffer.ToString();
        }

        public PdfStandardFont(PdfDocument document, PdfObject fontObj)
        {
            this.document = document;
            RawObject = fontObj;
        }
    }
}