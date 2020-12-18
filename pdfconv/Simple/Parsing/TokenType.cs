using System;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// PDF token type
    /// </summary>
    public enum TokenType
    {
        GroupStart,
        GroupEnd,
        StringStart,
        StringEnd,
        OpenBrace,
        CloseBrace,
        OpenSquareBrace,
        CloseSquareBrace,
        Space,
        Delimiter,
        Number,
        Id,
        EndOfLine
    }
}
