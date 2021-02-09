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
        private const string CodeSpaceSectionEnd = "endcodespacerange";
        private const string BfCharSectionEnd = "endbfchar";
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

        private void ParseConversionTables(IList<string> objContents)
        {
            var parser = new ObjectParser(TokenStreamer.CreateFromList(objContents));

            PdfAtom command;
            var paramList = new List<IPdfTerm>();

            do
            {
                command = parser.ReadNextCommand(paramList);

                switch(command?.AsString())
                {
                    case CodeSpaceSectionEnd:
                        CheckSidSize(paramList);
                        break;
                    case BfCharSectionEnd:
                        FillSidToCharMap(paramList);
                        break;
                    case BfRangeSectionEnd:
                        FillSidToCharMapFromRanges(paramList);
                        break;
                }

                paramList.Clear();
            }
            while(command != null);
        }

        private void CheckSidSize(List<IPdfTerm> elements)
        {
            foreach(var cid in elements)
            {
                int cidElemSize = (cid as PdfAtom).AsString().Length;
                if(cidElemSize > cidSize)
                {
                    cidSize = cidElemSize;
                }
            }
        }

        private void FillSidToCharMap(IList<IPdfTerm> elements)
        {
            for(int i = 0; i < elements.Count; i += 2)
            {
                string fromCharHex = (elements[i] as PdfAtom).AsString();
                string toCharHex = (elements[i + 1] as PdfAtom).AsString();
                int toCharCode = int.Parse(toCharHex, NumberStyles.HexNumber);
                conversionTable.Add(fromCharHex, (char)toCharCode);
            }
        }

        private void FillSidToCharMapFromRanges(IList<IPdfTerm> elements)
        {
            for(int i = 0; i < elements.Count; i += 3)
            {
                string rangeStartHex = (elements[i] as PdfAtom).AsString();
                string rangeEndHex = (elements[i + 1] as PdfAtom).AsString();
                string mappedCharHex = (elements[i + 2] as PdfAtom).AsString();

                int rangeStart = int.Parse(rangeStartHex, NumberStyles.HexNumber);
                int rangeEnd = int.Parse(rangeEndHex, NumberStyles.HexNumber);
                int mapped = int.Parse(mappedCharHex, NumberStyles.HexNumber);

                for(int cid = rangeStart; cid <= rangeEnd; cid++)
                {
                    string mapFromHex = cid.ToString($"x{cidSize}").ToUpper();
                    conversionTable.Add(mapFromHex, (char)mapped);
                    mapped++;
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