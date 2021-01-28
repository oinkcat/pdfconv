using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using PdfConverter.Simple.Parsing;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// PDF font that uses unicode conversion table
    /// </summary>
    public class PdfUnicodeFont : PdfFont
    {
        private Dictionary<string, char> conversionTable;

        /// <summary>
        /// Decode string using nicode conversion table
        /// </summary>
        /// <param name="input">String to decode</param>
        /// <returns>Decoded unicode string</returns>
        public override string DecodeString(string input)
        {
            var buffer = new StringBuilder();

            for(int i = 0; i < input.Length; i += 2)
            {
                string hexChar = input.Substring(i, 2);

                if(conversionTable.TryGetValue(hexChar, out char decodedChar))
                {
                    buffer.Append(decodedChar);
                }
                else
                {
                    buffer.Append('?');
                }
            }

            return buffer.ToString();
        }

        private void PopulateConversionTable()
        {
            var toUnicodeRef = RawObject.GetAttributeValue<PdfArray>("ToUnicode");
            var toUnicodeObj = document.ObjectRoot.GetObjectByRef(toUnicodeRef);

            toUnicodeObj.ConvertContentToText();

            ParseConversionTables(toUnicodeObj.TextContent);
        }

        // TODO: Parse beginbfrange - endbfrange
        private void ParseConversionTables(IList<string> objContents)
        {
            var tokenizer = new ContentTokenizer();
            bool readingBfChar = false;

            foreach(string contentLine in objContents)
            {
                var lineTokens = tokenizer.Tokenize(contentLine).ToList();
                if(lineTokens.Count == 0) { continue; }
                
                var lastTokenValue = lineTokens.Last().Value as string;

                if(readingBfChar)
                {
                    if(lastTokenValue == "endbfchar")
                    {
                        readingBfChar = false;
                    }
                    else if((lineTokens.Count == 2) &&
                            (lineTokens[0].Type == TokenType.HexString))
                    {
                        string fromCharHex = lineTokens[0].Value as string;
                        string toCharHex = lineTokens[1].Value as string;
                        int toCharCode = int.Parse(toCharHex, NumberStyles.HexNumber);
                        conversionTable.Add(fromCharHex, (char)toCharCode);
                    }
                }
                else
                {
                    if(lastTokenValue == "beginbfchar")
                    {
                        readingBfChar = true;
                    }
                }
            }
        }

        public PdfUnicodeFont(PdfDocument document, PdfObject fontObj)
        {
            this.document = document;
            conversionTable = new Dictionary<string, char>();
            RawObject = fontObj;

            PopulateBasicInfo();
            PopulateConversionTable();
        }
    }
}