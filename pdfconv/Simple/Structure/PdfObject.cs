using System;
using System.Text;
using System.Collections.Generic;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// Part of PDF document's content
    /// </summary>
    public class PdfObject
    {
        // Object's attributes
        private Dictionary<string, object> attributes;

        /// <summary>
        /// Object identifier
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Object's binary content
        /// </summary>
        /// <value></value>
        public byte[] BinaryContent { get; set; }

        /// <summary>
        /// Object's text content
        /// </summary>
        public List<string> TextContent { get; }

        /// <summary>
        /// Object body contains binary data
        /// </summary>
        public bool HasStream => BinaryContent != null;

        /// <summary>
        /// Object type
        /// </summary>
        public string Type => GetAttributeValue("Type") as string;

        /// <summary>
        /// Check if object contains specified attribute
        /// </summary>
        /// <param name="name">Attribute name to check</param>
        /// <returns>Whether or not object has attribute</returns>
        public bool HasAttribute(string name) => attributes.ContainsKey(name);

        /// <summary>
        /// Get value of attribute with specified name
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value or null if attribute not exists</returns>
        public object GetAttributeValue(string name)
        {
            return attributes.TryGetValue(name, out object value) ? value : null;
        }

        /// <summary>
        /// Get value of attribute with specified type and name
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <returns>Attribute value of desired type</returns>
        public T GetAttributeValue<T>(string name)
        {
            return (T)GetAttributeValue(name);
        }

        /// <summary>
        /// Convert binary content to text lines
        /// </summary>
        public void ConvertContentToText()
        {
            var textLines = Encoding.ASCII.GetString(BinaryContent).Split('\n');
            TextContent.AddRange(textLines);
        }

        public PdfObject(int id, Dictionary<string, object> attrs)
        {
            Id = id;
            TextContent = new List<string>();
            attributes = attrs ?? new Dictionary<string, object>();
        }
    }
}