using System;
using System.Globalization;
using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Splits object's content string to tokens
    /// </summary>
    public class ContentTokenizer
    {
        private static string[] delimiters = {
    		"<<", ">>",
            "<", ">",
    		"(", ")",
    		"[", "]",
    		" ", "/"
        };
    	
        /// <summary>
        /// Tokenize object content string
        /// </summary>
        /// <param name="inString">Object content string</param>
        /// <returns>Token sequence</returns>
    	public IEnumerable<Token> Tokenize(string inString)
    	{
    		int nextPos = 0;
    		
    		while(nextPos < inString.Length)
    		{
    			var (tokStartPos, tokType) = GetNextTokenStartPos(inString, nextPos);
		    	
		    	if(nextPos < tokStartPos)
		    	{
					// Next token is id or number
		    		string idValue = inString.Substring(nextPos, tokStartPos - nextPos);
		    		yield return ParseIdOrNumber(idValue);
		    	}

				if(tokType == -1) { break; } // No more tokens in current string

				// Get token starting from delimiter position
				var nextToken = GetGenericToken(inString, 
												tokStartPos, 
												tokType, 
												out var tokenValue);

				if(nextToken != null)
				{
					yield return nextToken;
				}

				nextPos = tokStartPos + tokenValue.Length;
    		}
    	}

		// Find next token starting position and delimiter type
		private (int, int) GetNextTokenStartPos(string inString, int startPos)
		{    			
			int delimPos = inString.Length;
			int delimType = -1;
			
			for(int tokIdx = 0; tokIdx < delimiters.Length; tokIdx++)
			{
				int pos = inString.IndexOf(delimiters[tokIdx], startPos);
				if(pos > -1)
				{
					if(pos < delimPos)
					{
						delimPos = pos;
						delimType = tokIdx;
					}
				}
			}

			return (delimPos, delimType);
		}

		// Get next token info
		private Token GetGenericToken (
			string inString, 
			int tokenPos,
			int tokenType,
			out string tokenValue
		) {				
			var delimiterTokenType = (TokenType)tokenType;

			if(delimiterTokenType == TokenType.HexStringStart)
			{
				// Next token is a hexadecimal string literal
				int endTokenTypeIdx = (int)TokenType.HexStringEnd;
				int strEndPos = inString.IndexOf(delimiters[endTokenTypeIdx], tokenPos);
				if(strEndPos < 0)
					throw new Exception("No HEX string ending token!");

				int tokenLength = strEndPos - tokenPos + 1;
				tokenValue = inString.Substring(tokenPos, tokenLength);
				string hexString = tokenValue.Substring(1, tokenValue.Length - 2);
				return new Token(TokenType.HexString, hexString);
			}
			else if(delimiterTokenType == TokenType.Slash)
			{
				// Next token is a "name"
				(int nameEndPos, _) = GetNextTokenStartPos(inString, tokenPos + 1);
				tokenValue = inString.Substring(tokenPos, nameEndPos - tokenPos);
				return new Token(TokenType.Name, tokenValue.Substring(1));
			}
			else if(delimiterTokenType != TokenType.Space)
			{
				// Next token is delimiter except of space
				tokenValue = delimiters[tokenType];
				return new Token(delimiterTokenType, tokenValue);
			}
			else
			{
				// Token is whitespace
				tokenValue = delimiters[(int)TokenType.Space];
				return null;
			}
		}

		// Get identifier or number from given string
		private Token ParseIdOrNumber(string idOrNum)
		{
			return double.TryParse(idOrNum, 
								   NumberStyles.AllowDecimalPoint, 
								   CultureInfo.InvariantCulture, 
								   out double number)
				? Token.CreateNumber(number)
				: Token.CreateIdentifier(idOrNum);
		}
    }
}