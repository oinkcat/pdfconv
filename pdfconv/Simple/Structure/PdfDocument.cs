using System;
using System.Collections.Generic;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// Represents PDF document
    /// </summary>
    public class PdfDocument
    {
        /// <summary>
        /// PDF objects root
        /// </summary>
        public PdfObjectRoot ObjectRoot { get; private set; }

        /// <summary>
        /// Document's pages
        /// </summary>
        public IList<PdfPage> Pages { get; private set; }

        /// <summary>
        /// Document's fonts
        /// </summary>
        public IDictionary<string, PdfFont> Fonts { get; private set; }

        /// <summary>
        /// Extract text from PDF document's pages
        /// </summary>
        /// <returns>Extracted text lines</returns>
        public List<string> ExtractTextContent()
        {
            var extractedTextLines = new List<string>();
            var textExtractor = new PdfTextExtractor(this);
            textExtractor.ExtractText(extractedTextLines);

            return extractedTextLines;
        }

        // Fill document contents from PDF root object
        private void PopulateContents()
        {
            var pagesObj = ObjectRoot.GetObjectsByType("Pages")[0];

            PopulatePages(pagesObj);
        }

        // Fill pages information
        private void PopulatePages(PdfObject pagesObj)
        {
            var pageRefs = pagesObj.GetAttributeValue<PdfArray>("Kids");
            int kidsCount = pageRefs.Count / 3;
            
            // Nested page elements
            for(int i = 0; i < kidsCount; i++)
            {
                var kidPageObj = ObjectRoot.GetObjectByRef(pageRefs, i);

                if(kidPageObj.Type == "Page")
                {
                    Pages.Add(new PdfPage(this, kidPageObj));

                    // Font resources in Page obj
                    PopulateFonts(kidPageObj);
                }
                else if(kidPageObj.Type == "Pages")
                {
                    // Nested pages (?)
                    PopulatePages(kidPageObj);
                }
            }

            // Font resources in Pages obj
            PopulateFonts(pagesObj);
        }

        // Fill font information
        private void PopulateFonts(PdfObject containerObj)
        {            
            var resourcesObj = ObjectRoot.GetObjectFromAttrib(containerObj, "Resources");
            if(resourcesObj == null) { return; }

            var fontResObj = ObjectRoot.GetObjectFromAttrib(resourcesObj, "Font");

            foreach(var kv in fontResObj.ContentAs<PdfDictionary>())
            {
                if(Fonts.ContainsKey(kv.Key)) { continue; }

                var fontObj = ObjectRoot.GetObjectByRef(kv.Value as PdfSequence);

                var toUnicodeObj = fontObj.GetAttributeValue("ToUnicode");
                var pdfFontResource = (toUnicodeObj == null)
                    ? new PdfStandardFont(this, fontObj) as PdfFont
                    : new PdfUnicodeFont(this, fontObj);

                Fonts.Add(kv.Key, pdfFontResource);
            }
        }

        public PdfDocument(PdfObjectRoot pdfRoot)
        {
            ObjectRoot = pdfRoot;
            Pages = new List<PdfPage>();
            Fonts = new Dictionary<string, PdfFont>();

            PopulateContents();
        }
    }
}