using System;
using System.IO;
using System.Text;
using System.Linq;
using Xunit;
using PdfConverter.Simple.Parsing;
using PdfConverter.Simple.Primitives;

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
            /Widths [777 666 443 389 333 250 722 500 443 556 833 556 ]
            /FontDescriptor 13 0 R
            /ToUnicode 14 0 R
            >>";

        /// <summary>
        /// Attribute tokenizer test
        /// </summary>
        [Fact]
        public void TestTokenizeAttribs()
        {            
            var attribTokener = new ContentTokenizer();
            var attribTokens = attribTokener.Tokenize(BasicString);

            Assert.NotEmpty(attribTokens);
            Assert.Equal(TokenType.DictStart, attribTokens.First().Type);
            Assert.Equal(TokenType.DictEnd, attribTokens.Last().Type);

            // Check absence of whitespace tokens
            bool hasSpaceTokens = attribTokens.Any(t => t.Type == TokenType.Space);
            Assert.False(hasSpaceTokens);
        }

        /// <summary>
        /// Attribute parser test with simple input
        /// </summary>
        [Fact]
        public void TestParseBasicAttribs()
        {
            const string CompressionAttrib = "Filter";

            using var linesReader = CreateStringReader(BasicString);
            var streamer = TokenStreamer.CreateFromReader(linesReader);
            var attribParser = new ObjectParser(streamer);

            var attribs = attribParser.ReadSingleObject() as PdfDictionary;
            Assert.Contains(CompressionAttrib, attribs.Keys);

            var nextToken = attribParser.ReadSingleObject();
            Assert.Null(nextToken);
        }

        /// <summary>
        /// Attribute parser test with more complex input
        /// </summary>
        [Fact]
        public void TestParseComplexAttribs()
        {
            using var linesReader = CreateStringReader(ComplexString);
            var streamer = TokenStreamer.CreateFromReader(linesReader);
            var attribParser = new ObjectParser(streamer);

            var attribDict = attribParser.ReadSingleObject();
            Assert.IsType<PdfDictionary>(attribDict);

            var nextTerm = attribParser.ReadSingleObject();
            Assert.Null(nextTerm);
        }

        // Create text lines reader for string
        private StreamReader CreateStringReader(string input)
        {
            var textBytes = Encoding.Default.GetBytes(input);
            return new StreamReader(new MemoryStream(textBytes));
        }
    }
}
