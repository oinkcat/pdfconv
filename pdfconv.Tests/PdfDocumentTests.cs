using System.IO;
using System.Collections.Generic;
using Xunit;
using PdfConverter.Simple;
using PdfConverter.Simple.Structure;

namespace PdfConverter.Tests
{
    /// <summary>
    /// Parsed PDF document tests
    /// </summary>
    public class PdfDocumentTests
    {
        private const string TestPdfPath = "../../../../pdfconv/TestPdf/testpdf.pdf";

        // Loaded PDF object root
        private PdfObjectRoot TestObjRoot;

        /// <summary>
        /// Test parsed PDF document objects
        /// </summary>
        [Fact]
        public void TestPdfPages()
        {
            var pdfDoc = new PdfDocument(TestObjRoot);

            Assert.NotEmpty(pdfDoc.Pages);
            Assert.NotEmpty(pdfDoc.Fonts);
        }

        /// <summary>
        /// Test text extraction from document's pages
        /// </summary>
        [Fact]
        public void TestPdfDocumentTextExtraction()
        {
            var pdfDoc = new PdfDocument(TestObjRoot);
            var textLines = pdfDoc.ExtractTextContent();

            Assert.NotEmpty(textLines);
            Assert.NotNull(textLines[0]);
        }

        public PdfDocumentTests()
        {
            using var testDocStream = File.OpenRead(TestPdfPath);
            var loader = new PdfLoader(testDocStream);
            TestObjRoot = loader.Load().GetAwaiter().GetResult();
        }
    }
}