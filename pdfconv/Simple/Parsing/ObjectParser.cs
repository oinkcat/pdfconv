using System;
using System.Collections.Generic;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Parses object's contents
    /// </summary>
    public class ObjectParser
    {
        private TokenStreamer tokener;

        /// <summary>
        /// Read one PDF object from stream
        /// </summary>
        /// <returns>parsed PDF object</returns>
        public IPdfTerm ReadSingleObject() => ParseTerm();

        /// <summary>
        /// Read PDF command and fill it's parameters list
        /// </summary>
        /// <param name="parameters">Command's parameters list</param>
        /// <returns>Command name</returns>
        public PdfAtom ReadNextCommand(IList<IPdfTerm> parameters)
        {
            bool commandRead = false;
            IPdfTerm parsedTerm = null;

            while(!commandRead)
            {
                parsedTerm = ParseTerm();
                commandRead = (parsedTerm == null) ||
                              (parsedTerm is PdfAtom atom && atom.Type == TokenType.Id);

                if(!commandRead)
                {
                    parameters.Add(parsedTerm);
                }
            }

            return parsedTerm as PdfAtom;
        }

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
                var element = ParseTermOrSequence();
                dict.Add(elementKey, element);

                token = tokener.GetNextToken();
            }

            return dict;
        }

        // Parse one or more sequential terms
        private IPdfTerm ParseTermOrSequence()
        {
            var firstElement = ParseTerm();
            
            var nextToken = tokener.GetNextToken();

            if(nextToken.IsAtomic && nextToken.Type != TokenType.Name)
            {
                var sequence = new PdfSequence { firstElement };
                
                while(nextToken.IsAtomic && nextToken.Type != TokenType.Name)
                {
                    sequence.Add(new PdfAtom(nextToken));
                    nextToken = tokener.GetNextToken();
                }

                tokener.PushBackToken(nextToken);
                return sequence;
            }
            else
            {
                tokener.PushBackToken(nextToken);
                return firstElement;
            }
        }

        public ObjectParser(TokenStreamer tokenStreamer)
        {
            tokener = tokenStreamer;
        }
    }
}