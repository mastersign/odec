using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography.Xml;
using de.mastersign.odec.crypto;
using de.mastersign.odec.test;
using de.mastersign.odec.test.res;
using NUnit.Framework;

namespace de.mastersign.odec.model.test
{
    [TestFixture]
    internal class SignatureWrapperTest : AssertionHelper
    {
        private readonly byte[] testByteArray = new byte[]
                                                    {
                                                        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xa,
                                                        0xb, 0xc, 0xd, 0xe, 0xf
                                                    };

        private XmlElement signatureParentElement;
        private Signature testSignature;

        [SetUp]
        public void Initialize()
        {
            var doc = new XmlDocument();
            using (var exampleFileStream = TestHelper.GetResFile("SignatureWrapperTest.xml"))
            {
                doc.Load(exampleFileStream);
            }
            signatureParentElement = doc.SelectSingleNode("//DemoSignature") as XmlElement;

            testSignature = new Signature();
            var si = new SignedInfo();
            si.AddReference(new Reference
                                {
                                    DigestMethod = AlgorithmIdentifier.SHA1,
                                    DigestValue = testByteArray,
                                    Uri = "DemoFile.xml"
                                });
            si.SignatureMethod = SignedXml.XmlDsigDSAUrl;
            si.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
            testSignature.SignedInfo = si;
            testSignature.SignatureValue = testByteArray;
        }

        private void CheckSignature(Signature s)
        {
            Expect(s, !Null);

            var si = s.SignedInfo;
            Expect(si, !Null);
            Expect(si.SignatureMethod,
                   Is.EqualTo(SignedXml.XmlDsigDSAUrl));
            Expect(si.CanonicalizationMethod,
                   Is.EqualTo(SignedXml.XmlDsigCanonicalizationUrl));
            Expect(s.SignedInfo.References.Count,
                   Is.EqualTo(1));

            var r = s.SignedInfo.References[0] as Reference;
            Expect(r, !Null);
            Expect(r.Uri,
                   Is.EqualTo("DemoFile.xml"));
            Expect(r.DigestMethod,
                   Is.EqualTo(AlgorithmIdentifier.SHA1));
            Expect(r.DigestValue,
                   Is.EqualTo(testByteArray));
            Expect(s.SignatureValue,
                   Is.EqualTo(testByteArray));
        }

        [Test]
        public void ReadFromXmlParamTest()
        {
            var target = new SignatureWrapper();
            var emptyElement = signatureParentElement.OwnerDocument.CreateElement("test");

            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromXml(null));

            Assert.Throws<ArgumentException>(
                () => target.ReadFromXml(emptyElement));
        }

        [Test]
        public void ReadFromXmlTest()
        {
            var target = new SignatureWrapper();
            Expect(target.Signature, Null);

            target.ReadFromXml(signatureParentElement);
            CheckSignature(target.Signature);
        }

        [Test]
        public void WriteToXmlParamTest()
        {
            var target = new SignatureWrapper();

            Assert.Throws<ArgumentNullException>(
                () => target.WriteToXml(null));

            var sb = new StringBuilder();
            using (var wr = XmlWriter.Create(sb))
            {
                Assert.Throws<InvalidOperationException>(
                    () => target.WriteToXml(wr));
            }
        }

