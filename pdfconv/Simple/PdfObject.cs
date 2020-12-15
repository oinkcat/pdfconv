using System;
using System.Collections.Generic;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Part of PDF document's content
    /// </summary>
    internal class PdfObject
    {
        public int Id { get; }

        public bool IsReferenceOnly { get; set; }

        public List<string> Contents { get; }

        public PdfObject(int id)
        {
            Id = id;
            Contents = new List<string>();
        }
    }
}