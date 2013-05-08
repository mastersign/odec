using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace de.mastersign.odec.cli
{
    internal static class XmlHelper
    {
        private static XmlSchemaSet descriptionSchema;
        public static XmlSchemaSet DescriptionSchema
        {
            get
            {
                if (descriptionSchema == null)
                {
                    descriptionSchema = new XmlSchemaSet();
                    AddSchemaText(descriptionSchema, Resources.DescriptionSchema, null);
                    descriptionSchema.Compile();
                }
                return descriptionSchema;
            }
        }

        private static void AddSchemaText(XmlSchemaSet set, string text, ValidationEventHandler handler)
        {
            using (var r = new StringReader(text))
            {
                var s = XmlSchema.Read(r, handler);
                set.Add(s);
            }
        }

        public static bool LoadEntityDescription(string file, out XmlDocument doc, out string errMsg)
        {
            return LoadXmlDoc(file, out doc, out errMsg, "Entity");
        }

        public static bool LoadOwnerDescription(string file, out XmlDocument doc, out string errMsg)
        {
            return LoadXmlDoc(file, out doc, out errMsg, "Owner");
        }

        private static bool LoadXmlDoc(string file, out XmlDocument doc, out string errMsg, string documentElementName)
        {
            var resDoc = new XmlDocument();
            try
            {
                resDoc.Load(file);
            }
            catch (XmlException xmlEx)
            {
                doc = null;
                errMsg = string.Format(
                    Resources.XML_ParsingError,
                    file, xmlEx.Message);
                return false;
            }
            catch (FileNotFoundException fnfEx)
            {
                doc = null;
                errMsg = string.Format(
                    Resources.XML_FileNotFound,
                    file, fnfEx.Message);
                return false;
            }
            catch (IOException ioEx)
            {
                doc = null;
                errMsg = string.Format(
                    Resources.XML_ReadingError,
                    file, ioEx.Message);
                return false;
            }
            catch (UnauthorizedAccessException unauthEx)
            {
                doc = null;
                errMsg = string.Format(
                    Resources.XML_FileAccessDenied,
                    file, unauthEx.Message);
                return false;
            }

            var validationError = false;
            var validationMessageSb = new StringBuilder();

            resDoc.Schemas = DescriptionSchema;
            resDoc.Validate(
                (s, ea) =>
                {
                    validationError = true;
                    WriteValidationError(validationMessageSb, ea);
                });

            if (validationError)
            {
                doc = null;
                errMsg = string.Format(
                    Resources.XML_ValidationError,
                    file, Environment.NewLine, validationMessageSb);
                return false;
            }

            if (resDoc.DocumentElement.Name != documentElementName)
            {
                doc = null;
                errMsg = string.Format(
                    Resources.XML_UnexpectedRootElement,
                    file, documentElementName);
                return false;
            }

            doc = resDoc;
            errMsg = null;
            return true;
        }

        private static void WriteValidationError(StringBuilder sb, ValidationEventArgs ea)
        {
            sb.AppendFormat("[{0}] {1}{2}",
                            Enum.GetName(typeof(XmlSeverityType), ea.Severity),
                            ea.Message,
                            Environment.NewLine);
            //if (ea.Exception != null)
            //{
            //    sb.AppendLine("----");
            //    sb.AppendLine(ea.ToString());
            //}
            //sb.AppendLine();
        }

        public static string ReadString(this XmlNode node, string xpath)
        {
            var e = node.SelectSingleNode(xpath) as XmlElement;
            return e != null ? e.InnerText.Trim() : null;
        }

        public static bool ElementExists(this XmlNode node, string xpath)
        {
           return (node.SelectSingleNode(xpath) as XmlElement) != null;
        }

        public static int[] ReadIdList(this XmlNode node, string xpath)
        {
            var listString = node.ReadString(xpath);
            if (string.IsNullOrEmpty(listString)) return new int[0];
            return listString.Split(' ', '\t', '\n')
                .Select(t => int.Parse(t.Trim()))
                .ToArray();
        }
    }
}
