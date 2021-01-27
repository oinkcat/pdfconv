using System;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Parses object's contents
    /// </summary>
    public class ObjectParser
    {
        private NewTokenStreamer tokener;

        /// <summary>
        /// Read one PDF object from stream
        /// </summary>
        /// <returns>parsed PDF object</returns>
        public IPdfTerm ReadSingleObject() => ParseTerm();

        // Parse single PDF structure element
        private IPdfTerm ParseTerm()
        {
            return tokener.GetNextToken() switch {
                { IsAtomic: true } t => new PdfAtom(t),
                { Type: TokenType.ArrayStart } => ParseArrayTerm(),
                { Type: TokenType.DictStart } => ParseDictTerm(),
                { Type: TokenType.End } => null,
                var t => throw new Exception($"Invalid token type: {t.Type}")
            };
        }

        // Parse array that may contain multiple terms
        private IPdfTerm ParseArrayTerm()
        {
            var array = new PdfArray();

            var elementToken = tokener.GetNextToken();

            while(elementToken.Type != TokenType.ArrayEnd)
            {
                tokener.PushBackToken(elementToken);
                array.Add(ParseTerm());

                elementToken = tokener.GetNextToken();
            }

            return array;
        }

        // Parse name-term mapping
        private IPdfTerm ParseDictTerm()
        {
            var dict = new PdfDictionary();

            var token = tokener.GetNextToken();

            while(token.Type != TokenType.DictEnd)
            {
                if(token.Type != TokenType.Name)
                {
                    throw new Exception("Expected: name token");
                }

                string elementKey = token.Value as string;
                dict.Add(elementKey, ParseTerm());

                token = tokener.GetNextToken();
            }

            return dict;
        }

        public ObjectParser(NewTokenStreamer tokenStreamer)
        {
            tokener = tokenStreamer;
        }
    }
}