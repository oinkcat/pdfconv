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
        private const string BfCharSectionStart = "beginbfchar";
        private const string BfCharSectionEnd = "endbfchar";

        private const string BfRangeSectionStart = "beginbfrange";
        private const string BfRangeSectionEnd = "endbfrange";

        private Dictionary<string, char> conversionTable;

        private int cidSize;

        /// <summary>
        /// Decode string using nicode conversion table
        /// </summary>
        /// <param name="input">String to decode</param>
        /// <returns>Decoded unicode string</returns>
        public override string DecodeString(string input)
        {
            var buffer = new StringBuilder();

            for(int i = 0; i < input.Length; i += cidSize)
            {
                string hexChar = input.Substring(i, cidSize);

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
            var lineEnumerator = objContents.GetEnumerator();

            while(lineEnumerator.MoveNext())
            {
                var lineTokens = tokenizer.Tokenize(lineEnumerator.Current).ToList();
                if(lineTokens.Count == 0) { continue; }
                
                var lastTokenValue = lineTokens.Last().Value as string;

                if(lastTokenValue == BfCharSectionStart)
                {
                    ParseBfCharSection(lineEnumerator);
                }
                else if(lastTokenValue == BfRangeSectionStart)
                {
                    ParseBfRangeSection(lineEnumerator);
                }
            }
        }

        private void ParseBfCharSection(IEnumerator<string> lineEnumerator)
        {
            bool hasMoreData = true;
            var tokenizer = new ContentTokenizer();

            while(hasMoreData)
            {
                lineEnumerator.MoveNext();

                var lineTokens = tokenizer.Tokenize(lineEnumerator.Current).ToList();

                if((lineTokens.Count == 2) &&
                   (lineTokens[0].Type == TokenType.HexString))
                {
                    string fromCharHex = lineTokens[0].Value as string;
                    string toCharHex = lineTokens[1].Value as string;
                    int toCharCode = int.Parse(toCharHex, NumberStyles.HexNumber);
                    conversionTable.Add(fromCharHex, (char)toCharCode);
                }
                else if(lineTokens.Count == 1 && lineTokens[0].Type == TokenType.Id)
                {
                    string keyword = lineTokens[0].Value as string;
                    hasMoreData = !keyword.Equals(BfCharSectionEnd);
                }
            }
        }

        private void ParseBfRangeSection(IEnumerator<string> lineEnumerator)
        {
            bool hasMoreData = true;
            var tokenizer = new ContentTokenizer();

            while(hasMoreData)
            {
                lineEnumerator.MoveNext();

                var lineTokens = tokenizer.Tokenize(lineEnumerator.Current).ToList();

                if((lineTokens.Count == 3) &&
                   (lineTokens[0].Type == TokenType.HexString))
                {
                    string rangeStartHex = lineTokens[0].Value as string;
                    string rangeEndHex = lineTokens[1].Value as string;
                    string mappedCharHex = lineTokens[2].Value as string;

                    if(rangeStartHex.Length > cidSize)
                    {
                        cidSize = rangeStartHex.Length;
                    }

                    int rangeStart = int.Parse(rangeStartHex, NumberStyles.HexNumber);
                    int rangeEnd = int.Parse(rangeEndHex, NumberStyles.HexNumber);
                    int mapped = int.Parse(mappedCharHex, NumberStyles.HexNumber);

                    for(int i = rangeStart; i <= rangeEnd; i++)
                    {
                        string mapFromHex = i.ToString($"x{cidSize}").ToUpper();
                        conversionTable.Add(mapFromHex, (char)mapped);
                        mapped++;
                    }
                }
                else if(lineTokens.Count == 1 && lineTokens[0].Type == TokenType.Id)
                {
                    string keyword = lineTokens[0].Value as string;
                    hasMoreData = !keyword.Equals(BfRangeSectionEnd);
                }
            }
        }

        public PdfUnicodeFont(PdfDocument document, PdfObject fontObj)
        {
            cidSize = 2;
            this.document = document;
            conversionTable = new Dictionary<string, char>();
            RawObject = fontObj;

            PopulateBasicInfo();
            PopulateConversionTable();
        }
    }
}