using System;
using System.Collections.Generic;

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
            PopulateFonts(pagesObj);
        }

        // Fill pages information
        private void PopulatePages(PdfObject pagesObj)
        {
            var pageRefs = pagesObj.GetAttributeValue<IList<object>>("Kids");
            int kidsCount = pageRefs.Count / 3;
            
            for(int i = 0; i < kidsCount; i++)
            {
                var kidPageObj = ObjectRoot.GetObjectByRef(pageRefs, i);

                if(kidPageObj.Type == "Page")
                {
                    Pages.Add(new PdfPage(this, kidPageObj));
                }
                else if(kidPageObj.Type == "Pages")
                {
                    // Nested pages (?)
                    PopulatePages(kidPageObj);
                }
            }
        }

        // Fill font information
        private void PopulateFonts(PdfObject pagesObj)
        {            
            var resourcesRef = pagesObj.GetAttributeValue<IList<object>>("Resources");
            var resourcesObj = ObjectRoot.GetObjectByRef(resourcesRef);
            var fontResRef = resourcesObj.GetAttributeValue<IList<object>>("Font");
            var fontResObj = ObjectRoot.GetObjectByRef(fontResRef);

            for(int i = 1;; i++)
            {
                string fontName = $"F{i}";
                var fontRef = fontResObj.GetAttributeValue<IList<object>>(fontName);

                if(fontRef == null) { break; }

                var fontObj = ObjectRoot.GetObjectByRef(fontRef);

                var toUnicodeObj = fontObj.GetAttributeValue("ToUnicode");
                if(toUnicodeObj == null)
                {
                    throw new NotImplementedException("Only Unicode is supported!");
                }

                Fonts.Add(fontName, new PdfUnicodeFont(this, fontObj));
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