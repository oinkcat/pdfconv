using System;
using System.Collections.Generic;

namespace Simple.Parsing
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
        /// Parse attributes string
        /// </summary>
        /// <param name="attrString">Object attributes string</param>
    	public void Parse(string attrString)
    	{
    		Console.WriteLine(attrString);
    		
    		foreach(var (pos, type, value) in NextToken(attrString))
    		{
    			Console.WriteLine($"{pos} - {type}: {value}");
    		}
    	}
    	
        // Get next token in attributes list
    	private IEnumerable<(int, int, string)> NextToken(string inString)
    	{
    		int nextPos = 0;
    		
    		while(nextPos < inString.Length)
    		{
    			int minPos = inString.Length;
    			int minType = -1;
    			int typeIdx = 0;
    			
	    		foreach(string dl in delimiters)
		    	{
		    		int pos = inString.IndexOf(dl, nextPos);
		    		if(pos > -1)
		    		{
		    			if(pos < minPos)
		    			{
		    				minPos = pos;
		    				minType = typeIdx;
		    			}
		    		}
		    		
		    		typeIdx++;
		    	}
		    	
		    	if(nextPos < minPos)
		    	{
		    		string idValue = inString.Substring(nextPos, minPos - nextPos);
		    		yield return (nextPos, -1, idValue);
		    	}
		    	
		    	string delim = delimiters[minType];
				yield return (minPos, minType, delim);
		    	nextPos = minPos + delim.Length;
    		}
    	}
    }
}