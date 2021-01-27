using System.Collections.Generic;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Basic PDF content element
    /// </summary>
    public interface IPdfTerm
    {
        // TODO: Useful members?
    }

    /// <summary>
    /// Non-container content element
    /// </summary>
    public class PdfAtom : IPdfTerm
    {
        private Token atomToken;

        /// <summary>
        /// Object type
        /// </summary>
        public TokenType Type => atomToken.Type;

        /// <summary>
        /// Object value
        /// </summary>
        public object Value => atomToken.Value;

        /// <summary>
        /// Get string representation of atom term
        /// </summary>
        /// <returns>Term type and value</returns>
        public override string ToString() => $"{Type}: {Value}";

        public PdfAtom(Token atomToken)
        {
            this.atomToken = atomToken;
        }
    }

    /// <summary>
    /// List of Pdf content elements
    /// </summary>
    public class PdfArray : List<IPdfTerm>, IPdfTerm
    {
        public override string ToString() => $"Array: {Count}";
    }

    /// <summary>
    /// Name-value map container
    /// </summary>
    public class PdfDictionary : Dictionary<string, IPdfTerm>, IPdfTerm
    { 
        public override string ToString() => $"Dict: {Count}";
    }
}