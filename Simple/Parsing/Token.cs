using System;
using System.Linq;

namespace Simple.Parsing
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

        public Token(TokenType type, object value = null)
        {
            Type = type;
            Value = value;
        }
    }
}