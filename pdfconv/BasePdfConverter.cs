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
                return await ConvertFileCore(realFilePath);
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

        /// <summary>
        /// Verify that file can be converted
        /// </summary>
        protected abstract Task<bool> CheckCanConvert(string path);
    }
}