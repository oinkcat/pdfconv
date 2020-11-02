using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PdfConverter
{
    /// <summary>
    /// Base class for all converters
    /// <summary>
    public abstract class BasePdfConverter : IPdfConverter
    {
        public async Task<bool> ConvertFile(string filePath)
        {
            string realFilePath = Path.GetFullPath(filePath);

            if(await CheckCanConvert(realFilePath))
            {
                return await ConvertFileCore(filePath);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Convert valid PDF file to text
        /// <summary>
        protected abstract Task<bool> ConvertFileCore(string path);

        // Verify that file can be converted
        private async Task<bool> CheckCanConvert(string path)
        {
            const int NumBytesToTest = 4;

            try
            {
                using var fs = new FileStream(path, FileMode.Open);
                var buffer = new byte[NumBytesToTest];

                var pdfHeader = new char[] { '%', 'P', 'D', 'F' };

                return await fs.ReadAsync(buffer, 0, NumBytesToTest) switch
                {
                    NumBytesToTest => buffer.Select(b => (char)b)
                                            .SequenceEqual(pdfHeader),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }
    }
}