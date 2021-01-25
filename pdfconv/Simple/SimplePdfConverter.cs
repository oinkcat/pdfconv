using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using PdfConverter.Simple.Structure;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Converts PDF filed using external program
    /// <summary>
    public class SimplePdfConverter : BasePdfConverter
    {
        private const int MaxSupportedDocMajorVersion = 1;
        private const int MaxSupportedDocMinorVersion = 4;

        private string directoryPath;
        private string baseName;

        /// <summary>
        /// Verify that specified file can be converted
        /// </summary>
        /// <param name="path">PDF file path to convert</param>
        /// <returns>Whether can convert or not</returns>
        protected override async Task<bool> CheckCanConvert(string path)
        {
            try
            {
                using var reader = File.OpenText(path);
                string signature = await reader.ReadLineAsync();
                string[] signarureParts = signature.Split('-');

                if(signarureParts[0] == "%PDF")
                {
                    var docVersion = Version.Parse(signarureParts[1]);
                    return (docVersion.Major <= MaxSupportedDocMajorVersion) &&
                           (docVersion.Minor <= MaxSupportedDocMinorVersion);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Perform PDF-to-text conversion
        /// <summary>
        protected override async Task<bool> ConvertFileCore(string path)
        {
            directoryPath = Path.GetDirectoryName(path);
            baseName = Path.GetFileNameWithoutExtension(path);

            // Do conversion itself
            using var pdfStream = File.OpenRead(path);
            var pdfObjectRoot = await new PdfLoader(pdfStream).Load();

            var textContents = ConvertToText(pdfObjectRoot);
            await SaveConvertedResult(textContents);
            
            return await Task.FromResult(true);
        }

        private IList<string> ConvertToText(PdfObjectRoot pdfRoot)
        {
            var parsedDocument = new PdfDocument(pdfRoot);
            return parsedDocument.ExtractTextContent();
        }

        private async Task SaveConvertedResult(IList<string> lines)
        {
            string outFilePath = Path.Combine(directoryPath, $"{baseName}.txt");
            await File.WriteAllLinesAsync(outFilePath, lines);
        }
    }
}