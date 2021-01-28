using System;

namespace PdfConverter.Simple.Primitives
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
        StringStart,
        StringEnd,
        ArrayStart,
        ArrayEnd,
        Space,
        Comment,
        Slash,
        Number,
        Id,
        Name,
        String,
        HexString,
        EndOfLine,
        End
    }
}
