using System;
using System.Linq;
using System.Collections.Generic;
using PdfConverter.Simple.Primitives;

namespace PdfConverter.Simple.Structure
{
    /// <summary>
    /// Contains all PDF document's content objects
    /// </summary>
    public class PdfObjectRoot
    {
        /// <summary>
        /// Pdf content objects
        /// </summary>
        public IList<PdfObject> Objects { get; }

        /// <summary>
        /// Get Pdf object by it's ID
        /// </summary>
        /// <param name="id">Object identifier</param>
        /// <returns>Pdf object with specified ID</returns>
        public PdfObject GetObjectById(int id) => Objects[id - 1];

        /// <summary>
        /// Get list of objects of specified type
        /// </summary>
        /// <param name="type">Object type to find</param>
        /// <returns>Found objects</returns>
        public IList<PdfObject> GetObjectsByType(string type)
        {
            const string TypeAttrib = "Type";

            return Objects
                .Where(o => o.GetAttributeValue<PdfAtom>(TypeAttrib)?.AsString() == type)
                .ToList();
        }

        /// <summary>
        /// Get Pdf object by reference descriptor
        /// </summary>
        /// <param name="reference">Reference descriptor</param>
        /// <param name="offset">Number of reference in list</param>
        /// <returns>Object referenced by it's descriptor</returns>
        public PdfObject GetObjectByRef(PdfArray reference, int offset = 0)
        {
            int objIdListIdx = offset * 3;
            int referencedObjId = (int)(reference[objIdListIdx] as PdfAtom).AsNumber();

            return GetObjectById(referencedObjId);
        }

        /// <summary>
        /// Get direct or indirect object from attributes dictionary
        /// </summary>
        /// <param name="parentObj">Object that contains sought object</param>
        /// <param name="keyName">Object name</param>
        /// <returns>Object by specified name</returns>
        public PdfObject GetObjectFromAttrib(PdfObject parentObj, string keyName)
        {
            var childObj = parentObj.GetAttributeValue(keyName);

            if(childObj == null)
            {
                // No nested object found with given name
                return null;
            }
            else if(childObj is PdfSequence)
            {
                // Indirect object
                return GetObjectByRef(childObj as PdfArray);
            }
            else
            {
                // Wrap value in temporary object
                return PdfObject.CreateAsWrappedContent(childObj);
            }
        }

        /// <summary>
        /// Pdf root object
        /// </summary>
        public PdfObject Catalog { get; }

        public PdfObjectRoot()
        {
            Objects = new List<PdfObject>();
        }

        public PdfObjectRoot(IEnumerable<PdfObject> objects)
        {
            Objects = objects.OrderBy(o => o.Id).ToList();
            Catalog = GetObjectsByType("Catalog").FirstOrDefault();
        }
    }
}