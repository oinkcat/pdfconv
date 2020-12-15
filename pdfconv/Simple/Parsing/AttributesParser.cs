using System;
using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Object attributes parser
    /// </summary>
    public class AttributesParser
    {
        private static string[] delimiters = {
    		"<<", ">>",
            "<", ">",
    		"(", ")",
    		"[", "]",
    		" ", "/"
        };
    	
        /// <summary>
        /// Parse attribute string to tokens
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
		    		string idValue = inString.Substring(nextPos, minPos - nextPos);
		    		yield return Token.CreateIdentifier(idValue);
		    	}
		    	
		    	string delim = delimiters[minType];
				yield return new Token((TokenType)minType, delim);
		    	nextPos = minPos + delim.Length;
    		}
    	}
    }
}