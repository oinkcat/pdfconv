using System.IO;
using System.Text;
using Xunit;
using PdfConverter.Simple.Parsing;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Tests
{
    public class ContinuousParsingTests
    {
        const string ObjectContent = @"
            <</Type/Font/Subtype/TrueType/BaseFont/BAAAAA+LiberationSerif-Bold
            /FirstChar 0
            /LastChar 11
            /Widths [777 666 443 389 333 250 722 500 443 556 833 556 ]
            /FontDescriptor 13 0 R
            /ToUnicode 14 0 R
            >>";

        [Fact]
        public void TestContinuousParsing()
        {
            const int NumTokensInTestString = 35;

            using var contentReader = CreateStringReader(ObjectContent);
            var streamer = TokenStreamer.CreateFromReader(contentReader);

            Token currentToken;
            int numTokensRead = 0;

            do
            {
                currentToken = streamer.GetNextToken();
                numTokensRead++;
            }
            while(currentToken.Type != TokenType.End);

            Assert.True(streamer.AtEndOfStream);
            Assert.Equal(NumTokensInTestString, numTokensRead - 1);
        }

        // Create text lines reader for string
        private StreamReader CreateStringReader(string input)
        {
            var textBytes = Encoding.Default.GetBytes(input);
            return new StreamReader(new MemoryStream(textBytes));
        }
    }
}