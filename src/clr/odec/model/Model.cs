using System.IO;
using System.Xml;
using System.Xml.Schema;
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

        private static XmlSchema xmldsigSchema;
        private static XmlSchema containerSchema;
        private static XmlSchema profileSchema;

        /// <summary>
        /// Gets the target namespace of the container XML-Schema.
        /// </summary>
        /// <value>The container namespace.</value>
        public static string ContainerNamespace
        {
            get
            {
                if (containerSchema == null) LoadSchemas();
                return containerSchema.TargetNamespace;
            }
        }

        /// <summary>
        /// Gets the target namespace of the profile XML-Schema.
        /// </summary>
        /// <value>The profile namespace.</value>
        public static string ProfileNamespace
        {
            get
            {
                if (profileSchema == null) LoadSchemas();
                return profileSchema.TargetNamespace;
            }
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

        private static XmlSchemaSet schemaSet;

        /// <summary>
        /// Gets a compiled version of the container XML-Schema.
        /// </summary>
        /// <value>The schema.</value>
        public static XmlSchemaSet Schema
        {
            get
            {
                if (schemaSet == null) LoadSchemas();
                return schemaSet;
            }
        }

        private static void LoadSchemas()
        {
            schemaSet = new XmlSchemaSet();
            xmldsigSchema = schemaSet.AddSchemaText(Resources.XmldsigCoreSchema, null);
            containerSchema = schemaSet.AddSchemaText(Resources.ContainerSchema, null);
            profileSchema = schemaSet.AddSchemaText(Resources.ProfileSchema, null);
            schemaSet.Compile();
        }

        private static XmlSchema AddSchemaText(this XmlSchemaSet set, string text, ValidationEventHandler handler)
        {
            using (var r = new StringReader(text))
            {
                XmlSchema s;
                using (var xr = XmlReader.Create(r))
                {
                    s = XmlSchema.Read(xr, handler);
                }
                set.Add(s);
                return s;
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