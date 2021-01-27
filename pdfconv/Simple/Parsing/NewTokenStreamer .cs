using System;
using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Yields tokens sequentially (new version)
    /// </summary>
    public class NewTokenStreamer 
    {
        private ITokenStreamSource lineSource;

        private IEnumerator<Token> tokenSource;

        private Token pushedBackToken;

        /// <summary>
        /// Reached end of current line
        /// </summary>
        public bool AtEndOfStream { get; private set; }

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
                AtEndOfStream = false;
                return tokenToRetry;
            }

            // Yield next token
            if(tokenSource.MoveNext())
            {
                return tokenSource.Current;
            }
            else
            {
                tokenSource.Dispose();
                tokenSource = null;

                UpdateTokenSource();
                
                if(tokenSource.MoveNext())
                {
                    return tokenSource.Current;
                }
                else
                {
                    AtEndOfStream = true;
                    return new Token(TokenType.End);
                }
            }
        }

        private void UpdateTokenSource()
        {
            string nextLine = GetNextNonEmptyLine();
            tokenSource = new ContentTokenizer()
                .Tokenize(nextLine ?? String.Empty)
                .GetEnumerator();
        }

        private string GetNextNonEmptyLine()
        {
            string lineRead = String.Empty;

            while(lineRead.Length == 0)
            {
                lineRead = lineSource.GetNextLine();

                if(lineRead == null)
                {
                    return null;
                }
            }

            return lineRead;
        }

        public NewTokenStreamer(ITokenStreamSource lineSource)
        {
            this.lineSource = lineSource;
            UpdateTokenSource();
        }
    }
}