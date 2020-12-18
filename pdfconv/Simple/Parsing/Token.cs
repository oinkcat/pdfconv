using System;
using System.Linq;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// PDF token
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Token type
        /// </summary>
        public TokenType Type { get; private set; }

        /// <summary>
        /// Token value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Get end-of-line token
        /// </summary>
        /// <returns>EOL token value</returns>
        public static Token EOL => new Token(TokenType.EndOfLine);

        /// <summary>
        /// Create identifier token
        /// </summary>
        /// <param name="id">Identifier name</param>
        /// <returns>Identifier token</returns>
        public static Token CreateIdentifier(string id) => new Token(TokenType.Id, id);

        /// <summary>
        /// Create number token
        /// </summary>
        /// <param name="num">Number value</param>
        /// <returns>Number token</returns>
        public static Token CreateNumber(double num) => new Token(TokenType.Number, num);

        public Token(TokenType type, object value = null)
        {
            Type = type;
            Value = value;
        }
    }
}