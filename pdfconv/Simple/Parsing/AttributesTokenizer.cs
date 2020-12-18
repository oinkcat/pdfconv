using System;
using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Splits object's attribute string to tokens
    /// </summary>
    public class AttributesTokenizer
    {
        private static string[] delimiters = {
    		"<<", ">>",
            "<", ">",
    		"(", ")",
    		"[", "]",
    		" ", "/"
        };

		// private Token lastToken;
    	
        /// <summary>
        /// Tokenize attribute string
        /// </summary>
        /// <param name="inString">Object attribute string</param>
        /// <returns>Token sequence</returns>
    	public IEnumerable<Token> Tokenize(string inString)
    	{
    		int nextPos = 0;
    		
    		while(nextPos < inString.Length)
    		{
    			int minPos = inString.Length;
    			int minType = -1;
    			
	    		for(int tokIdx = 0; tokIdx < delimiters.Length; tokIdx++)
		    	{
		    		int pos = inString.IndexOf(delimiters[tokIdx], nextPos);
		    		if(pos > -1)
		    		{
		    			if(pos < minPos)
		    			{
		    				minPos = pos;
		    				minType = tokIdx;
		    			}
		    		}
		    	}
		    	
		    	if(nextPos < minPos)
		    	{
					// Next token is id or number
		    		string idValue = inString.Substring(nextPos, minPos - nextPos);
		    		yield return ParseIdOrNumber(idValue);
		    	}

				if(minType == -1) {  break; } // No more tokens in current string

				var delimiterToken = (TokenType)minType;
				string delim = delimiters[minType];

				if(delimiterToken != TokenType.Space)
				{
					// Next token is delimiter except of space
					yield return new Token(delimiterToken, delim);
				}

				nextPos = minPos + delim.Length;
    		}
    	}

		// Get identifier or number from given string
		private Token ParseIdOrNumber(string idOrNum)
		{
			return double.TryParse(idOrNum, out double number)
				? Token.CreateNumber(number)
				: Token.CreateIdentifier(idOrNum);
		}
    }
}