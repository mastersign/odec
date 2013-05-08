using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using de.mastersign.odec.crypto;
using de.mastersign.odec.model;
using NUnit.Framework;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.test
{
    internal static class TestHelper
    {
        //public static readonly X509Certificate2 CA;
        public static readonly X509Certificate2 Cert1;
        public static readonly X509Certificate2 Cert2;

        public static readonly RSAParameters Key1;
        public static readonly RSAParameters Key2;

        static TestHelper()
        {
            //CA = BclKeyHelper.LoadCertificate(GetResFilePath("ca.crt"));
            Cert1 = BclKeyHelper.LoadCertificateFile(GetResFilePath("Test.crt"));
            Key1 = BclKeyHelper.LoadPrivateKeyFile(GetResFilePath("Test.key"));
            Cert2 = BclKeyHelper.LoadCertificateFile(GetResFilePath("Test2.crt"));
            Key2 = BclKeyHelper.LoadPrivateKeyFile(GetResFilePath("Test2.key"));
        }

        public static string GetResFilePath(string fileName)
        {
            var codeBaseUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var basePath = Path.GetDirectoryName(codeBaseUri.LocalPath);
            var result = Path.Combine(Path.Combine(basePath, "test\\res"), fileName);
            return result;
        }

        public static Uri GetResFileUri(string fileName)
        {
            return new Uri(GetResFilePath(fileName));
        }

        public static Stream GetResFile(string fileName)
        {
            return File.OpenRead(GetResFilePath(fileName));
        }

        public static string CreateTempRandomFile(int sizeInKb)
        {
            var path = Path.GetTempFileName();
            var buffer = new byte[1024];
            var rand = new Random();

            using (var fs = File.OpenWrite(path))
            {
                for (int i = 0; i < sizeInKb; i++)
                {
                    rand.NextBytes(buffer);
                    fs.Write(buffer, 0, buffer.Length);
                }
            }
            return path;
        }

        public static byte[] HexToByteArray(string value)
        {
            value = value.Trim().Replace(":", "").Replace(" ", "").Replace("-", "");
            if (value.Length % 2 != 0)
            {
                throw new ArgumentException("Length of hex places is not even.");
            }
            var result = new byte[value.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return result;
        }

        public static T CopyByXmlSerialization<T>(T original)
            where T : class, IXmlStorable, new()
        {
            var sb = new StringBuilder();

            var ws = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineHandling = NewLineHandling.Entitize };
            using (var w = XmlWriter.Create(sb, ws))
            {
                w.WriteStartDocument();
                w.WriteObject("ParentElement", null, original);
                w.WriteEndDocument();
            }
            Console.WriteLine(sb.ToString());

            var doc = new XmlDocument();
            doc.LoadXml(sb.ToString());

            return doc.ReadObject<T>("ParentElement", null);
        }

        public static void CheckIXmlStorable<T>(params T[] testobjects)
            where T : class, IXmlStorable, IEquatable<T>, new()
        {
            foreach (var original in testobjects)
            {
                if (original == null)
                {
                    throw new ArgumentException("An item of the given test object array is null.");
                }

                var copy1 = CopyByXmlSerialization(original);
                Assert.IsNotNull(copy1, "Copying by XML serialization failed, returned object is null.");
                Assert.IsTrue(copy1.IsValid);
                Assert.IsTrue(copy1.Equals(original));

                var copy2 = CopyByXmlSerialization(copy1);
                Assert.IsNotNull(copy2, "Copying (2) by XML serialization failed, returned object is null.");
                Assert.IsTrue(copy2.IsValid);
                Assert.IsTrue(copy2.Equals(copy1));
                Assert.IsTrue(copy2.Equals(original));
            }
        }

        public static void CheckSchemaConformity(string elementName, string ns, IXmlStorable target)
        {
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb,
                                            new XmlWriterSettings { Indent = true, IndentChars = "  " }))
            {
                w.WriteStartDocument();
                w.WriteObject(elementName, ns, target);
                w.WriteEndDocument();
            }
            Console.WriteLine(sb.ToString());

            var doc = new XmlDocument();
            doc.LoadXml(sb.ToString());

            string errMsg;
            var valid = doc.IsSchemaConform(out errMsg);

            if (errMsg != null) Console.WriteLine(errMsg);
            Assert.IsTrue(valid);
        }

        #region Factories

        public static SignatureWrapper BuildSignature(string resFile, RSAParameters key)
        {
            var cryptoProv = new BclRSAProvider { Key = key };
            var hashProv = new BclHashProvider();
            var canonicalizer = new BclC14NCanonicalizer();
            var sig = new SignatureWrapper();
            sig.BuildSignature(resFile, GetResFile(resFile), hashProv, cryptoProv, canonicalizer);
            Assert.IsTrue(sig.IsValid);
            return sig;
        }

        public static Owner CreateOwnerType(int no)
        {
            var result =
                new Owner
                    {
                        Institute = string.Format("Test institute {0}", no),
                        Operator = string.Format("Mr. Smith-{0}", no),
                        Role = string.Format("expert-{0}", no),
                        Email = string.Format("test@server-{0}.com", no),
                        X509Certificate = string.Format("0123456789{0}", no),
                    };
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static EditionElement CreateEditionElement(RSAParameters key, int no, int newEntityCnt, int removedEntityCnt)
        {
            var result =
                new EditionElement
                    {
                        Guid = new Guid("d782d9ee-dddd-4a57-89dd-c631e5a1aca3"),
                        Software = string.Format("Test Software {0}", no),
                        Profile = "Test Profile",
                        Version = "1.2.3.4000b",
                        Timestamp = new DateTime(1997, 8, 21, 15, 22, 18, no),
                        Owner = CreateOwnerType(no),
                        Copyright = string.Format("Test \x00A9 Copyright {0}", no),
                        Comments = string.Format("Test comments\nwith multiple\nlines {0}.", no),
                        HistorySignature = BuildSignature("DemoHistory.xml", key),
                        IndexSignature = BuildSignature("DemoIndex.xml", key),
                    };
            for (int i = 0; i < removedEntityCnt; i++)
            {
                result.RemovedEntities.Add(i);
            }
            for (int i = 0; i < newEntityCnt; i++)
            {
                result.NewEntities.Add(removedEntityCnt + i);
            }
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static HistoryItemElement CreateHistoryItemElement(RSAParameters key, int no)
        {
            var result =
                new HistoryItemElement
                    {
                        Edition = CreateEditionElement(key, no, no, no),
                        PastMasterSignature = BuildSignature("DemoFile.xml", key),
                    };
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static HistoryElement CreateHistoryElement(RSAParameters key1, int itemCount)
        {
            var result = new HistoryElement();
            for (int i = 0; i < itemCount; i++)
            {
                result.Push(CreateHistoryItemElement(key1, i));
            }
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static IndexItemElement CreateIndexItemElement(RSAParameters key, int id, int successorCount)
        {
            var result = new IndexItemElement
            {
                Id = id,
                Label = id != 2 ? "label_" + id : null,
                EntitySignature = BuildSignature("DemoFile.xml", key)
            };
            for (int i = id + 1; i <= id + successorCount; i++)
            {
                result.Successors.Add(i);
            }
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static IndexElement CreateIndexElement(RSAParameters key, int count)
        {
            var result = new IndexElement();
            for (int i = 0; i < count; i++)
            {
                result.Add(CreateIndexItemElement(key, result.GetNextEntityId(), count - i - 1));
            }
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static ValueReference CreateValueReference(RSAParameters key, int no)
        {
            var result = new ValueReference();
            result.Name = string.Format("ValueName_{0}.bin", no);
            result.Type = new Guid(no, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
            result.Size = no * 1024;
            result.Appearance = ValueAppearance.plain;
            result.ValueSignature = BuildSignature("DemoFile.xml", key);
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static ProvenanceElement CreateProvenanceElement(int no)
        {
            var result = new ProvenanceElement();
            result.Guid = new Guid(no, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
            Assert.IsTrue(result.IsValid);
            return result;
        }

        public static EntityElement CreateEntityElement(RSAParameters key, int id, int valueCount, bool parameterSet)
        {
            var result = new EntityElement();
            result.Id = id;
            result.Label = id != 2 ? "label_" + id : null;
            result.Type = new Guid(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);
            result.Provenance = CreateProvenanceElement(id);
            if (parameterSet) result.ParameterSet = CreateValueReference(key, id);
            for (int i = 0; i < valueCount; i++)
            {
                result.Values.Add(CreateValueReference(key, i));
            }
            for (int i = 1; i < id; i++)
            {
                result.Predecessors.Add(i);
            }
            Assert.IsTrue(result.IsValid);
            return result;
        }

        #endregion

    }
}