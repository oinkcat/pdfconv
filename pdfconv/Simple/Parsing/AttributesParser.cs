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

        private int nestedDictLevel;

        private List<object> currentArray;

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
            while(token.Type != TokenType.EndOfLine)
            {
                if(token.Type == TokenType.Name)
                {
                    string attribName = token.Value as string;

                    var valueOrNameToken = GetNextToken();
                    if(valueOrNameToken.Type != TokenType.Name)
                    {
                        if(valueOrNameToken.Type == TokenType.DictStart)
                        {
                            nestedDictLevel++;
                        }
                        else if(valueOrNameToken.Type == TokenType.ArrayStart)
                        {
                            currentArray = new List<object>();
                        }

                        PushBackToken(valueOrNameToken);
                        var inlineAttributeValues = ReadInlineAttributeValues();
                        SaveAttribute(attribName, inlineAttributeValues);
                    }
                    else
                    {
                        SaveAttribute(attribName, valueOrNameToken.Value);
                    }
                }
                else if(token.IsAtomic && currentArray != null)
                {
                    currentArray.Add(token.Value);
                }
                else if(token.Type == TokenType.ArrayEnd)
                {
                    currentArray = null;
                }
                else if(token.Type == TokenType.DictEnd)
                {
                    nestedDictLevel--;
                }

                token = GetNextToken();
            }
                
            parsingCompleted = nestedDictLevel < 0;

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
                tokenSource = new ContentTokenizer()
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
            var inlineAttributes = (currentArray != null) 
                ? currentArray
                : new List<object>();

            var terminalTokens = new HashSet<TokenType>(new TokenType[] {
                TokenType.Name, TokenType.EndOfLine, 
                TokenType.ArrayEnd, TokenType.DictEnd
            });

            do
            {
                nextToken = GetNextToken();
                object attribValue = nextToken switch {
                    { Type: TokenType.Number } => (double)nextToken.Value,
                    { Type: TokenType.Id } => (string)nextToken.Value,
                    { Type: TokenType.String } => (string)nextToken.Value,
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

        private void SaveAttribute(string name, object value)
        {
            string nameWithLevel = (nestedDictLevel > 0)
                ? $"{name}_{nestedDictLevel}"
                : name;
            parsedAttributes.Add(nameWithLevel, value);
        }

        /// <summary>
        /// Reset parser state
        /// </summary>
        public void Reset()
        {
            parsedAttributes.Clear();
            parsingCompleted = false;
            currentChunk = null;
            pushedBackToken = null;
            nestedDictLevel = 0;
        }

        public AttributesParser()
        {
            parsedAttributes = new Dictionary<string, object>();
        }
    }
}