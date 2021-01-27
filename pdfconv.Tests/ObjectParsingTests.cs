using Xunit;
using PdfConverter.Simple.Parsing;

namespace PdfConverter.Tests
{
    /// <summary>
    /// Test parsing whole objects
    /// </summary>
    public class ObjectParsingTests
    {
        private const string ObjectContent = "1 0 obj" +
                                             "<</Length 804/Filter/FlateDecode>>" +
                                             "stream";

        /// <summary>
        /// Test parsing multipart object contents
        /// </summary>
        [Fact]
        public void TestObjectParsing()
        {
            var streamer = new TokenStreamer();
            streamer.SetSourceLine(ObjectContent);

            // Read object id
            var currentToken = Token.EOL;
            while(currentToken.Type != TokenType.Id)
            {
                currentToken = streamer.GetNextToken();
            }

            Assert.Equal("obj", currentToken.Value as string);
            Assert.False(streamer.AtEndOfLine);

            // Read attribute dict
            var attribParser = new AttributesParser(streamer);
            bool needMoreInput = attribParser.FeedNextChunk(null);
            var parsedAttribs = attribParser.GetParsedAttributes();

            Assert.False(needMoreInput);
            Assert.Equal(804, (int)(double)parsedAttribs["Length"]);

            // Read body stream start token
            var streamStartToken = streamer.GetNextToken();
            
            Assert.Equal("stream", streamStartToken.Value as string);
        }
    }
}