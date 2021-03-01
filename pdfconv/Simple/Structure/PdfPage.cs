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
        public List<string> RawContent { get; }

        // Fill page instructions stream from PDF page object
        private void PopulatePageContent(PdfObject pageObj)
        {
            var pdfObjRoot = document.ObjectRoot;

            var contentObj = pdfObjRoot.GetObjectFromAttrib(pageObj, "Contents");

            if(contentObj.HasStream)
            {
                AppendPageContent(contentObj);
            }
            else if(contentObj.Content is PdfArray contentObjRefs)
            {
                int numContentChunks = contentObjRefs.Count / 3;
                for(int i = 0; i < numContentChunks; i++)
                {
                    var contentChunkObj = pdfObjRoot.GetObjectByRef(contentObjRefs, i);
                    AppendPageContent(contentChunkObj);
                }
            }
            else
            {
                throw new Exception($"Page #{RawObject.Id} content not found!");
            }

        }

        // Append object contents to page contents
        private void AppendPageContent(PdfObject contentObj)
        {
            contentObj.ConvertContentToText();
            RawContent.AddRange(contentObj.TextContent);
        }

        public PdfPage(PdfDocument document, PdfObject pageObj)
        {
            this.document = document;

            RawObject = pageObj;
            RawContent = new List<string>();

            PopulatePageContent(pageObj);
        }
    }
}