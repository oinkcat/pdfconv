using System.Collections.Generic;
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

        private const string ComplexObjectContent = @"
            [
                <</Length 1000/Filter/FlateDecode>>
                10
                (test)
                <</TestArray [0 1 2]/TestDict<</K/V>>>>
            ]
        ";

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

        /// <summary>
        /// Test parsing full object's content terms
        /// </summary>
        [Fact]
        public void TestAllTermsParsing()
        {
            var sourceList = new List<string> { ObjectContent };
            var streamer = NewTokenStreamer.CreateFromList(sourceList);
            var parser = new ObjectParser(streamer);

            var allTerms = new List<IPdfTerm>();
            IPdfTerm parsedTerm;

            do
            {
                parsedTerm = parser.ReadSingleObject();
                if(parsedTerm != null)
                {
                    allTerms.Add(parsedTerm);
                }
            }
            while(parsedTerm != null);

            Assert.Null(parsedTerm);
            Assert.Equal(5, allTerms.Count);
        }

        /// <summary>
        /// Test parsing object content terms
        /// </summary>
        [Fact]
        public void TestObjectTermsParsing()
        {
            var sourceList = new List<string>(ComplexObjectContent.Split("\r\n"));
            var streamer = NewTokenStreamer.CreateFromList(sourceList);
            var parser = new ObjectParser(streamer);

            IPdfTerm term = parser.ReadSingleObject();
            Assert.IsType<PdfArray>(term);

            var firstArrayElement = (term as PdfArray)[0];
            Assert.IsType<PdfDictionary>(firstArrayElement);
        }
    }
}