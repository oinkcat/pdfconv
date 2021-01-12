using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfConverter.Simple.Parsing;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// Extracts text from PDF document's pages
    /// </summary>
    public class PdfTextExtractor
    {
        private PdfDocument document;

        /// <summary>
        /// Convert page content instuction stream to text
        /// </summary>
        /// <param name="outLines">Output text lines</param>
        public void ExtractText(IList<string> outLines)
        {
            foreach(var page in document.Pages)
            {
                ExtractTextFromPage(page, outLines);
            }
        }

        private void ExtractTextFromPage(PdfPage page, IList<string> outLines)
        {
            var tokenizer = new ContentTokenizer();
            bool textOutputStarted = false;

            foreach(string contentLine in page.RawContent)
            {
                var lineTokens = tokenizer.Tokenize(contentLine);

                if(textOutputStarted)
                {
                    if(lineTokens.First().Value as string == "ET")
                    {
                        textOutputStarted = false;
                    }
                    else
                    {
                        string lineText = GetTextFromOutputInstructions(lineTokens);
                        outLines.Add(lineText);
                    }
                }
                else if(lineTokens.First().Value as string == "BT")
                {
                    textOutputStarted = true;
                }
            }
        }

        private string GetTextFromOutputInstructions(IEnumerable<Token> lineTokens)
        {
            var paramTokens = new List<Token>();
            PdfFont currentFont = null;

            foreach(var token in lineTokens)
            {
                if(token.Type == TokenType.Id)
                {
                    string instructionName = token.Value as string;

                    if(instructionName == "Tf")
                    {
                        string fontResName = paramTokens[0].Value as string;
                        currentFont = document.Fonts[fontResName];
                    }
                    else if(instructionName == "TJ" || instructionName == "Tj")
                    {
                        return GetStringFromTextTokens(paramTokens, currentFont);
                    }

                    paramTokens.Clear();
                }
                else
                {
                    paramTokens.Add(token);
                }
            }

            throw new Exception("No text output instructions found in line!");
        }

        private string GetStringFromTextTokens(IList<Token> tokens, PdfFont font)
        {
            var textBuffer = new StringBuilder();

            foreach(var token in tokens.Where(t => t.Type == TokenType.HexString))
            {
                string decodedText = font.DecodeString(token.Value as string);
                textBuffer.Append(decodedText);
            }

            return textBuffer.ToString();
        }

        public PdfTextExtractor(PdfDocument document)
        {
            this.document = document;
        }
    }
}