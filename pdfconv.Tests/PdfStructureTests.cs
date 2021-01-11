using System.IO;
using System.Collections.Generic;
using Xunit;
using PdfConverter.Simple;
using PdfConverter.Simple.Structure;

namespace PdfConverter.Tests
{
    /// <summary>
    /// Pdf documents object structure tests
    /// </summary>
    public class PdfStructureTests
    {
        private const string TestPdfPath = "../../../../pdfconv/TestPdf/testpdf.pdf";

        private PdfObjectRoot TestDocument;

        /// <summary>
        /// Test access to PDF root and pages objects
        /// </summary>
        [Fact]
        public void TestPdfCatalogAndPageCount()
        {
            var pagesRef = TestDocument.Catalog.GetAttributeValue<List<object>>("Pages");
            var pagesObj = TestDocument.GetObjectByRef(pagesRef);

            int pagesCount = (int)pagesObj.GetAttributeValue<double>("Count");

            Assert.Equal(3, pagesCount);
        }

        /// <summary>
        /// Test access to page information objects
        /// </summary>
        [Fact]
        public void TestPdfPageObjects()
        {
            var pagesRef = TestDocument.Catalog.GetAttributeValue<List<object>>("Pages");
            var pagesObj = TestDocument.GetObjectByRef(pagesRef);

            var pageKids = pagesObj.GetAttributeValue<List<object>>("Kids");
            Assert.NotEmpty(pageKids);

            int pagesCount = (int)pagesObj.GetAttributeValue<double>("Count");
            Assert.Equal(3, pagesCount);

            for(int i = 0; i < pagesCount; i++)
            {
                var singlePageObj = TestDocument.GetObjectByRef(pageKids, i);
                string pageType = singlePageObj.GetAttributeValue<string>("Type");

                Assert.Equal("Page", pageType);
            }
        }

        /// <summary>
        /// Test access to PDF resources
        /// </summary>
        [Fact]
        public void TestPdfResources()
        {
            var pagesRef = TestDocument.Catalog.GetAttributeValue<List<object>>("Pages");
            var pagesObj = TestDocument.GetObjectByRef(pagesRef);

            var resourcesRef = pagesObj.GetAttributeValue<List<object>>("Resources");
            var resourcesObj = TestDocument.GetObjectByRef(resourcesRef);

            var fontTableRef = resourcesObj.GetAttributeValue<List<object>>("Font");
            var fontTableObj = TestDocument.GetObjectByRef(fontTableRef);
            Assert.NotNull(fontTableObj);

            for(int n = 1;; n++)
            {
                string fontRefAttrName = string.Concat("F", n);
                var fontRef = fontTableObj.GetAttributeValue<List<object>>(fontRefAttrName);

                if(fontRef == null) { break; }

                var fontObj = TestDocument.GetObjectByRef(fontRef);

                Assert.NotNull(fontObj);
                Assert.Equal("Font", fontObj.GetAttributeValue<string>("Type"));
            }
        }

        /// <summary>
        /// Test access to PDF objects which are necessary for text extraction
        /// </summary>
        [Fact]
        public void TestPdfRequiredObjects()
        {
            var pagesObj = TestDocument.GetObjectsByType("Pages")[0];
            var resourcesRef = pagesObj.GetAttributeValue<List<object>>("Resources");
            var resourceObj = TestDocument.GetObjectByRef(resourcesRef);

            // Font objects
            var fontObjs = TestDocument.GetObjectsByType("Font");
            Assert.NotEmpty(fontObjs);

            foreach(var fontObj in fontObjs)
            {
                var toUnicodeRef = fontObj.GetAttributeValue<List<object>>("ToUnicode");
                var toUnicodeObj = TestDocument.GetObjectByRef(toUnicodeRef);
                Assert.True(toUnicodeObj.HasStream);

                toUnicodeObj.ConvertContentToText();
                Assert.NotEmpty(toUnicodeObj.TextContent);
            }

            // Page objects
            var pageObjs = TestDocument.GetObjectsByType("Page");
            Assert.Equal(3, pageObjs.Count);

            foreach(var page in TestDocument.GetObjectsByType("Page"))
            {
                var pageResourcesRef = page.GetAttributeValue<List<object>>("Resources");
                Assert.NotNull(pageResourcesRef);

                var pageResourceObj = TestDocument.GetObjectByRef(pageResourcesRef);
                Assert.Same(resourceObj, pageResourceObj);

                var pageContentsRef = page.GetAttributeValue<List<object>>("Contents");
                Assert.NotNull(pageContentsRef);

                var contents = TestDocument.GetObjectByRef(pageContentsRef);
                Assert.True(contents.HasStream);
            }
        }

        public PdfStructureTests()
        {
            using var testDocStream = File.OpenRead(TestPdfPath);
            var loader = new PdfLoader(testDocStream);
            TestDocument = loader.Load().GetAwaiter().GetResult();
        }
    }
}