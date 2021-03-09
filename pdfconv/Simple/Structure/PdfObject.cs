using System;
using System.Text;
using System.Collections.Generic;
using PdfConverter.Simple.Primitives;
using PdfConverter.Simple.StreamDecoding;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// Part of PDF document's content
    /// </summary>
    public class PdfObject
    {
        private IStreamDecoder binaryContentDecoder;

        // Object's attributes
        private PdfDictionary Attributes
        {
            get => (Content is PdfDictionary dict)
                        ? dict
                        : new PdfDictionary();
        }

        /// <summary>
        /// Object identifier
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Content term
        /// </summary>
        /// <value>Atom or container</value>
        public IPdfTerm Content { get; }

        /// <summary>
        /// Object's stream binary content
        /// </summary>
        public byte[] RawStreamContent { get; set; }

        /// <summary>
        /// Object's stream text content
        /// </summary>
        public List<string> TextContent { get; }

        /// <summary>
        /// Object body contains binary data
        /// </summary>
        public bool HasStream => RawStreamContent != null;

        /// <summary>
        /// Object type
        /// </summary>
        public string Type => GetAttributeValue<PdfAtom>("Type")?.AsString();

        /// <summary>
        /// Check if object contains specified attribute
        /// </summary>
        /// <param name="name">Attribute name to check</param>
        /// <returns>Whether or not object has attribute</returns>
        public bool HasAttribute(string name) => Attributes.ContainsKey(name);

        /// <summary>
        /// Get value of attribute with specified name
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value or null if attribute not exists</returns>
        public IPdfTerm GetAttributeValue(string name)
        {
            return Attributes.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// Get value of attribute with specified type and name
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <returns>Attribute value of desired type</returns>
        public T GetAttributeValue<T>(string name) where T : IPdfTerm
        {
            return (T)GetAttributeValue(name);
        }

        /// <summary>
        /// Return object content as term of specified type
        /// </summary>
        /// <typeparam name="T">Required term type</typeparam>
        /// <returns>Term of specified type</returns>
        public T ContentAs<T>() where T : IPdfTerm => (T)Content;

        /// <summary>
        /// Set encoded content and it's decoder
        /// </summary>
        /// <param name="contentData">Encoded content data</param>
        /// <param name="decoder">Content decoder</param>
        public void SetEncodedContent(byte[] contentData, IStreamDecoder decoder)
        {
            RawStreamContent = contentData;
            binaryContentDecoder = decoder;
        }

        /// <summary>
        /// Convert binary content to text lines
        /// </summary>
        public void ConvertContentToText()
        {
            if(TextContent.Count == 0)
            {
                var decodedContent = (binaryContentDecoder != null)
                    ? binaryContentDecoder.Decode(RawStreamContent)
                    : RawStreamContent;

                var textLines = Encoding.ASCII.GetString(decodedContent).Split('\n');
                TextContent.AddRange(textLines);
            }
        }

        /// <summary>
        /// Create temporary object from content
        /// </summary>
        /// <param name="content">Content to wrap as object</param>
        /// <returns>Content wrapper object</returns>
        public static PdfObject CreateAsWrappedContent(IPdfTerm content)
        {
            return new PdfObject(-1, content);
        }

        public PdfObject(int id, IPdfTerm content = null)
        {
            Id = id;
            Content = content;
            TextContent = new List<string>();
        }
    }
}