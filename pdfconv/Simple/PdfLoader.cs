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
                    if(line.EndsWith(ObjStart))
                    {
                        int objId = int.Parse(line.Split()[0]);
                        var pdfObj = await LoadObject(objId, reader);
                        objects.Add(objId, pdfObj);
                    }
                }

                // Load linked objects
                await LoadReferencedObjects();
            }

            return new PdfObjectRoot(objects.Values);
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
                    newObject.BinaryContent = content;
                }
            }
            else if(!attribLine.EndsWith(AttribGroupEnd))
            {
                // Load body from string data
                for(string line = attribLine; 
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