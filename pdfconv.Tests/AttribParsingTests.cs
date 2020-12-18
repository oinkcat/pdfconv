using System;
using System.IO;
using System.Text;
using System.Linq;
using Xunit;
using PdfConverter.Simple.Parsing;

namespace PdfConverter.Tests
{
    /// <summary>
    /// PDF object attributes parsing tests
    /// </summary>
    public class AttribParsingTests
    {
        const string BasicString = "<</Length 274/Filter/FlateDecode>>";

        const string ComplexString = @"
            <</Type/Font/Subtype/TrueType/BaseFont/BAAAAA+LiberationSerif-Bold
            /FirstChar 0
            /LastChar 11
            /Widths[777 666 443 389 333 250 722 500 443 556 833 556 ]
            /FontDescriptor 13 0 R
            /ToUnicode 14 0 R
            >>";

        /// <summary>
        /// Attribute tokenizer test
        /// </summary>
        [Fact]
        public void TestTokenizeAttribs()
        {            
            var attribTokener = new AttributesTokenizer();
            var attribTokens = attribTokener.Tokenize(BasicString);

            Assert.NotEmpty(attribTokens);
            Assert.Equal(TokenType.GroupStart, attribTokens.First().Type);
            Assert.Equal(TokenType.GroupEnd, attribTokens.Last().Type);

            // Check absence of whitespace tokens
            Assert.False(attribTokens.Any(t => t.Type == TokenType.Space));
        }

        /// <summary>
        /// Attribute parser test with simple input
        /// </summary>
        [Fact]
        public void TestParseBasicAttribs()
        {
            const string CompressionAttrib = "Filter";

            var attribParser = new AttributesParser();

            bool needMoreInput = attribParser.FeedNextChunk(BasicString);
            var attribs = attribParser.GetParsedAttributes();

            Assert.False(needMoreInput);
            Assert.Contains(CompressionAttrib, attribs.Keys);
        }

        /// <summary>
        /// Attribute parser test with more complex input
        /// </summary>
        [Fact]
        public void TestParseComplexAttribs()
        {
            using var linesReader = CreateStringReader(ComplexString);
            var attribParser = new AttributesParser();
            bool needMoreInput = true;

            while(!linesReader.EndOfStream)
            {
                string line = linesReader.ReadLine().Trim();

                if(line.Length > 0)
                {
                    needMoreInput = attribParser.FeedNextChunk(line);
                }
            }

            Assert.False(needMoreInput);
        }

        // Create text lines reader for string
        private StreamReader CreateStringReader(string input)
        {
            var textBytes = Encoding.Default.GetBytes(input);
            return new StreamReader(new MemoryStream(textBytes));
        }
    }
}
