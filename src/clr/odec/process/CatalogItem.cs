using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.process
{
    /// <summary>
    /// The base class for catalog items.
    /// </summary>
    public abstract class CatalogItem
    {
        /// <summary>
        /// The global unique identifier for the catalog item.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the human recognizable name of the catalog item.
        /// </summary>
        /// <value>The name of the catalog item.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets an unstructured text in natural language, describing the catalog item.
        /// </summary>
        /// <value>The description of the catalog item.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the name of the XML element representing this class of catalog items.
        /// </summary>
        /// <value>The name of the XML element representing this class of catalog items.</value>
        public abstract string XmlName { get; }

        /// <summary>
        /// Loads the catalog item definition from a <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> containing the catalog item definition.</param>
        /// <exception cref="FormatException">Is thrown, if the value of the <c>guid</c> attribute
        /// in the given <see cref="XmlElement"/> is no valid <see cref="Guid"/>.</exception>
        public virtual void LoadFromXml(XmlElement e)
        {
            if (e == null) throw new ArgumentNullException("e");
            if (e.HasAttribute("guid"))
            {
                var idStr = e.GetAttribute("guid");
                try
                {
                    Id = new Guid(idStr);
                }
                catch (Exception)
                {
                    throw new FormatException(Resources.CatalogItem_LoadFromXml_FormatException_InvalidGuid);
                }
            }
            Name = e.ReadElementString("p:Name", null);
            Description = e.ReadElementString("p:Description", null);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0:D} : {1}", Id, Name);
        }

    }
}
