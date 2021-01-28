using System;
using System.Collections.Generic;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// A single PDF document's page
    /// </summary>
    public class PdfPage
    {
        private PdfDocument document;

        /// <summary>
        /// PDF page object
        /// </summary>
        public PdfObject RawObject { get; private set; }

        /// <summary>
        /// PDF page instructions
        /// </summary>
        public IList<string> RawContent => RawObject.TextContent;

        // Fill page instructions stream from PDF page object
        private void PopulatePageContent(PdfObject pageObj)
        {
            var pdfObjRoot = document.ObjectRoot;

            var pageContentRef = pageObj.GetAttributeValue<PdfArray>("Contents");
            RawObject = pdfObjRoot.GetObjectByRef(pageContentRef);

            if(!RawObject.HasStream)
            {
                throw new Exception("Page object contents is not a stream!");
            }

            RawObject.ConvertContentToText();
        }

        public PdfPage(PdfDocument document, PdfObject pageObj)
        {
            this.document = document;

            PopulatePageContent(pageObj);
        }
    }
}