        [Test]
        public void WriteToXmlTest()
        {
            var target = new SignatureWrapper();
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb))
            {
                Expect(!target.IsValid);
                Assert.Throws<InvalidOperationException>(
                    () => target.WriteToXml(w));
            }
        }

        [Test]
        public void ReadFromXmlWriteToXmlTest()
        {
            var target = new SignatureWrapper();
            var sb = new StringBuilder();

            target.Signature = testSignature;
            using (var wr = XmlWriter.Create(sb))
            {
                wr.WriteStartDocument();
                wr.WriteStartElement("DemoSignature");
                target.WriteToXml(wr);
                wr.WriteEndElement();
                wr.WriteEndDocument();
            }

            var doc = new XmlDocument();
            doc.LoadXml(sb.ToString());

            var sig = new Signature();
            sig.LoadXml(doc.SelectSingleNode("//DemoSignature/sig:Signature", Model.NamespaceManager) as XmlElement);
            CheckSignature(sig);
        }

        [Test]
        public void BuildSignatureParamTest1()
        {
            var target = new SignatureWrapper();

            var uri = TestHelper.GetResFileUri("DemoFile.xml").ToString();
            var digest = DemoFileHashes.SHA1;
            var method = AlgorithmIdentifier.SHA1;
            var cryptoProv = new BclRSAProvider { Key = TestHelper.Key1 };
            var canonicalizer = new BclC14NCanonicalizer();

            Assert.Throws<ArgumentNullException>(
                () => target.BuildSignature(null, digest, method, cryptoProv, canonicalizer));
            Assert.Throws<ArgumentNullException>(
                () => target.BuildSignature(uri, null, method, cryptoProv, canonicalizer));
            Assert.Throws<ArgumentNullException>(
                () => target.BuildSignature(uri, digest, null, cryptoProv, canonicalizer));
            Assert.Throws<ArgumentNullException>(
                () => target.BuildSignature(uri, digest, method, null, canonicalizer));
            Assert.Throws<ArgumentNullException>(
                () => target.BuildSignature(uri, digest, method, cryptoProv, null));
        }

        [Test]
        public void BuildSignatureTest1()
        {
            var target = new SignatureWrapper();
            Expect(target.Signature, Null);

            var uri = TestHelper.GetResFileUri("DemoFile.xml").ToString();
            var digest = DemoFileHashes.SHA1;
            var method = AlgorithmIdentifier.SHA1;
            var cryptoProv = new BclRSAProvider { Key = TestHelper.Key1 };
            var canonicalizer = new BclC14NCanonicalizer();

            target.BuildSignature(uri, digest, method, cryptoProv, canonicalizer);

            var sig = target.Signature;
            Expect(sig, !Null);
            Expect(sig.SignatureValue, !Null);
            var si = sig.SignedInfo;
            Expect(si, !Null);
            Expect(si.CanonicalizationMethod, Is.EqualTo(canonicalizer.CanonicalizationMethod));
            Expect(si.SignatureMethod, Is.EqualTo(cryptoProv.SignatureMethod));
            Expect(si.References.Count == 1);
            var r = si.References[0] as Reference;
            Expect(r, !Null);
            Expect(r.DigestMethod, Is.EqualTo(method));
            Expect(r.Uri, Is.EqualTo(uri));
            Expect(r.DigestValue, Is.EqualTo(digest));
        }

        [Test]
        public void BuildSignatureParamTest2()
        {
            var target = new SignatureWrapper();
            using (var stream = TestHelper.GetResFile("DemoFile.xml"))
            {
                var uri = TestHelper.GetResFileUri("DemoFile.xml").ToString();
                var hashProv = new BclHashProvider();
                var cryptoProv = new BclRSAProvider { Key = TestHelper.Key1 };
                var canonicalizer = new BclC14NCanonicalizer();

                Assert.Throws<ArgumentNullException>(
                    () => target.BuildSignature(null, stream, hashProv, cryptoProv, canonicalizer));
                Assert.Throws<ArgumentNullException>(
                    () => target.BuildSignature(uri, null, hashProv, cryptoProv, canonicalizer));
                Assert.Throws<ArgumentNullException>(
                    () => target.BuildSignature(uri, stream, null, cryptoProv, canonicalizer));
                Assert.Throws<ArgumentNullException>(
                    () => target.BuildSignature(uri, stream, hashProv, null, canonicalizer));
                Assert.Throws<ArgumentNullException>(
                    () => target.BuildSignature(uri, stream, hashProv, cryptoProv, null));
            }
        }

        [Test]
        public void BuildSignatureTest2()
        {
            var target = new SignatureWrapper();
            Expect(target.Signature, Null);

            var uri = TestHelper.GetResFileUri("DemoFile.xml").ToString();
            var hashProv = new BclHashProvider();

            var cryptoProv = new BclRSAProvider { Key = TestHelper.Key1 };
            var canonicalizer = new BclC14NCanonicalizer();

            using (var stream = TestHelper.GetResFile("DemoFile.xml"))
            {
                target.BuildSignature(uri, stream, hashProv, cryptoProv, canonicalizer);
            }

            var sig = target.Signature;
            Expect(sig, !Null);
            Expect(sig.SignatureValue, !Null);
            var si = sig.SignedInfo;
            Expect(si, !Null);
            Expect(si.CanonicalizationMethod, Is.EqualTo(canonicalizer.CanonicalizationMethod));
            Expect(si.SignatureMethod, Is.EqualTo(cryptoProv.SignatureMethod));
            Expect(si.References.Count == 1);
            var r = si.References[0] as Reference;
            Expect(r, !Null);
            Expect(hashProv.GetSupportedMethods(), Contains(r.DigestMethod));
            Expect(r.Uri, Is.EqualTo(uri));
            Expect(r.DigestValue, !Null);
            Expect(r.DigestValue.Length > 0);
        }

        [Test]
        public void VerifyDigestParamTest()
        {
            var target = new SignatureWrapper();
            var uri = "DemoFile.xml";
            var digest = DemoFileHashes.SHA1;
            var method = AlgorithmIdentifier.SHA1;
            var stream = new MemoryStream();
            var hashProv = new BclHashProvider();

            Assert.Throws<ArgumentNullException>(
                () => target.VerifyDigest(null, stream, hashProv));
            Assert.Throws<ArgumentNullException>(
                () => target.VerifyDigest(uri, null, hashProv));
            Assert.Throws<ArgumentNullException>(
                () => target.VerifyDigest(uri, stream, null));

            Assert.Throws<ArgumentNullException>(
                () => target.VerifyDigest(null, digest, method));
            Assert.Throws<ArgumentNullException>(
                () => target.VerifyDigest(uri, null, method));
            Assert.Throws<ArgumentNullException>(
                () => target.VerifyDigest(uri, digest, null));
        }

        [Test]
        public void VerifyDigestTest()
        {
            var target = new SignatureWrapper();
            Expect(target.Signature, Null);

            var uri = TestHelper.GetResFileUri("DemoFile.xml").ToString();
            var digest = DemoFileHashes.SHA1;
            var method = AlgorithmIdentifier.SHA1;
            var cryptoProv = new BclRSAProvider { Key = TestHelper.Key1 };
            var hashProv = new BclHashProvider();
            var canonicalizer = new BclC14NCanonicalizer();

            var results = new List<ValidationSeverity>();
            var handler = new ValidationHandler(ae =>
            {
                Console.WriteLine(ae);
                results.Add(ae.Severity);
            });

            target.BuildSignature(uri, (byte[])digest.Clone(), method, cryptoProv, canonicalizer);
            Expect(target.Signature, !Null);

            // Positive check with given digest
            Expect(target.VerifyDigest(uri, digest, method, "DemoFile.xml", handler));
            Expect(results, Contains(ValidationSeverity.Success));
            Expect(results, Not.Contains(ValidationSeverity.Error));

            results.Clear();
            // Negative check with given digest
            ((Reference)target.Signature.SignedInfo.References[0]).DigestValue[0] ^= 7; // Manipulate data digest
            Expect(!target.VerifyDigest(uri, digest, method, "DemoFile.xml", handler));
            Expect(results, Contains(ValidationSeverity.Error));

            ((Reference)target.Signature.SignedInfo.References[0]).DigestValue[0] ^= 7; // Revert change to data digest
            results.Clear();

            // Positive check with data stream
            using (var s = TestHelper.GetResFile("DemoFile.xml"))
            {
                Expect(target.VerifyDigest(uri, s, hashProv, "DemoFile.xml", handler));
            }
            Expect(results, Contains(ValidationSeverity.Success));
            Expect(results, Not.Contains(ValidationSeverity.Error));

            results.Clear();
            // Negative check with data stream
            ((Reference)target.Signature.SignedInfo.References[0]).DigestValue[0] ^= 7; // Manipulate data digest
            using (var s = TestHelper.GetResFile("DemoFile.xml"))
            {
                Expect(!target.VerifyDigest(uri, s, hashProv, "DemoFile.xml", handler));
            }
            Expect(results, Contains(ValidationSeverity.Error));
        }

        [Test]
        public void VerifySignatureParamTest()
        {
            var target = new SignatureWrapper();
            var hashProv = new BclHashProvider();
            var cryptoProv = new BclRSAProvider { Key = TestHelper.Key1 };
            var canonicalizer = new BclC14NCanonicalizer();

            Assert.Throws<ArgumentNullException>(
                () => target.VerifySignature(null, cryptoProv, canonicalizer));
            Assert.Throws<ArgumentNullException>(
                () => target.VerifySignature(hashProv, null, canonicalizer));
            Assert.Throws<ArgumentNullException>(
                () => target.VerifySignature(hashProv, cryptoProv, null));
        }

        [Test]
        public void VerifySignatureTest()
        {
            var target = new SignatureWrapper();
            Expect(target.Signature, Null);

            var uri = TestHelper.GetResFileUri("DemoFile.xml").ToString();
            var digest = DemoFileHashes.SHA1;
            var method = AlgorithmIdentifier.SHA1;
            var cryptoProvSign = new BclRSAProvider { Key = TestHelper.Key1 };
            var cryptoProvVerify = new BclRSAProvider { Certificate = TestHelper.Cert1 };
            var hashProv = new BclHashProvider();
            var canonicalizer = new BclC14NCanonicalizer();

            var results = new List<ValidationSeverity>();
            var handler = new ValidationHandler(ae => 
            {
                    Console.WriteLine(ae);
                    results.Add(ae.Severity);
            });

            target.BuildSignature(uri, digest, method, cryptoProvSign, canonicalizer);
            Expect(target.Signature, !Null);

            // Positive check
            Expect(target.VerifySignature(hashProv, cryptoProvVerify, canonicalizer, 
                "DemoFile.xml", handler));
            Expect(results, Contains(ValidationSeverity.Success));
            Expect(results, Not.Contains(ValidationSeverity.Error));

            results.Clear();
            // Negative check
            target.Signature.SignatureValue[0] ^= 7; // Manipulate signature value
            Expect(!target.VerifySignature(hashProv, cryptoProvVerify, canonicalizer,
                "DemoFile.xml", handler));
            Expect(results, Contains(ValidationSeverity.Error));
        }

        [Test]
        public void IsValidTest()
        {
            var target = new SignatureWrapper();
            Expect(!target.IsValid);

            target = TestHelper.BuildSignature("DemoFile.xml", TestHelper.Key1);
            Expect(target.IsValid);
        }

        [Test]
        public void EqualsAndGetHashCodeTest()
        {
            var key1 = TestHelper.Key1;
            var key2 = TestHelper.Key2;

            var target1 = new SignatureWrapper();
            var target2 = TestHelper.BuildSignature("DemoFile.xml", key1);
            var target3 = TestHelper.BuildSignature("DemoFile.xml", key1);
            var target4 = TestHelper.BuildSignature("DemoFile.xml", key2);

            var hash1 = target1.GetHashCode();
            var hash2 = target2.GetHashCode();
            var hash3 = target3.GetHashCode();
            var hash4 = target4.GetHashCode();

            Expect(hash1, Is.Not.EqualTo(hash2));
            Expect(hash1, Is.Not.EqualTo(hash3));
            Expect(hash1, Is.Not.EqualTo(hash4));
            Expect(target1, Is.Not.EqualTo(target2));
            Expect(target1, Is.Not.EqualTo(target3));
            Expect(target1, Is.Not.EqualTo(target4));

            Expect(hash2, Is.EqualTo(hash3));
            Expect(target2, Is.EqualTo(target3));

            Expect(hash3, Is.Not.EqualTo(hash4));
            Expect(target3, Is.Not.EqualTo(target4));
        }

        [Test]
        public void WriteToStreamParamTest()
        {
            var target1 = new SignatureWrapper();
            using (var ms = new MemoryStream())
            {
                Assert.Throws<InvalidOperationException>(
                    () => target1.WriteToStream(ms, false));
            }

            var target2 = TestHelper.BuildSignature("DemoFile.xml", TestHelper.Key1);
            Assert.Throws<ArgumentNullException>(
                () => target2.WriteToStream(null, false));
        }

        [Test]
        public void ReadFromStreamParamTest()
        {
            var target = new SignatureWrapper();
            Assert.Throws<ArgumentNullException>(
                () => target.ReadFromStream(null));
        }

        [Test]
        public void StreamSerializationTest()
        {
            var target = TestHelper.BuildSignature("DemoFile.xml", TestHelper.Key1);

            using (var ms = new MemoryStream())
            {
                target.WriteToStream(ms, false);

                ms.Position = 0L;

                var target2 = new SignatureWrapper();
                target2.ReadFromStream(ms);

                Expect(target2, Is.EqualTo(target));
            }
        }
    }
}