﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Commons.Xml.Relaxng;
using de.mastersign.odec.Properties;
using de.mastersign.odec.model;

namespace de.mastersign.odec.utils
{
    internal static class XmlUtils
    {
        public static bool ElementExists(this XmlNode parentNode, string name)
        {
            return (parentNode.SelectSingleNode(name, Model.NamespaceManager) as XmlElement) != null;
        }

        public static string ReadElementString(this XmlNode parentNode, string name, string def)
        {
            var textElement = parentNode.SelectSingleNode(name, Model.NamespaceManager) as XmlElement;
            if (textElement == null)
            {
                return def;
            }
            var value = textElement.InnerText;
            return value.Length > 0 ? value : def;
        }

        public static T ParseValue<T>(string text, T def, Func<string, T> parser)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (parser == null) throw new ArgumentNullException("parser");
            try
            {
                return parser(text);
            }
            catch (Exception)
            {
                return def;
            }
        }

        public static T ReadParsedObject<T>(this XmlNode parentNode, string name, T def, Func<string, T> parser)
        {
            if (parentNode == null) throw new ArgumentNullException("parentNode");
            if (name == null) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name.Trim())) throw new ArgumentException(Resources.XmlUtils_ReadParsedObject_ArgumentException_NameIsEmpty, "name");
            if (parser == null) throw new ArgumentNullException("parser");

            var textElement = parentNode.SelectSingleNode(name, Model.NamespaceManager) as XmlElement;
            if (textElement == null)
            {
                return def;
            }
            return ParseValue(textElement.InnerText.Trim(), def, parser);
        }

        public static void WriteObject(this XmlWriter w, string parentName, string ns, IXmlStorable o)
        {
            w.WriteStartElement(parentName, ns);
            o.WriteToXml(w);
            w.WriteEndElement();
        }

        public static void WriteObject(this XmlWriter w, string parentName, string ns, IXmlStorable2 o, bool canonicalizedSignatures)
        {
            w.WriteStartElement(parentName, ns);
            o.WriteToXml(w, canonicalizedSignatures);
            w.WriteEndElement();
        }

        public static T ReadObject<T>(this XmlNode parentNode, string name, T o)
            where T : class, IXmlStorable, new()
        {
            var objectElement = parentNode.SelectSingleNode(name, Model.NamespaceManager) as XmlElement;
            if (objectElement != null)
            {
                if (o == null) o = new T();
                o.ReadFromXml(objectElement);
            }
            return o;
        }

        public static IEnumerable<T> ReadObjects<T>(this XmlNode parentNode, string name)
            where T : class, IXmlStorable, new()
        {
            var nodes = parentNode.SelectNodes(name, Model.NamespaceManager);
            if (nodes == null) yield break;
            foreach (XmlElement e in nodes)
            {
                var o = new T();
                o.ReadFromXml(e);
                yield return o;
            }
        }

        public static bool IsSchemaConform(this XmlDocument doc, out string errorMessage)
        {
            var error = false;
            var sb = new StringBuilder();

            var xmlText = doc.OuterXml;

            error = IsSchemaConform(xmlText, sb, Model.ContainerSchema) &&
                IsSchemaConform(xmlText, sb, Model.ProfileSchema) &&
                IsSchemaConform(xmlText, sb, Model.XmlSignatureSchema);

            errorMessage = error ? sb.ToString() : null;
            return !error;
        }

        private static bool IsSchemaConform(string xmlText, StringBuilder messageBuffer, RelaxngPattern pattern)
        {
            var error = false;
            using (var r = new StringReader(xmlText))
            using (var xr = XmlReader.Create(r))
            using (var vr = new RelaxngValidatingReader(xr, pattern))
            {
                vr.InvalidNodeFound += (s, msg) => { messageBuffer.AppendLine(msg); error = true; return false; };
                var tmpDoc = new XmlDocument();
                tmpDoc.Load(vr);
            }
            return error;
        }

        public class LoggingXmlResolver : XmlResolver
        {
            private readonly XmlResolver innerResolver;

            public LoggingXmlResolver(XmlResolver innerResolver)
            {
                this.innerResolver = innerResolver;
            }

            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                Console.Out.WriteLine("### uri={0}, relative={1}", baseUri, relativeUri);
                Console.Out.Flush();
                return innerResolver.ResolveUri(baseUri, relativeUri);
            }

            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                Console.Out.WriteLine("### uri={0}, role={1}, type={2}", absoluteUri, role, ofObjectToReturn);
                Console.Out.Flush();
                return innerResolver.GetEntity(absoluteUri, role, ofObjectToReturn);
            }

            public override ICredentials Credentials
            {
                set { innerResolver.Credentials = value; }
            }
        }
    }
}