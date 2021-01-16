using System.Linq;
using Xunit;
using PdfConverter.Simple.Parsing;

namespace PdfConverter.Tests
{
    /// <summary>
    /// Test parsing PDF objects' content
    /// </summary>
    public class PdfContentTests
    {
        private const string SimpleContentLine = "<01> <0031>";

        private const string ComplexContentLine = "56.8 546.2 Td /F1 12 Tf[<11>5<12>-3" +
                                                  "<13>14<14>10<15>2<1617>17<18191A>-1" +
                                                  "<1B>]TJ";

        private const string MultilineStringContentLine = "0 0 Td /F1 2 Tf<11121314\n" +
                                                          "15161718\n" +
                                                          "191A1B>Tj";

        /// <summary>
        /// Test parsing simple content line
        /// </summary>
        [Fact]
        public void TestParsingSimpleContent()
        {
            var contentTokens = new ContentTokenizer()
                                    .Tokenize(SimpleContentLine)
                                    .ToList();

            Assert.Equal(TokenType.HexString, contentTokens[0].Type);
            Assert.Equal(TokenType.HexString, contentTokens[1].Type);
        }

        /// <summary>
        /// Test parsing more compplex content line
        /// </summary>
        [Fact]
        public void TestParsingComplexContent()
        {
            var contentTokens = new ContentTokenizer().Tokenize(ComplexContentLine);
            var allHexStrings = contentTokens.Where(t => t.Type == TokenType.HexString);

            Assert.Equal(8, allHexStrings.Count());
        }

        /// <summary>
        /// Test paring conent line with multi-line string literal
        /// </summary>
        [Fact]
        public void TestParsingMultilineStringContent()
        {
            string[] multilineContent = MultilineStringContentLine.Split('\n');
            var tokenizer = new ContentTokenizer();

            foreach(string contentLine in multilineContent)
            {
                var contentTokens = tokenizer.Tokenize(contentLine);
                bool hasHexString = contentTokens.Any(t => t.Type == TokenType.HexString);
                Assert.True(hasHexString);
            }
        }
    }
}