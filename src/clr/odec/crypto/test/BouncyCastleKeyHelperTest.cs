using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.odec.test;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    internal class BouncyCastleKeyHelperTest : AssertionHelper
    {
        [Test]
        public void LoadCertificateFileParamTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => BouncyCastleKeyHelper.LoadCertificateFile(null));

            var notExistingFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.Throws<FileNotFoundException>(
                () => BouncyCastleKeyHelper.LoadCertificateFile(notExistingFile));
        }

        [Test]
        public void LoadCertificateFileTest()
        {
            var file = TestHelper.GetResFilePath("Test.crt");
            var cert = BouncyCastleKeyHelper.LoadCertificateFile(file);
            Expect(cert, !Null);
        }

        [Test]
        public void LoadCertificateParamTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => BouncyCastleKeyHelper.LoadCertificate(null));

            Assert.Throws<ArgumentException>(
                () => BouncyCastleKeyHelper.LoadCertificate("No PEM encoded data here."));

            // check with incomplete certificate
            var file = TestHelper.GetResFilePath("Test.crt");
            var pemText = File.ReadAllText(file, Encoding.ASCII).Trim();
            var incompletePem = pemText.Substring(0, pemText.Length / 2);
            Assert.Throws<ArgumentException>(
                () => BouncyCastleKeyHelper.LoadCertificate(incompletePem));
        }

        [Test]
        public void LoadCertificateTest()
        {
            var file = TestHelper.GetResFilePath("Test.crt");
            var certText = File.ReadAllText(file, Encoding.ASCII).Trim();

            var cert = BouncyCastleKeyHelper.LoadCertificate(certText);
            Expect(cert, !Null);

            var certText2 = "ABCD\nEFGH\n\n1234\n" + certText;
            var cert2 = BouncyCastleKeyHelper.LoadCertificate(certText2);
            Expect(cert2, !Null);

            var certText3 = certText2 + "\nABCD\nEFGH\n\n1234\n";
            var cert3 = BouncyCastleKeyHelper.LoadCertificate(certText3);
            Expect(cert3, !Null);
        }

        [Test]
        public void LoadPrivateKeyFileParamTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => BouncyCastleKeyHelper.LoadPrivateKeyFile(null));

            string notExistingFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.Throws<FileNotFoundException>(
                () => BouncyCastleKeyHelper.LoadPrivateKeyFile(notExistingFile));
        }

        [Test]
        public void LoadPrivateKeyFileTest()
        {
            string file = TestHelper.GetResFilePath("Test.key");
            var key = BouncyCastleKeyHelper.LoadPrivateKeyFile(file);
            Expect(key, !Null);
        }

        [Test]
        public void LoadPrivateKeyParamTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => BouncyCastleKeyHelper.LoadPrivateKey(null));

            Assert.Throws<ArgumentException>(
                () => BouncyCastleKeyHelper.LoadPrivateKey("No PEM encoded data here."));

            // check with incomplete key
            var file = TestHelper.GetResFilePath("Test.key");
            var pemText = File.ReadAllText(file, Encoding.ASCII).Trim();
            var incompletePem = pemText.Substring(0, pemText.Length/2);
            Assert.Throws<ArgumentException>(
                () => BouncyCastleKeyHelper.LoadPrivateKey(incompletePem));
        }

        [Test]
        public void LoadPrivateKeyTest()
        {
            var file = TestHelper.GetResFilePath("Test.key");
            var keyText = File.ReadAllText(file, Encoding.ASCII).Trim();

            var key = BouncyCastleKeyHelper.LoadPrivateKey(keyText);
            Expect(key, !Null);

            var keyText2 = "ABCD\nEFGH\n\n1234\n" + keyText;
            var key2 = BouncyCastleKeyHelper.LoadPrivateKey(keyText2);
            Expect(key2, !Null);

            var keyText3 = keyText2 + "\nABCD\nEFGH\n\n1234\n";
            var key3 = BouncyCastleKeyHelper.LoadPrivateKey(keyText3);
            Expect(key3, !Null);
        }

    }
}