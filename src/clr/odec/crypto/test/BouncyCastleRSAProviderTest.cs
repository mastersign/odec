using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using de.mastersign.odec.test;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    internal class BouncyCastleRSAProviderTest : AssertionHelper
    {
        [Test]
        public void CanComputeSignatureTest()
        {
            var target = new BouncyCastleRSAProvider();
            Expect(!target.CanComputeSignature);

            target.Certificate = BouncyCastleKeyHelper.LoadCertificateFile(
                TestHelper.GetResFilePath("Test.crt"));
            Expect(!target.CanComputeSignature);

            target.PrivateKey = BouncyCastleKeyHelper.LoadPrivateKeyFile(
                TestHelper.GetResFilePath("Test.key"));
            Expect(target.CanComputeSignature);

            target.Certificate = null;
            Expect(target.CanComputeSignature);
        }

        [Test]
        public void CanVerifySignatureTest()
        {
            var target = new BouncyCastleRSAProvider();
            Expect(!target.CanVerifySignature);

            target.PrivateKey = BouncyCastleKeyHelper.LoadPrivateKeyFile(
                TestHelper.GetResFilePath("Test.key"));
            Expect(!target.CanVerifySignature);

            target.Certificate = BouncyCastleKeyHelper.LoadCertificateFile(
                TestHelper.GetResFilePath("Test.crt"));
            Expect(target.CanVerifySignature);

            target.PrivateKey = null;
            Expect(target.CanVerifySignature);
        }

        [Test]
        public void SignatureMethodTest()
        {
            var target = new BouncyCastleRSAProvider();
            Expect(
                target.SignatureMethod,
                Is.EqualTo(SignedXml.XmlDsigRSASHA1Url));
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void ComputeSignatureParamTest1()
        {
            var target = new BouncyCastleRSAProvider();
            target.ComputeSignature(null);
        }

        [Test, ExpectedException(typeof (NotSupportedException))]
        public void ComputeSignatureTest()
        {
            var target = new BouncyCastleRSAProvider();
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                target.ComputeSignature(source);
            }
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void VerifySignatureParamTest1()
        {
            var target = new BouncyCastleRSAProvider();
            target.VerifySignature(null, null);
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void VerifySignatureParamTest2()
        {
            var target = new BouncyCastleRSAProvider();
            target.VerifySignature(null, new byte[20]);
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void VerifySignatureParamTest3()
        {
            var target = new BouncyCastleRSAProvider();
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                target.VerifySignature(source, null);
            }
        }

        [Test, ExpectedException(typeof (NotSupportedException))]
        public void VerifySignatureTest()
        {
            var target = new BouncyCastleRSAProvider();
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                target.VerifySignature(source, new byte[20]);
            }
        }

        [Test]
        public void ComputeSignatureAndVerifySignatureTest()
        {
            var target = new BouncyCastleRSAProvider();
            target.PrivateKey = BouncyCastleKeyHelper.LoadPrivateKeyFile(
                TestHelper.GetResFilePath("Test.key"));

            byte[] signature;
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                signature = target.ComputeSignature(source);
                Expect(signature, !Null);
            }

            target.Certificate = BouncyCastleKeyHelper.LoadCertificateFile(
                TestHelper.GetResFilePath("Test.crt"));
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                Expect(target.VerifySignature(source, signature),
                       "Verification of computed signature failed.");
            }
        }
    }
}