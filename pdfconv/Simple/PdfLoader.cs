using System;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading.Tasks;
using PdfConverter.Simple.Parsing;

using MyReader = PdfConverter.Simple.UnbufferedStreamReader;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Loads PDF file
    /// </summary>
    internal class PdfLoader
    {
        private const string ObjStart = " obj";
        private const string ObjEnd = "endobj";
        private const string StreamStart = "stream";
        private const string AttribGroupStart = "<<";
        private const string AttribGroupEnd = ">>";

        private Stream inputFile;

        private Dictionary<int, PdfObject> objects;

        private Dictionary<int, (int, long)> references;

        public async Task<PdfDocument> Load()
        {
            // Load objects initially
            using(var reader = new MyReader(inputFile))
            {
                // Load some objects and references
                while(!reader.EndOfStream)
                {
                    string line = (await reader.ReadLineAsync()).Trim();
                    if(line.EndsWith(ObjStart))
                    {
                        int objId = int.Parse(line.Split()[0]);
                        var pdfObj = await LoadObject(objId, reader);
                        pdfObj.Contents.Add(String.Empty);
                        objects.Add(objId, pdfObj);
                    }
                }

                // Load linked objects
                await LoadReferencedObjects();
            }

            return new PdfDocument(objects.Values);
        }

        private async Task LoadReferencedObjects()
        {
            foreach(int objId in references.Keys)
            {
                (int refId, long objStartPos) = references[objId];
                int objSize = int.Parse(objects[refId].Contents[0]);
                
                inputFile.Seek(objStartPos, SeekOrigin.Begin);
                var objContents = await ReadCompressedContent(objSize);
                objContents.Add(String.Empty);
                objects[objId].Contents.AddRange(objContents);

                objects[refId].IsReferenceOnly = true;
            }
        }

        private async Task<PdfObject> LoadObject(int id, MyReader reader)
        {
            var attribParser = new AttributesParser();

            // Read object attribute group
            string attribLine = await reader.ReadLineAsync();

            if(attribLine.StartsWith(AttribGroupStart))
            {
                bool needMoreInput = false;
                do
                {
                    needMoreInput = attribParser.FeedNextChunk(attribLine);
                    if(!needMoreInput) { break; }

                    attribLine = await reader.ReadLineAsync();
                }
                while(needMoreInput);
            }

            var newObject = new PdfObject(id, attribParser.GetParsedAttributes());

            // Read object body
            if(newObject.GetAttributeValue("Filter")?.Equals("FlateDecode") ?? false)
            {
                string streamToken = await reader.ReadLineAsync();
                if(streamToken != StreamStart) { throw new InvalidDataException(); }
                
                var lengthAttribVal = newObject.GetAttributeValue("Length");

                if(lengthAttribVal is IList<object> lengthRefParams)
                {
                    // Object body should be loaded later
                    int referencedId = (int)(double)lengthRefParams[0];
                    references.Add(id, (referencedId, reader.BaseStream.Position));
                }
                else
                {
                    // Load body from binary compressed data
                    int size = (int)(double)lengthAttribVal;
                    var content = await ReadCompressedContent(size);
                    newObject.Contents.AddRange(content);
                }
            }
            else if(attribLine != AttribGroupEnd)
            {
                // Load body from string data
                for(string line = attribLine; 
                    line != ObjEnd; 
                    line = await reader.ReadLineAsync())
                {
                    newObject.Contents.Add(line);
                }
            }

            return newObject;
        }

        private async Task<IList<string>> ReadCompressedContent(int length)
        {
            var compressedBytes = new byte[length];
            await inputFile.ReadAsync(compressedBytes, 0, length);

            using var buffer = new MemoryStream(compressedBytes[2..]);
            using var decoder = new DeflateStream(buffer, CompressionMode.Decompress);
            using var decodedData = new MemoryStream(length);
            decoder.CopyTo(decodedData);

            var binaryData = decodedData.GetBuffer();
            return new List<string> { Encoding.ASCII.GetString(binaryData) };
        }

        public PdfLoader(Stream inFile)
        {
            inputFile = inFile;
            objects = new Dictionary<int, PdfObject>();
            references = new Dictionary<int, (int, long)>();
        }
    }
}