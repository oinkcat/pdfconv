using System;
using System.Linq;
using System.Collections.Generic;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Represents document's content
    /// </summary>
    internal class PdfDocument
    {
        public IList<PdfObject> Objects { get; }

        public PdfObject GetObjectById(int id) => Objects.Single(obj => obj.Id == id);

        public PdfDocument()
        {
            Objects = new List<PdfObject>();
        }

        public PdfDocument(IEnumerable<PdfObject> objects)
        {
            Objects = objects.ToList();
        }
    }
}