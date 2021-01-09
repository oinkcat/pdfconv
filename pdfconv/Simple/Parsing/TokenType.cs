using System;

namespace PdfConverter.Simple.Parsing
{
    /// <summary>
    /// PDF token type
    /// </summary>
    public enum TokenType
    {
        DictStart,
        DictEnd,
        HexStringStart,
        HexStringEnd,
        OpenBrace,
        CloseBrace,
        OpenSquareBrace,
        CloseSquareBrace,
        Space,
        Slash,
        Number,
        Id,
        Name,
        HexString,
        EndOfLine
    }
}
