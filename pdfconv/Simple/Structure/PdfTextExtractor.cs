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

        private StringBuilder lineBuffer;

        private IList<string> outputLines;

        /// <summary>
        /// Convert page content instuction stream to text
        /// </summary>
        /// <param name="outLines">Output text lines</param>
        public void ExtractText(IList<string> outLines)
        {
            outputLines = outLines;

            foreach(var page in document.Pages)
            {
                ExtractTextFromPage(page);
            }
        }

        private void ExtractTextFromPage(PdfPage page)
        {
            var streamer = TokenStreamer.CreateFromList(page.RawContent);
            var parser = new ObjectParser(streamer);

            PdfAtom command;
            var paramList = new List<IPdfTerm>();

            do
            {
                command = parser.ReadNextCommand(paramList);

                if(command != null)
                {
                    InterpretPdfCommand(command.AsString(), paramList);
                }

                paramList.Clear();
            }
            while(command != null);
        }

        private void InterpretPdfCommand(string commandName, IList<IPdfTerm> cmdParams)
        {
            switch(commandName)
            {
                case "BT":
                    // ?
                    break;
                case "ET":
                    AppendCurrentTextLine();
                    break;
                case "Tf":
                    SetCurrentFont(cmdParams);
                    break;
                case "Tj":
                case "TJ":
                    OutputText(cmdParams[0]);
                    break;
            }
        }

        private void AppendCurrentTextLine()
        {
            outputLines.Add(lineBuffer.ToString());
            lineBuffer.Clear();
        }

        private void SetCurrentFont(IList<IPdfTerm> fontSpec)
        {
            string fontResName = (fontSpec[0] as PdfAtom).AsString();
            currentFont = document.Fonts[fontResName];
        }

        private void OutputText(IPdfTerm textTerms)
        {
            var textTermsList = textTerms switch {
                PdfArray termsArray => textTerms as PdfArray,
                PdfAtom textLiteral => new PdfArray { textLiteral },
                _ => throw new Exception("Invalid term type")
            };

            var stringAtoms = textTermsList.Where(t => {
                var atom = t as PdfAtom;
                return atom.Type == TokenType.HexString || atom.Type == TokenType.String;
            });
            
            if(stringAtoms.Any())
            {
                foreach(var atomTerm in stringAtoms)
                {
                    var atom = atomTerm as PdfAtom;

                    if(atom.Type == TokenType.String)
                    {
                        lineBuffer.Append(atom.AsString());
                    }
                    else
                    {
                        string decodedText = currentFont.DecodeString(atom.AsString());
                        lineBuffer.Append(decodedText);
                    }
                }
            }
        }

        public PdfTextExtractor(PdfDocument document)
        {
            this.document = document;
            lineBuffer = new StringBuilder();
        }
    }
}