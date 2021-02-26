using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using PdfConverter.Simple.Parsing;
using PdfConverter.Simple.Primitives;
using PdfConverter.Simple.Structure;
using PdfConverter.Simple.StreamDecoding;

using MyReader = PdfConverter.Simple.UnbufferedStreamReader;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Loads PDF file
    /// </summary>
    public class PdfLoader
    {
        private const string ObjStart = "obj";
        private const string ObjEnd = "endobj";
        private const string StreamStart = "stream";
        private const string StreamEnd = "endstream";
        private const string XrefStart = "xref";
        private const string PdfEof = "%%EOF";

        private MyReader reader;

        private ObjectParser parser;

        private Dictionary<int, PdfObject> objects;

        private Dictionary<int, (int, long)> references;

        /// <summary>
        /// Load all PDF file objects
        /// </summary>
        /// <returns>PDF object structure</returns>
        public async Task<PdfObjectRoot> Load()
        {
            // Load objects initially
            using(reader)
            {
                // Load objects and references
                do
                {
                    var pdfObject = await ReadNextObject();

                    if(pdfObject != null && !objects.ContainsKey(pdfObject.Id))
                    {
                        objects.Add(pdfObject.Id, pdfObject);
                    }
                }
                while(!reader.EndOfStream);

                // Load linked objects
                await LoadReferencedObjects();
            }

            return new PdfObjectRoot(objects.Values);
        }

        private async Task<PdfObject> ReadNextObject()
        {
            int objId = await ReadObjectId();
            return (objId > 0)
                ? await ReadObjectBody(objId)
                : null;
        }

        private async Task<int> ReadObjectId()
        {
            var numberParts = new List<double>();
            bool idParsed = false;

            while(!idParsed)
            {
                var term = parser.ReadSingleObject();

                if(term is PdfAtom atomTerm)
                {
                    if(atomTerm.Type == TokenType.Number)
                    {
                        numberParts.Add((double)atomTerm.Value);
                    }
                    else if(atomTerm.Value as string == ObjStart)
                    {
                        if(numberParts.Count == 2)
                        {
                            idParsed = true;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else if(atomTerm.Value as string == XrefStart)
                    {
                        // Skip Xref and trailer
                        while(await reader.ReadLineAsync() != PdfEof) { }
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if(term is null)
                {
                    return -1;
                }
            }

            return (int)numberParts[0];
        }

        private async Task<PdfObject> ReadObjectBody(int id) {
            var contentTerm = parser.ReadSingleObject();
            bool hasStream = false;
            bool objEnded = false;

            (hasStream, objEnded) = CheckIfStreamStartOrObjEnd(contentTerm);

            if(objEnded)
            {
                // Object is empty
                return new PdfObject(id);
            }

            var newObject = new PdfObject(id, contentTerm);
            
            if(!hasStream)
            {
                // Stream may follow object attributes
                var followingTerm = parser.ReadSingleObject();
                (hasStream, objEnded) = CheckIfStreamStartOrObjEnd(followingTerm);
            }

            if(hasStream)
            {
                var filterName = newObject.GetAttributeValue<PdfAtom>("Filter")?.AsString();
                
                if(filterName != null && DecodersFactory.Instance.HasDecoder(filterName))
                {
                    var lengthAttrVal = newObject.GetAttributeValue("Length");

                    if(lengthAttrVal is PdfSequence refSeq)
                    {
                        // Object body should be loaded later
                        int referencedId = (int)(refSeq[0] as PdfAtom).AsNumber();
                        references.Add(id, (referencedId, reader.BaseStream.Position));
                    }
                    else if(lengthAttrVal is PdfAtom directLength)
                    {
                        // Load body from binary compressed data
                        int size = (int)directLength.AsNumber();
                        var content = await ReadCompressedContent(filterName, size);
                        newObject.BinaryContent = content;
                    }
                    else
                    {
                        throw new Exception("Invalid length attribute value");
                    }

                    // Go to object's end
                    while(await reader.ReadLineAsync() != ObjEnd) { }
                }
                else
                {
                    // Read uncompressed content
                    string bodyLine = await reader.ReadLineAsync();
                    while(bodyLine != ObjEnd)
                    {
                        newObject.TextContent.Add(bodyLine);
                        bodyLine = await reader.ReadLineAsync();
                    }
                }
            }

            return newObject;
        }

        private (bool, bool) CheckIfStreamStartOrObjEnd(IPdfTerm term)
        {
            bool isStreamStart = false;
            bool isObjectEnd = false;

            if(term is PdfAtom keywordId && keywordId.Type == TokenType.Id)
            {
                string keyword = keywordId.Value as string;

                if(keyword == ObjEnd)
                {
                    isObjectEnd = true;
                }
                else if(keyword == StreamStart)
                {
                    isStreamStart = true;
                }
            }

            return (isStreamStart, isObjectEnd);
        }

        // Read and decompress stream contents using given compression filter
        private async Task<byte[]> ReadCompressedContent(string filterName, int length)
        {
            var compressedBytes = new byte[length];
            await reader.BaseStream.ReadAsync(compressedBytes, 0, length);

            var decompressor = DecodersFactory.Instance.GetDecoder(filterName);
            return decompressor.Decode(compressedBytes);
        }

        private async Task LoadReferencedObjects()
        {
            foreach(int objId in references.Keys)
            {
                (int refId, long objStartPos) = references[objId];
                int objSize = (int)(double)objects[refId].ContentAs<PdfAtom>().Value;
                
                reader.BaseStream.Seek(objStartPos, SeekOrigin.Begin);
                var pdfObj = objects[objId];
                string filterName = pdfObj.GetAttributeValue<PdfAtom>("Filter").AsString();
                var objContent = await ReadCompressedContent(filterName, objSize);
                objects[objId].BinaryContent = objContent;
            }
        }

        public PdfLoader(Stream inFile)
        {
            reader = new MyReader(inFile);
            parser = new ObjectParser(TokenStreamer.CreateFromReader(reader));

            objects = new Dictionary<int, PdfObject>();
            references = new Dictionary<int, (int, long)>();
        }
    }
}