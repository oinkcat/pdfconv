using System;
using System.Collections.Generic;

namespace PdfConverter.Simple
{
    /// <summary>
    /// Part of PDF document's content
    /// </summary>
    internal class PdfObject
    {
        // Object's attributes
        private Dictionary<string, object> attributes;

        /// <summary>
        /// Object identifier
        /// </summary>
        public int Id { get; }

        public bool IsReferenceOnly { get; set; }

        /// <summary>
        /// Object contents
        /// </summary>
        public List<string> Contents { get; }

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

        public PdfObject(int id, Dictionary<string, object> attrs)
        {
            Id = id;
            Contents = new List<string>();
            attributes = attrs ?? new Dictionary<string, object>();
        }
    }
}