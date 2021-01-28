using System.Linq;
using System.Collections.Generic;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Parses object attribute string
    /// </summary>
    public class AttributesParser
    {
        private Dictionary<string, object> parsedAttributes;

        private bool parsingCompleted;

        private int nestedDictLevel;

        private List<object> currentArray;

        private TokenStreamer streamer;

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

            if(inString != null)
            {
                streamer.SetSourceLine(inString);
            }

            var token = streamer.GetNextToken();
            bool chunkParsed = false;

            // Very basic parsing
            while(!chunkParsed)
            {
                if(token.Type == TokenType.Name)
                {
                    string attribName = token.Value as string;

                    var valueOrNameToken = streamer.GetNextToken();
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

                        streamer.PushBackToken(valueOrNameToken);
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
                    // HACK: Flattening all nested structures
                    currentArray.Add(token.Value);
                }
                else if(token.Type == TokenType.ArrayEnd)
                {
                    currentArray = null;
                }
                else if(token.Type == TokenType.DictStart)
                {
                    nestedDictLevel++;
                }
                else if(token.Type == TokenType.DictEnd)
                {
                    nestedDictLevel--;
                }
                else if(token.Type == TokenType.EndOfLine)
                {
                    chunkParsed = true;
                }

                if(nestedDictLevel <= 0)
                {
                    parsingCompleted = true;
                    chunkParsed = true;
                }

                if(!chunkParsed)
                {
                    token = streamer.GetNextToken();
                }
            }

            return !parsingCompleted;
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
                nextToken = streamer.GetNextToken();
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

            streamer.PushBackToken(nextToken);

            return (inlineAttributes.Count > 1)
                ? inlineAttributes
                : inlineAttributes.FirstOrDefault();
        }

        private void SaveAttribute(string name, object value)
        {
            string nameWithLevel = (nestedDictLevel > 1)
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
            nestedDictLevel = 0;
        }

        public AttributesParser()
        {
            parsedAttributes = new Dictionary<string, object>();
            streamer = new TokenStreamer();
        }

        public AttributesParser(TokenStreamer streamer)
        {
            parsedAttributes = new Dictionary<string, object>();
            this.streamer = streamer;
        }
    }
}