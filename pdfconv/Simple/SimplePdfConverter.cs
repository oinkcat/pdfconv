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

        private IList<string> ConvertToText(PdfObjectRoot pdf)
        {
            var lines = new List<string>();

            foreach(var obj in pdf.Objects)
            {
                lines.Add($"Object ID: {obj.Id}, type: {obj.Type}");
                
                if(obj.HasStream)
                {
                    lines.Add("<< Binary Content >>");
                    lines.Add(String.Empty);
                }
                else
                {
                    lines.AddRange(obj.TextContent);
                }
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