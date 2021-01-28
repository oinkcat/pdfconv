using System.IO;
using System.Collections.Generic;
using Xunit;
using PdfConverter.Simple;
using PdfConverter.Simple.Primitives;
using PdfConverter.Simple.Structure;

namespace PdfConverter.Tests
{
    /// <summary>
    /// Pdf documents object structure tests
    /// </summary>
    public class PdfStructureTests
    {
        private const string TestPdfPath = "../../../TestPdf/testpdf.pdf";

        private PdfObjectRoot TestObjRoot;

        /// <summary>
        /// Test access to PDF root and pages objects
        /// </summary>
        [Fact]
        public void TestPdfCatalogAndPageCount()
        {
            var pagesRef = TestObjRoot.Catalog.GetAttributeValue<PdfArray>("Pages");
            var pagesObj = TestObjRoot.GetObjectByRef(pagesRef);

            int pagesCount = (int)pagesObj.GetAttributeValue<PdfAtom>("Count").AsNumber();

            Assert.Equal(3, pagesCount);
        }

        /// <summary>
        /// Test access to page information objects
        /// </summary>
        [Fact]
        public void TestPdfPageObjects()
        {
            var pagesRef = TestObjRoot.Catalog.GetAttributeValue<PdfArray>("Pages");
            var pagesObj = TestObjRoot.GetObjectByRef(pagesRef);

            var pageKids = pagesObj.GetAttributeValue<PdfArray>("Kids");
            Assert.NotEmpty(pageKids);

            int pagesCount = (int)pagesObj.GetAttributeValue<PdfAtom>("Count").AsNumber();
            Assert.Equal(3, pagesCount);

            for(int i = 0; i < pagesCount; i++)
            {
                var pageObj = TestObjRoot.GetObjectByRef(pageKids, i);
                string pageType = pageObj.GetAttributeValue<PdfAtom>("Type").AsString();

                Assert.Equal("Page", pageType);
            }
        }

        /// <summary>
        /// Test access to PDF resources
        /// </summary>
        [Fact]
        public void TestPdfResources()
        {
            var pagesRef = TestObjRoot.Catalog.GetAttributeValue<PdfArray>("Pages");
            var pagesObj = TestObjRoot.GetObjectByRef(pagesRef);

            var resourcesRef = pagesObj.GetAttributeValue<PdfArray>("Resources");
            var resourcesObj = TestObjRoot.GetObjectByRef(resourcesRef);

            var fontTableRef = resourcesObj.GetAttributeValue<PdfArray>("Font");
            var fontTableObj = TestObjRoot.GetObjectByRef(fontTableRef);
            Assert.NotNull(fontTableObj);

            for(int n = 1;; n++)
            {
                string fontRefAttrName = string.Concat("F", n);
                var fontRef = fontTableObj.GetAttributeValue<PdfArray>(fontRefAttrName);

                if(fontRef == null) { break; }

                var fontObj = TestObjRoot.GetObjectByRef(fontRef);

                Assert.NotNull(fontObj);
                
                string fontObjType = fontObj.GetAttributeValue<PdfAtom>("Type")?.AsString();
                Assert.Equal("Font", fontObjType);
            }
        }

        /// <summary>
        /// Test access to PDF objects which are necessary for text extraction
        /// </summary>
        [Fact]
        public void TestPdfRequiredObjects()
        {
            var pagesObj = TestObjRoot.GetObjectsByType("Pages")[0];
            var resourcesRef = pagesObj.GetAttributeValue<PdfArray>("Resources");
            var resourceObj = TestObjRoot.GetObjectByRef(resourcesRef);

            // Font objects
            var fontObjs = TestObjRoot.GetObjectsByType("Font");
            Assert.NotEmpty(fontObjs);

            foreach(var fontObj in fontObjs)
            {
                var toUnicodeRef = fontObj.GetAttributeValue<PdfArray>("ToUnicode");
                var toUnicodeObj = TestObjRoot.GetObjectByRef(toUnicodeRef);
                Assert.True(toUnicodeObj.HasStream);

                toUnicodeObj.ConvertContentToText();
                Assert.NotEmpty(toUnicodeObj.TextContent);
            }

            // Page objects
            var pageObjs = TestObjRoot.GetObjectsByType("Page");
            Assert.Equal(3, pageObjs.Count);

            foreach(var page in TestObjRoot.GetObjectsByType("Page"))
            {
                var pageResourcesRef = page.GetAttributeValue<PdfArray>("Resources");
                Assert.NotNull(pageResourcesRef);

                var pageResourceObj = TestObjRoot.GetObjectByRef(pageResourcesRef);
                Assert.Same(resourceObj, pageResourceObj);

                var pageContentsRef = page.GetAttributeValue<PdfArray>("Contents");
                Assert.NotNull(pageContentsRef);

                var contents = TestObjRoot.GetObjectByRef(pageContentsRef);
                Assert.True(contents.HasStream);
            }
        }

        public PdfStructureTests()
        {
            using var testDocStream = File.OpenRead(TestPdfPath);
            var loader = new PdfLoader(testDocStream);
            TestObjRoot = loader.Load().GetAwaiter().GetResult();
        }
    }
}