using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Converts PDF filed using external program
    /// <summary>
    internal class SimplePdfConverter : BasePdfConverter
    {
        private string directoryPath;
        private string baseName;

        /// <summary>
        /// Perform PDF-to-text conversion
        /// <summary>
        protected override async Task<bool> ConvertFileCore(string path)
        {
            directoryPath = Path.GetDirectoryName(path);
            baseName = Path.GetFileNameWithoutExtension(path);

            // Do conversion itself
            using var pdfStream = File.OpenRead(path);
            var pdfFile = await new PdfLoader(pdfStream).Load();

            var textContents = ConvertToText(pdfFile);
            await SaveConvertedResult(textContents);
            
            return await Task.FromResult(true);
        }

        private IList<string> ConvertToText(PdfDocument pdf)
        {
            var lines = new List<string>();

            foreach(var obj in pdf.Objects)
            {
                if(obj.IsReferenceOnly) { continue; }
                
                lines.Add($"Object ID: {obj.Id}");
                lines.AddRange(obj.Contents);
            }

            return lines;
        }

        private async Task SaveConvertedResult(IList<string> lines)
        {
            string outFilePath = Path.Combine(directoryPath, $"{baseName}.txt");
            await File.WriteAllLinesAsync(outFilePath, lines);
        }
    }
}