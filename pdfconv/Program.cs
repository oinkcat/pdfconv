using System;
using PdfConverter.Simple;

namespace PdfConverter
{
    public class Program
    {
        private const int ArgsCount = 1;

        static void Main(string[] args)
        {
            if(args.Length == ArgsCount)
            {
                IPdfConverter conv = new SimplePdfConverter();
                string pdfFilePath = args[0];

                Console.WriteLine($"Converting {pdfFilePath}...");
                bool success = conv.ConvertFile(pdfFilePath).Result;

                if(success)
                {
                    Console.WriteLine("Converted successfully!");
                }
                else
                {
                    Console.WriteLine("There was error during conversion!");
                }
            }
            else
            {
                Console.WriteLine("Incorrect arguments number!");
            }
        }
    }
}
