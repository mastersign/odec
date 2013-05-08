using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// The standard implementation for <see cref="IXmlCanonicalizer"/> 
    /// using BCL implementation the of the W3C recommended C14N canonization method 
    /// without regarding comments.
    /// </summary>
    public class BclC14NCanonicalizer : IXmlCanonicalizer
    {
        #region Implementation of IXmlCanonizer

        /// <summary>
        /// Transforms the specified XML element with the W3C C14N method and returns the result as an octet stream.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <returns>The canonicalized XML as an octet stream.</returns>
        public Stream Canonize(XmlElement element)
        {
            var transform = new XmlDsigC14NTransform();
            transform.Algorithm = CanonicalizationMethod;
            if (!transform.InputTypes.Any(t => t == typeof (XmlNodeList)))
            {
                throw new NotSupportedException(
                    Resources.BclC14NCanonicalizer_Canonize_NotSupported_XmlDocumentAsInput);
            }
            if (!transform.OutputTypes.Any(t => t == typeof (Stream)))
            {
                throw new NotSupportedException(
                    Resources.BclC14NCanonicalizer_Canonize_NotSupported_StreamAsOutput);
            }

            string securityUrl = null; // Containing Document BaseURI
            XmlResolver xmlResolver = new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
            var document = new XmlDocument();
            document.PreserveWhitespace = true;
            using (var reader = new StringReader(element.OuterXml))
            {
                var settings = new XmlReaderSettings
                                   {
                                       XmlResolver = xmlResolver,
                                       DtdProcessing = DtdProcessing.Ignore,
                                   };
                var reader2 = XmlReader.Create(reader, settings, securityUrl);
                document.Load(reader2);
            }

            transform.LoadInput(document);
            return (Stream) transform.GetOutput();
        }

        /// <summary>
        /// Gets a <see cref="String"/>, identifying the canonicalization method.
        /// </summary>
        /// <value>The canonicalization method <c>http://www.w3.org/TR/2001/REC-xml-c14n-20010315</c>.</value>
        public string CanonicalizationMethod
        {
            get { return SignedXml.XmlDsigC14NTransformUrl; }
        }

        #endregion
    }
}