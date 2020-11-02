using System;
using System.Threading.Tasks;

namespace PdfConverter
{
    /// <summary>
    /// Converts PDF file to text
    /// </summary>
    public interface IPdfConverter
    {
        /// <summary>
        /// Convert PDF file to text
        /// </summary>
        Task<bool> ConvertFile(string filePath);
    }
}