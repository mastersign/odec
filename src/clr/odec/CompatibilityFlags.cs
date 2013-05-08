using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec
{
    /// <summary>
    /// This class stores compatibility settings for different incarnations of the container format.
    /// </summary>
    public struct CompatibilityFlags
    {
        /// <summary>
        /// Gets or sets a value to control XML canonicalization for the container structure XML documents
        /// (index.xml, history.xml, edition.xml, entity.xml) on or off.
        /// The default is <c>false</c>, which means XML canonicalization is used.
        /// Setting to <c>true</c> will suppress the XML canonicalization while creating the signatures.
        /// </summary>
        /// <value><c>true</c> if XML canonicalization of container structure XML documents is suppressed.</value>
        public bool SuppressStructureXmlCanonicalization { get; set; }

        /// <summary>
        /// Gets or sets a value to control written form of XML signatures.
        /// <c>true</c> means that XML signature elements are written without indentation and canonicalized,
        /// <c>false</c> means that XML signature elements are written as uncanonicalized and indented XML.
        /// <seealso cref="System.Security.Cryptography.Xml.SignedInfo"/>
        /// <seealso cref="System.Security.Cryptography.Xml.Signature"/>
        /// </summary>
        public bool WriteXmlSignatureCanonicalized { get; set; }

        /// <summary>
        /// Gets the default flags for container format specification conform processing.
        /// </summary>
        public static CompatibilityFlags DefaultFlags
        {
            get
            {
                return new CompatibilityFlags
                    {
                        SuppressStructureXmlCanonicalization = false,
                        WriteXmlSignatureCanonicalized = false,
                    };
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var flags = new List<string>();
            if (SuppressStructureXmlCanonicalization)
            {
                flags.Add(Resources.CompatibilityFlags_SupressStructureXMLCanonicalization);
            }
            if (WriteXmlSignatureCanonicalized)
            {
                flags.Add(Resources.CompatibilityFlags_WriteXmlSignatureCanonicalized);
            }

            return flags.Count > 0
                       ? string.Join(CultureInfo.CurrentUICulture.TextInfo.ListSeparator + " ", flags.ToArray())
                       : Resources.CompatibilityFlags_None;
        }
    }
}
