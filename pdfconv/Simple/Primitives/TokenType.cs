using System;

namespace PdfConverter.Simple.Primitives
{
    /// <summary>
    /// PDF token type
    /// </summary>
    public enum TokenType
    {
        Space,
        Slash,
        StringStart,
        StringEnd,
        DictStart,
        DictEnd,
        ArrayStart,
        ArrayEnd,
        HexStringStart,
        HexStringEnd,
        Comment,
        Number,
        Id,
        Name,
        String,
        HexString,
        EndOfLine,
        End
    }
}
