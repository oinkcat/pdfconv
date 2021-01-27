using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Source for tokens based on list of strings
    /// </summary>
    public class StringListTokenSource : ITokenStreamSource
    {
        private IEnumerator<string> enumerator;

        /// <summary>
        /// Get next line in list
        /// </summary>
        /// <returns>Source text line</returns>
        public string GetNextLine()
        {
            if(enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            else
            {
                return null;
            }
        }

        public StringListTokenSource(IList<string> list)
        {
            enumerator = list.GetEnumerator();
        }
    }
}