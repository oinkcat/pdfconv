using System.IO;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Source for tokens based on TextReader
    /// </summary>
    public class TextReaderTokenSource : ITokenStreamSource
    {
        private TextReader sourceReader;

        /// <summary>
        /// Read new line from reader
        /// </summary>
        /// <returns>Source text line</returns>
        public string GetNextLine()
        {
            if(sourceReader.Peek() > -1)
            {
                return sourceReader.ReadLine();
            }
            else
            {
                return null;
            }
        }

        public TextReaderTokenSource(TextReader reader)
        {
            sourceReader = reader;
        }
    }
}