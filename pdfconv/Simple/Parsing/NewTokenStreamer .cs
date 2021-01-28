using System;
using System.IO;
using System.Collections.Generic;
using PdfConverter.Simple.Primitives;

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
                return RetrieveNextTokenFromSource();
            }
        }

        private Token RetrieveNextTokenFromSource()
        {
            Token readToken = null;

            while(readToken == null)
            {
                tokenSource.Dispose();
                tokenSource = null;

                bool hasMoreInput = UpdateTokenSource();
                
                if(hasMoreInput)
                {
                    if(tokenSource.MoveNext())
                    {
                        readToken = tokenSource.Current;
                    }
                }
                else
                {
                    AtEndOfStream = true;
                    return new Token(TokenType.End);
                }
            }

            return readToken;
        }

        private bool UpdateTokenSource()
        {
            string nextLine = GetNextNonEmptyLine();
            tokenSource = new ContentTokenizer()
                .Tokenize(nextLine ?? String.Empty)
                .GetEnumerator();

            return nextLine != null;
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

        public static NewTokenStreamer CreateFromReader(TextReader reader)
        {
            return new NewTokenStreamer(new TextReaderTokenSource(reader));
        }

        public static NewTokenStreamer CreateFromList(IList<string> stringList)
        {
            return new NewTokenStreamer(new StringListTokenSource(stringList));
        }

        public NewTokenStreamer(ITokenStreamSource lineSource)
        {
            this.lineSource = lineSource;
            UpdateTokenSource();
        }
    }
}