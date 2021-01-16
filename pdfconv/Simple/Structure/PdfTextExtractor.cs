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

        private PdfFont currentFont;

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

            var tokensBuffer = new List<Token>();
            var lineBuffer = new StringBuilder();

            foreach(string contentLine in page.RawContent)
            {
                var lineTokens = tokenizer.Tokenize(contentLine);
                tokensBuffer.AddRange(lineTokens);

                if(!tokenizer.IsFullyTokenized) { continue; }

                if(textOutputStarted)
                {
                    if(tokensBuffer[0].Value as string == "ET")
                    {
                        outLines.Add(lineBuffer.ToString());
                        lineBuffer.Clear();

                        textOutputStarted = false;
                    }
                    else
                    {
                        string chunkText = GetTextFromOutputInstructions(tokensBuffer);
                        lineBuffer.Append(chunkText);
                    }
                }
                else if(tokensBuffer[0].Value as string == "BT")
                {
                    textOutputStarted = true;
                }

                tokensBuffer.Clear();
            }
        }

        private string GetTextFromOutputInstructions(IEnumerable<Token> lineTokens)
        {
            var paramTokens = new List<Token>();

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

            var tokenValues = tokens
                .Where(t => t.Type == TokenType.HexString)
                .Select(t => t.Value);

            string hexString = String.Join(String.Empty, tokenValues);
            string decodedText = font.DecodeString(hexString);
            textBuffer.Append(decodedText);

            return textBuffer.ToString();
        }

        public PdfTextExtractor(PdfDocument document)
        {
            this.document = document;
        }
    }
}