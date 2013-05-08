using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.process
{
    /// <summary>
    /// The definition of a data type.
    /// </summary>
    public class DataType : CatalogItem
    {
        /// <summary>
        /// Gets the name of the XML element representing this class of catalog items.
        /// </summary>
        /// <value>The name of the XML element representing this class of catalog items.</value>
        public override string XmlName { get { return "DataType"; } }

        /// <summary>
        /// Gets MIME type or <c>null</c>, if no MIME type is specified.
        /// </summary>
        /// <value>The MIME type of the data type.</value>
        public string MimeType { get; private set; }

        /// <summary>
        /// Loads the catalog item definition from a <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> containing the catalog item definition.</param>
        /// <exception cref="FormatException">Is thrown, if the value of the <c>guid</c> attribute
        /// in the given <see cref="XmlElement"/> is no valid <see cref="Guid"/>.</exception>
        public override void LoadFromXml(XmlElement e)
        {
            base.LoadFromXml(e);

            MimeType = e.ReadElementString("p:MimeType", null);
        }

        /// <summary>
        /// Validates an entity value or provenance parameter set with this data type definition.
        /// </summary>
        /// <param name="stream">A <see cref="System.IO.Stream"/> with the content of the value.</param>
        /// <param name="handler">A <see cref="ValidationHandler"/> for validation messages.</param>
        /// <returns></returns>
        public bool ValidateValue(System.IO.Stream stream, ValidationHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}
