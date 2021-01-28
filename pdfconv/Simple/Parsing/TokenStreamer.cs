using System.Collections.Generic;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Yields tokens sequentially
    /// </summary>
    public class TokenStreamer
    {
        private string currentLine;

        private Token pushedBackToken;

        private IEnumerator<Token> tokenSource;

        /// <summary>
        /// Reached end of current line
        /// </summary>
        public bool AtEndOfLine { get; private set; }

        /// <summary>
        /// Set string for tokenizing
        /// </summary>
        /// <param name="line">String to be tokenized</param>
        public void SetSourceLine(string line) => currentLine = line;

        /// <summary>
        /// Put token back to stream
        /// </summary>
        /// <param name="token">Token to push back</param>
        public void PushBackToken(Token token) => pushedBackToken = token;

        /// <summary>
        /// Get next token in stream
        /// </summary>
        /// <returns>Next token in sequence</returns>
        public Token GetNextToken()
        {
            if(pushedBackToken != null)
            {
                var tokenToRetry = pushedBackToken;
                pushedBackToken = null;
                AtEndOfLine = tokenToRetry.Type == TokenType.EndOfLine;
                return tokenToRetry;
            }

            if(tokenSource == null)
            {
                tokenSource = new ContentTokenizer()
                    .Tokenize(currentLine)
                    .GetEnumerator();
                AtEndOfLine = false;
            }

            // Yield next token
            if(tokenSource.MoveNext())
            {
                return tokenSource.Current;
            }
            else
            {
                AtEndOfLine = true;
                tokenSource.Dispose();
                tokenSource = null;

                return Token.EOL;
            }
        }

        public TokenStreamer()
        {
            AtEndOfLine = true;
        }

        public TokenStreamer(string line) : this()
        {
            currentLine = line;
        }
    }
}