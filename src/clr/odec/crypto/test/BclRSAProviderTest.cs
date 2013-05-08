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
    internal class BclRSAProviderTest : AssertionHelper
    {
        [Test]
        public void CanComputeSignatureTest()
        {
            var target = new BclRSAProvider();
            Expect(!target.CanComputeSignature);
            target.Key = TestHelper.Key1;
            Expect(target.CanComputeSignature);
        }

        [Test]
        public void CanVerifySignatureTest()
        {
            var target = new BclRSAProvider();
            Expect(!target.CanVerifySignature);
            target.Certificate = TestHelper.Cert1;
            Expect(target.CanVerifySignature);
        }

        [Test]
        public void SignatureMethodTest()
        {
            var target = new BclRSAProvider();
            Assert.AreEqual(target.SignatureMethod, SignedXml.XmlDsigRSASHA1Url);
        }

        [Test]
        public void ComputeSignatureParamTest()
        {
            var target = new BclRSAProvider();
            Assert.Throws<ArgumentNullException>(
                () => target.ComputeSignature(null));
        }

        [Test]
        public void ComputeSignatureTest()
        {
            var target = new BclRSAProvider();
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                Assert.Throws<NotSupportedException>(
                    () => target.ComputeSignature(source));

                target.Key = TestHelper.Key1;
                source.Position = 0L;
                var sig1a = target.ComputeSignature(source);
                source.Position = 0L;
                var sig1b = target.ComputeSignature(source);
                Expect(sig1a, Is.EqualTo(sig1b));

                target.Key = TestHelper.Key2;
                source.Position = 0L;
                var sig2 = target.ComputeSignature(source);

                Expect(sig2, Is.Not.EqualTo(sig1a));
            }
        }

        [Test]
        public void VerifySignatureParamTest()
        {
            var target = new BclRSAProvider();

            Assert.Throws<ArgumentNullException>(
                () => target.VerifySignature(null, null));
            Assert.Throws<ArgumentNullException>(
                () => target.VerifySignature(null, new byte[20]));

            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                Assert.Throws<ArgumentNullException>(
                    () => target.VerifySignature(source, null));
            }
        }

        [Test]
        public void VerifySignatureTest()
        {
            var target = new BclRSAProvider();
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                Assert.Throws<NotSupportedException>(
                    () => target.VerifySignature(source, new byte[20]));
            }
        }

        [Test]
        public void ComputeSignatureAndVerifySignatureTest()
        {
            var target = new BclRSAProvider();
            target.Key = TestHelper.Key1;
            Expect(target.CanComputeSignature);

            byte[] signature;
            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                signature = target.ComputeSignature(source);
            }

            Expect(signature, !Null);

            target = new BclRSAProvider();
            target.Certificate = TestHelper.Cert1;
            Expect(target.CanVerifySignature);

            using (var source = TestHelper.GetResFile("DemoFile.xml"))
            {
                Expect(target.VerifySignature(source, signature), "Verification of computed signature failed (1st).");
                source.Position = 0L;
                Expect(target.VerifySignature(source, signature), "Verification of computed signature failed (2nd).");
                source.Position = 0L;
                Expect(target.VerifySignature(source, signature), "Verification of computed signature failed (3rd).");
            }
        }
    }
}