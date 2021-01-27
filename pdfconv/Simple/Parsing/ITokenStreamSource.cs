namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// Source of lines for tokenizing
    /// </summary>
    public interface ITokenStreamSource
    {
        /// <summary>
        /// Get line from source
        /// </summary>
        /// <returns>Line for tokenizer</returns>
        string GetNextLine();
    }
}