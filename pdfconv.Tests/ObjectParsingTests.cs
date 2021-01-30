using System.Collections.Generic;
using Xunit;
using PdfConverter.Simple.Parsing;
using PdfConverter.Simple.Primitives;

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

        private const string ObjectWithComplexString = @"[1 2 (TESTING \(test\)) 3]";

        /// <summary>
        /// Test parsing multipart object contents
        /// </summary>
        [Fact]
        public void TestObjectParsing()
        {
            var sourceList = new List<string> { ObjectContent };
            var streamer = TokenStreamer.CreateFromList(sourceList);

            // Read object id
            var currentToken = Token.EOL;
            while(currentToken.Type != TokenType.Id)
            {
                currentToken = streamer.GetNextToken();
            }

            Assert.Equal("obj", currentToken.Value as string);

            // Read attribute dict
            var attribParser = new ObjectParser(streamer);
            var parsedAttribs = attribParser.ReadSingleObject() as PdfDictionary;

            Assert.Equal(804, (int)(parsedAttribs["Length"] as PdfAtom).AsNumber());

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
            var streamer = TokenStreamer.CreateFromList(sourceList);
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
            var streamer = TokenStreamer.CreateFromList(sourceList);
            var parser = new ObjectParser(streamer);

            IPdfTerm term = parser.ReadSingleObject();
            Assert.IsType<PdfArray>(term);

            var firstArrayElement = (term as PdfArray)[0];
            Assert.IsType<PdfDictionary>(firstArrayElement);
        }

        /// <summary>
        /// Test parsing object that contains string with escaped brackets
        /// </summary>
        [Fact]
        public void TestObjectWithComplexStringParsing()
        {
            var sourceList = new List<string> { ObjectWithComplexString };
            var parser = new ObjectParser(TokenStreamer.CreateFromList(sourceList));

            var parsedObj = parser.ReadSingleObject() as PdfArray;

            Assert.Equal(4, parsedObj.Count);
        }
    }
}