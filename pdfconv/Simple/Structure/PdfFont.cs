using System;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// Base PDF font
    /// </summary>
    public abstract class PdfFont
    {
        protected PdfDocument document;

        /// <summary>
        /// PDF font object
        /// </summary>
        public PdfObject RawObject { get; protected set; }

        /// <summary>
        /// Font name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Decode input string
        /// </summary>
        /// <param name="input">A string to decode</param>
        /// <returns>Decoded string</returns>
        public abstract string DecodeString(string input);

        /// <summary>
        /// Fill basic font information
        /// </summary>
        protected void PopulateBasicInfo()
        {
            Name = RawObject.GetAttributeValue<string>("BaseFont");
        }
    }
}