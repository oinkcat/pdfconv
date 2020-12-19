using System;
using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Parses object attribute string
    /// </summary>
    public class AttributesParser
    {
        private Dictionary<string, object> parsedAttributes;

        private int openGroupLevel;

        private bool parsingCompleted;

        private IEnumerator<Token> tokenSource;

        private string currentChunk;

        /// <summary>
        /// Get parse results
        /// </summary>
        /// <returns>Parsed attributes</returns>
        public Dictionary<string, object> GetParsedAttributes()
        {
            return parsingCompleted
                ? new Dictionary<string, object>(parsedAttributes)
                : null;
        }

        /// <summary>
        /// Parse next chunk of string
        /// </summary>
        /// <param name="inString">String to parse</param>
        /// <returns>Can continue parse</returns>
        public bool FeedNextChunk(string inString)
        {
            // TODO: Full implementation

            if(parsingCompleted) { return false; }

            currentChunk = inString;

            var token = GetNextToken();

            // Very basic parsing
            while(token.Type != TokenType.GroupEnd && token.Type != TokenType.EndOfLine)
            {
                if(token.Type == TokenType.Delimiter)
                {
                    string attribName = GetNextToken().Value as string;

                    var valueOrDlToken = GetNextToken();
                    if(valueOrDlToken.Type != TokenType.Delimiter)
                    {
                        object attribValue = valueOrDlToken switch {
                            { Type: TokenType.Number } => (double)valueOrDlToken.Value,
                            { Type: TokenType.Id } => (string)valueOrDlToken.Value,
                            _ => null
                        };
                        parsedAttributes.Add(attribName, attribValue);
                    }
                    else
                    {
                        var attribValue = GetNextToken().Value;
                        parsedAttributes.Add(attribName, attribValue);
                    }
                }

                token = GetNextToken();
            }

            parsingCompleted = token.Type == TokenType.GroupEnd;

            return !parsingCompleted;
        }

        // Get next token in stream
        private Token GetNextToken()
        {
            if(tokenSource == null)
            {
                var tokenizer = new AttributesTokenizer();
                tokenSource = tokenizer.Tokenize(currentChunk).GetEnumerator();
            }

            if(tokenSource.MoveNext())
            {
                return tokenSource.Current;
            }
            else
            {
                tokenSource.Dispose();
                tokenSource = null;

                return Token.EOL;
            }
        }

        /// <summary>
        /// Reset parser state
        /// </summary>
        public void Reset()
        {
            parsedAttributes.Clear();
            parsingCompleted = false;
            openGroupLevel = 0;
            currentChunk = null;
        }

        public AttributesParser()
        {
            parsedAttributes = new Dictionary<string, object>();
        }
    }
}