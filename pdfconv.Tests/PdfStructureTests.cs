using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using PdfConverter.Simple;

namespace PdfConverter.Tests
{
    /// <summary>
    /// Pdf documents loading tests
    /// </summary>
    public class PdfLoadingTests
    {
        private const string TestPdfPath = "../../../../pdfconv/TestPdf/testpdf.pdf";

        private PdfDocument TestDocument;

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

        public PdfLoadingTests()
        {
            using var testDocStream = File.OpenRead(TestPdfPath);
            var loader = new PdfLoader(testDocStream);
            TestDocument = loader.Load().GetAwaiter().GetResult();
        }
    }
}