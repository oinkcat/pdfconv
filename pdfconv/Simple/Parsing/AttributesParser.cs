using System.Linq;
using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Parses object attribute string
    /// </summary>
    public class AttributesParser
    {
        private Dictionary<string, object> parsedAttributes;

        private bool parsingCompleted;

        private IEnumerator<Token> tokenSource;

        private Token pushedBackToken;

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
            while(token.Type != TokenType.DictEnd && token.Type != TokenType.EndOfLine)
            {
                if(token.Type == TokenType.Delimiter)
                {
                    string attribName = GetNextToken().Value as string;

                    var valueOrDlToken = GetNextToken();
                    if(valueOrDlToken.Type != TokenType.Delimiter)
                    {
                        PushBackToken(valueOrDlToken);
                        var inlineAttributeValues = ReadInlineAttributeValues();
                        parsedAttributes.Add(attribName, inlineAttributeValues);
                    }
                    else
                    {
                        var attribValue = GetNextToken().Value;
                        parsedAttributes.Add(attribName, attribValue);
                    }
                }

                token = GetNextToken();
            }

            parsingCompleted = token.Type == TokenType.DictEnd;

            return !parsingCompleted;
        }

        // Get next token in stream
        private Token GetNextToken()
        {
            if(pushedBackToken != null)
            {
                var tokenToRetry = pushedBackToken;
                pushedBackToken = null;
                return tokenToRetry;
            }

            if(tokenSource == null)
            {
                tokenSource = new AttributesTokenizer()
                    .Tokenize(currentChunk)
                    .GetEnumerator();
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

                return Token.EOL;
            }
        }

        private object ReadInlineAttributeValues()
        {
            Token nextToken = null;
            var inlineAttributes = new List<object>();

            var terminalTokens = new HashSet<TokenType>(new TokenType[] {
                TokenType.Delimiter, TokenType.EndOfLine, TokenType.DictEnd
            });

            do
            {
                nextToken = GetNextToken();
                object attribValue = nextToken switch {
                    { Type: TokenType.Number } => (double)nextToken.Value,
                    { Type: TokenType.Id } => (string)nextToken.Value,
                    _ => null
                };
                
                if(attribValue != null)
                {
                    inlineAttributes.Add(attribValue);
                }
            }
            while(!terminalTokens.Contains(nextToken.Type));

            PushBackToken(nextToken);

            return (inlineAttributes.Count > 1)
                ? inlineAttributes
                : inlineAttributes.FirstOrDefault();
        }

        private void PushBackToken(Token token) => pushedBackToken = token;

        /// <summary>
        /// Reset parser state
        /// </summary>
        public void Reset()
        {
            parsedAttributes.Clear();
            parsingCompleted = false;
            currentChunk = null;
            pushedBackToken = null;
        }

        public AttributesParser()
        {
            parsedAttributes = new Dictionary<string, object>();
        }
    }
}