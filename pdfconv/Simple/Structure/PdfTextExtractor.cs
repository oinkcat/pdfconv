using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfConverter.Simple.Parsing;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// Extracts text from PDF document's pages
    /// </summary>
    public class PdfTextExtractor
    {
        private PdfDocument document;

        private PdfFont currentFont;

        private bool textOutputStarted;

        private StringBuilder lineBuffer;

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

            var tokensBuffer = new List<Token>();

            foreach(string contentLine in page.RawContent)
            {
                var lineTokens = tokenizer.Tokenize(contentLine);
                tokensBuffer.AddRange(lineTokens);

                if(!tokenizer.IsFullyTokenized) { continue; }

                if(textOutputStarted)
                {
                    if(tokensBuffer.FirstOrDefault()?.Value as string == "ET")
                    {
                        outLines.Add(lineBuffer.ToString());
                        lineBuffer.Clear();

                        textOutputStarted = false;
                    }
                    else
                    {
                        string chunkText = GetTextFromOutputInstructions(tokensBuffer);

                        if(chunkText != null)
                        {
                            lineBuffer.Append(chunkText);
                        }
                    }
                }
                else if(tokensBuffer.FirstOrDefault()?.Value as string == "BT")
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

            return null;
        }

        private string GetStringFromTextTokens(IList<Token> tokens, PdfFont font)
        {
            var stringTokens = tokens.Where(t => {
                return t.Type == TokenType.HexString || t.Type == TokenType.String;
            });

            if(stringTokens.Any())
            {
                var textBuffer = new StringBuilder();

                foreach(var token in stringTokens)
                {
                    if(token.Type == TokenType.String)
                    {
                        textBuffer.Append(token.Value as string);
                    }
                    else
                    {
                        string decodedText = font.DecodeString(token.Value as string);
                        textBuffer.Append(decodedText);
                    }
                }

                return textBuffer.ToString();
            }
            else
            {
                return null;
            }
        }

        public PdfTextExtractor(PdfDocument document)
        {
            this.document = document;
            lineBuffer = new StringBuilder();
        }
    }
}