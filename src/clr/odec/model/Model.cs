using System.IO;
using System.Xml;
using System.Xml.Schema;
using Commons.Xml.Relaxng;
using de.mastersign.odec.Properties;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// This static class contains tools to bind the object model to the XML representation.
    /// </summary>
    public static class Model
    {
        /// <summary>
        /// The XML namespace of W3C XML signatures.
        /// </summary>
        public const string XMLSIGD_NS = "http://www.w3.org/2000/09/xmldsig#";

        /// <summary>
        /// Gets the target namespace of the container XML-Schema.
        /// </summary>
        /// <value>The container namespace.</value>
        public static string ContainerNamespace
        {
            get { return "http://www.mastersign.de/odec/container/"; }
        }

        /// <summary>
        /// Gets the target namespace of the profile XML-Schema.
        /// </summary>
        /// <value>The profile namespace.</value>
        public static string ProfileNamespace
        {
            get { return "http://www.mastersign.de/odec/profile/"; }
        }

        private static XmlResolver resolver;

        /// <summary>
        /// Gets a <see cref="XmlResolver"/> which logs every resolution.
        /// </summary>
        public static XmlResolver LoggingResolver
        {
            get
            {
                if (resolver == null)
                {
                    resolver = new XmlUtils.LoggingXmlResolver(new XmlUrlResolver());
                }
                return resolver;
            }
        }

        private static RelaxngPattern xmlSigSchema;
        private static RelaxngPattern containerSchema;
        private static RelaxngPattern profileSchema;

        /// <summary>
        /// Gets a compiled version of the container XML-Schema.
        /// </summary>
        /// <value>The schema.</value>
        public static RelaxngPattern ContainerSchema
        {
            get
            {
                if (containerSchema == null) LoadSchemas();
                return containerSchema;
            }
        }

        /// <summary>
        /// Gets a compiled version of the profile XML-Schema.
        /// </summary>
        /// <value>The schema.</value>
        public static RelaxngPattern ProfileSchema
        {
            get
            {
                if (profileSchema == null) LoadSchemas();
                return profileSchema;
            }
        }

        /// <summary>
        /// Gets a compiled version of the XML signature Schema.
        /// </summary>
        /// <value>The schema.</value>
        public static RelaxngPattern XmlSignatureSchema
        {
            get
            {
                if (xmlSigSchema == null) LoadSchemas();
                return xmlSigSchema;
            }
        }

        private static void LoadSchemas()
        {
            xmlSigSchema = LoadPattern(Resources.XmldsigCoreSchemaRelaxNg);
            containerSchema = LoadPattern(Resources.ContainerSchemaRelaxNg);
            profileSchema = LoadPattern(Resources.ProfileSchemaRelaxNg);
        }

        private static RelaxngPattern LoadPattern(string text)
        {
            using (var r = new StringReader(text))
            {
                RelaxngPattern p;
                using (var xr = XmlReader.Create(r))
                {
                    p = RelaxngPattern.Read(xr);
                }
                p.Compile();
                return p;
            }
        }

        private static XmlNamespaceManager namespaceManager;

        /// <summary>
        /// Gets a namespace manager to resolve namespaces in XPath Queries.
        /// </summary>
        /// <remarks>
        /// The prefix <c>c</c> is mapped to the container namespace 
        /// and the prefix <c>sig</c> is mapped to the W3C XML signature namespace.
        /// </remarks>
        /// <value>The namespace manager for processing XML in the container structure.</value>
        public static XmlNamespaceManager NamespaceManager
        {
            get
            {
                if (namespaceManager == null)
                {
                    var nt = new NameTable();
                    namespaceManager = new XmlNamespaceManager(nt);
                    namespaceManager.AddNamespace("c", ContainerNamespace);
                    namespaceManager.AddNamespace("p", ProfileNamespace);
                    namespaceManager.AddNamespace("sig", XMLSIGD_NS);
                    namespaceManager.PushScope();
                }
                return namespaceManager;
            }
        }
    }
}