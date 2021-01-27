using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading.Tasks;
using PdfConverter.Simple.Parsing;
using PdfConverter.Simple.Structure;

using MyReader = PdfConverter.Simple.UnbufferedStreamReader;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Loads PDF file
    /// </summary>
    public class PdfLoader
    {
        private const string ObjStart = " obj";
        private const string ObjEnd = "endobj";
        private const string StreamStart = "stream";
        private const string AttribGroupStart = "<<";
        private const string AttribGroupEnd = ">>";

        private Stream inputFile;

        private Dictionary<int, PdfObject> objects;

        private Dictionary<int, (int, long)> references;

        public async Task<PdfObjectRoot> Load()
        {
            // Load objects initially
            using(var reader = new MyReader(inputFile))
            {
                // Load some objects and references
                while(!reader.EndOfStream)
                {
                    string line = (await reader.ReadLineAsync()).Trim();
                    // Skip comments
                    if(line.Length == 0 || line.StartsWith("%")) { continue; }

                    var streamer = new TokenStreamer(line);

                    int objId = ReadObjectId(streamer);
                    if(objId > 0)
                    {
                        var pdfObj = await LoadObject(objId, streamer, reader);
                        if(!objects.ContainsKey(objId))
                        {
                            objects.Add(objId, pdfObj);
                        }
                    }
                }

                // Load linked objects
                await LoadReferencedObjects();
            }

            return new PdfObjectRoot(objects.Values);
        }

        private int ReadObjectId(TokenStreamer streamer)
        {
            var numberParts = new List<double>();
            bool idParsed = false;

            while(!idParsed)
            {
                var currentToken = streamer.GetNextToken();

                if(currentToken.Type == TokenType.Number)
                {
                    numberParts.Add((double)currentToken.Value);
                }
                else if(currentToken.Type == TokenType.Id)
                {
                    if((currentToken.Value as string).Equals("obj"))
                    {
                        idParsed = true;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }

            return (int)numberParts[0];
        }

        private async Task LoadReferencedObjects()
        {
            foreach(int objId in references.Keys)
            {
                (int refId, long objStartPos) = references[objId];
                int objSize = int.Parse(objects[refId].TextContent[0]);
                
                inputFile.Seek(objStartPos, SeekOrigin.Begin);
                var objContent = await ReadCompressedContent(objSize);
                objects[objId].BinaryContent = objContent;
            }
        }

        private async Task<PdfObject> LoadObject (
            int id, 
            TokenStreamer streamer, 
            MyReader reader
        ) {
            var attribParser = new AttributesParser(streamer);

            // Read object attribute group
            string startLine = null;
            Token nextToken;

            do
            {
                if(streamer.AtEndOfLine)
                {
                    startLine = await reader.ReadLineAsync();
                    streamer.SetSourceLine(startLine);
                }
                nextToken = streamer.GetNextToken();
            }
            while(nextToken.Type == TokenType.EndOfLine);

            streamer.PushBackToken(nextToken);

            if(nextToken.Type == TokenType.DictStart)
            {
                bool needMoreInput = false;
                do
                {
                    if(streamer.AtEndOfLine)
                    {
                        streamer.SetSourceLine(await reader.ReadLineAsync());
                    }

                    needMoreInput = attribParser.FeedNextChunk(null);
                    if(!needMoreInput) { break; }
                }
                while(needMoreInput);
            }

            var newObject = new PdfObject(id, attribParser.GetParsedAttributes());

            // Read object body
            if(newObject.GetAttributeValue("Filter")?.Equals("FlateDecode") ?? false)
            {
                var streamToken = streamer.GetNextToken();

                if(streamer.AtEndOfLine)
                {
                    streamer.SetSourceLine(await reader.ReadLineAsync());
                    streamToken = streamer.GetNextToken();
                }

                if(!(streamToken.Value as string).Equals(StreamStart))
                {
                    throw new InvalidDataException();
                }
                
                var lengthAttribVal = newObject.GetAttributeValue("Length");

                if(lengthAttribVal is IList<object> lengthRefParams)
                {
                    // Object body should be loaded later
                    int referencedId = (int)(double)lengthRefParams[0];
                    references.Add(id, (referencedId, reader.BaseStream.Position));

                    // Skip object's content
                    string bodyLine = startLine;
                    while(bodyLine != ObjEnd)
                    {
                        bodyLine = await reader.ReadLineAsync();
                    }
                }
                else
                {
                    // Load body from binary compressed data
                    int size = (int)(double)lengthAttribVal;
                    var content = await ReadCompressedContent(size);
                    newObject.BinaryContent = content;
                }
            }
            else
            {
                // Load body from string data
                for(string line = startLine;
                    line != ObjEnd; 
                    line = await reader.ReadLineAsync())
                {
                    newObject.TextContent.Add(line);
                }
            }

            return newObject;
        }

        private async Task<byte[]> ReadCompressedContent(int length)
        {
            var compressedBytes = new byte[length];
            await inputFile.ReadAsync(compressedBytes, 0, length);

            using var buffer = new MemoryStream(compressedBytes[2..]);
            using var decoder = new DeflateStream(buffer, CompressionMode.Decompress);
            using var decodedData = new MemoryStream(length);
            decoder.CopyTo(decodedData);

            return decodedData.GetBuffer();
        }

        public PdfLoader(Stream inFile)
        {
            inputFile = inFile;
            objects = new Dictionary<int, PdfObject>();
            references = new Dictionary<int, (int, long)>();
        }
    }
}