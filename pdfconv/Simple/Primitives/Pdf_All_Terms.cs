using System.Collections.Generic;

namespace PdfConverter.Simple.Primitives
{
    /// <summary>
    /// Basic PDF content element
    /// </summary>
    public interface IPdfTerm
    {
        /// <summary>
        /// Is term represents atomic value
        /// </summary>
        bool IsAtomic { get; }
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
        /// Atom is atomic
        /// </summary>
        public bool IsAtomic => true;

        /// <summary>
        /// Get value as number
        /// </summary>
        /// <returns>Term value as double</returns>
        public double AsNumber() => (double)atomToken.Value;

        /// <summary>
        /// Get value as string
        /// </summary>
        /// <returns>Term value as string</returns>
        public string AsString() => atomToken.Value as string;

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
        /// <summary>
        /// Array/sequence is not atomic
        /// </summary>
        public bool IsAtomic => false;

        public override string ToString() => $"Array: {Count}";
    }

    /// <summary>
    /// Arbitrary sequence of terms
    /// </summary>
    public class PdfSequence : PdfArray
    {
        public override string ToString() => $"Seq: {Count}";
    }

    /// <summary>
    /// Name-value map container
    /// </summary>
    public class PdfDictionary : Dictionary<string, IPdfTerm>, IPdfTerm
    { 
        /// <summary>
        /// Dictionary is not atomic
        /// </summary>
        public bool IsAtomic => false;
        
        public override string ToString() => $"Dict: {Count}";
    }
}