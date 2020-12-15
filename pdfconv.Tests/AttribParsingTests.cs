using System;
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
        [Fact]
        public void TestParseAttribs()
        {
            const string AttribString = "<</Length 274/Filter/FlateDecode>>";
            
            var attribParser = new AttributesParser();
            var parsedAttribs = attribParser.Tokenize(AttribString);

            Assert.NotEmpty(parsedAttribs);
            Assert.Equal(TokenType.GroupStart, parsedAttribs.First().Type);
            Assert.Equal(TokenType.GroupEnd, parsedAttribs.Last().Type);
        }
    }
}
