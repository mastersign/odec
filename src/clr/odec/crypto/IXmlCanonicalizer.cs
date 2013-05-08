using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// Represents the capability to canonicalize an XML element.
    /// </summary>
    public interface IXmlCanonicalizer
    {
        /// <summary>
        /// Transforms the specified XML element with a canonicalization method and returns the result as an octet stream.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <returns>The canonicalized XML as an octet stream.</returns>
        Stream Canonize(XmlElement element);

        /// <summary>
        /// Gets a <see cref="String"/>, identifying the canonicalization method.
        /// </summary>
        /// <remarks>
        /// For the W3C recommended canonicalization C14N, for example, the URI
        /// <c>http://www.w3.org/TR/2001/REC-xml-c14n-20010315</c> should be returned.
        /// </remarks>
        /// <value>The canonicalization method.</value>
        string CanonicalizationMethod { get; }
    }
}